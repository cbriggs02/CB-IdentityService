﻿using IdentityServiceApi.Constants;
using IdentityServiceApi.Data;
using IdentityServiceApi.Interfaces.Logging;
using IdentityServiceApi.Interfaces.Utilities;
using IdentityServiceApi.Models.DTO;
using IdentityServiceApi.Models.Entities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using IdentityServiceApi.Models.Shared;
using IdentityServiceApi.Models.ServiceResultModels.Logging;
using IdentityServiceApi.Models.ServiceResultModels.Shared;
using IdentityServiceApi.Models.RequestModels.Logging;

namespace IdentityServiceApi.Services.Logging
{
    /// <summary>
    ///     Service responsible for interacting with audit-log-related data and business logic.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class AuditLoggerService : IAuditLoggerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditLoggerServiceResultFactory _auditLoggerServiceResultFactory;
        private readonly IParameterValidator _parameterValidator;
        private readonly IMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AuditLoggerService"/> class with the provided
        ///     application database context.
        /// </summary>
        /// <param name="context">
        ///     The database context used for accessing audit logs.
        /// </param>
        /// <param name="parameterValidator">
        ///     The parameter validator service used for defense checking service parameters.
        /// </param>
        /// <param name="mapper">
        ///     Object mapper for converting between entities and data transfer objects (DTOs).
        /// </param>
        /// <param name="auditLoggerServiceResultFactory">
        ///     The service used for creating the result objects being returned in operations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the context is null.
        /// </exception>
        public AuditLoggerService(ApplicationDbContext context, IParameterValidator parameterValidator, IAuditLoggerServiceResultFactory auditLoggerServiceResultFactory, IMapper mapper)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _parameterValidator = parameterValidator ?? throw new ArgumentNullException(nameof(parameterValidator));
            _auditLoggerServiceResultFactory = auditLoggerServiceResultFactory ?? throw new ArgumentNullException(nameof(auditLoggerServiceResultFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        ///     Asynchronously retrieves a paginated list of audit logs from the database.
        /// </summary>
        /// <param name="request">
        ///     Contains the pagination details for fetching audit log and audit action for optional filtering
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The task result contains
        ///     the audit logs and pagination metadata.
        /// </returns>
        public async Task<AuditLogServiceListResult> GetLogsAsync(AuditLogListRequest request)
        {
            _parameterValidator.ValidateObjectNotNull(request, nameof(request));

            var query = _context.AuditLogs.AsQueryable();
            if (request.Action.HasValue)
            {
                query = query.Where(x => x.Action == request.Action.Value);
            }

            var totalCount = await query.CountAsync();
            var auditLogs = await query
                .Select(x => new SimplifiedAuditLogDTO { Id = x.Id, Action = x.Action, TimeStamp = x.TimeStamp })
                .OrderBy(x => x.TimeStamp)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .AsNoTracking()
                .ToListAsync();

            var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);
            PaginationModel paginationMetadata = new()
            {
                TotalCount = totalCount,
                PageSize = request.PageSize,
                CurrentPage = request.Page,
                TotalPages = totalPages
            };

            return new AuditLogServiceListResult { Logs = auditLogs, PaginationMetadata = paginationMetadata };
        }

        /// <summary>
        ///     Asynchronously retrieves a specific audit log entry from the database using its unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the audit log entry to retrieve.
        /// </param>
        /// <returns>
        ///     A task that represents the asynchronous operation. The result contains the audit log entry details
        ///     wrapped in a <see cref="AuditLogServiceResult"/>, or an error result if the log was not found.
        /// </returns>
        public async Task<AuditLogServiceResult> GetLogAsync(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var log = await _context.AuditLogs.FindAsync(id);
            if(log == null)
            {
                return _auditLoggerServiceResultFactory.AuditLoggerOperationFailure(new[] { ErrorMessages.AuditLog.NotFound });
            }

            var auditLogDTO = _mapper.Map<AuditLogDTO>(log);
            return _auditLoggerServiceResultFactory.AuditLoggerOperationSuccess(auditLogDTO);
        }

        /// <summary>
        ///     Asynchronously deletes an audit log entry by its unique identifier.
        /// </summary>
        /// <param name="id">
        ///     The unique identifier of the audit log entry to be deleted.
        /// </param>
        /// <returns>
        ///     A task representing the asynchronous operation. The task result 
        ///     indicates whether the deletion was successful and provides any 
        ///     associated error messages if applicable.
        /// </returns>
        public async Task<ServiceResult> DeleteLogAsync(string id)
        {
            _parameterValidator.ValidateNotNullOrEmpty(id, nameof(id));

            var log = await _context.AuditLogs.FindAsync(id);
            if (log == null)
            {
                return _auditLoggerServiceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.AuditLog.NotFound });
            }

            _context.AuditLogs.Remove(log);

            int result = await _context.SaveChangesAsync();
            if (result != 1)
            {
                return _auditLoggerServiceResultFactory.GeneralOperationFailure(new[] { ErrorMessages.AuditLog.DeletionFailed });
            }

            return _auditLoggerServiceResultFactory.GeneralOperationSuccess();
        }

        /// <summary>
        ///     Adds an audit log to the database after validating its properties.
        /// </summary>
        /// <param name="log">
        ///     The audit log entry to be added.
        /// </param>
        /// <returns>
        ///     Asynchronous task representing the operation.
        /// </returns>
        protected async Task AddLog(AuditLog log)
        {
            _parameterValidator.ValidateObjectNotNull(log, nameof(log));
            _parameterValidator.ValidateNotNullOrEmpty(log.Details, nameof(log.Details));
            _parameterValidator.ValidateNotNullOrEmpty(log.IpAddress, nameof(log.IpAddress));

            ValidateAuditAction(log.Action);
            ValidateTimestamp(log.TimeStamp);

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        private static void ValidateTimestamp(DateTime timeStamp)
        {
            if (Math.Abs((DateTime.UtcNow - timeStamp).TotalSeconds) > 30)  // Allow up to 30 seconds tolerance
            {
                throw new ArgumentException(ErrorMessages.AuditLog.InvalidDate);
            }
        }

        private static void ValidateAuditAction(AuditAction action)
        {
            if (!Enum.IsDefined(typeof(AuditAction), action))
            {
                throw new ArgumentException(ErrorMessages.AuditLog.InvalidAction);
            }
        }
    }
}
