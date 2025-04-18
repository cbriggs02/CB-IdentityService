using IdentityServiceApi.Interfaces.Logging;
using System.Diagnostics;

namespace IdentityServiceApi.Middleware
{
	/// <summary>
	///     Middleware for monitoring the performance of HTTP requests, including request duration and CPU usage.
	///     This middleware captures performance metrics for every HTTP request, logs them, and provides 
	///     insights for performance tuning.
	/// </summary>
	/// <remarks>
	///     @Author: Christian Briglio
	///     @Created: 2024
	/// </remarks>
	public class PerformanceMonitoringMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IWebHostEnvironment _env;
		private readonly int performanceThreshold = 1000;

		/// <summary>
		///     Initializes a new instance of the <see cref="PerformanceMonitoringMiddleware"/> class.
		/// </summary>
		/// <param name="next">
		///     The delegate representing the next middleware in the request pipeline.
		/// </param>
		/// <param name="logger">
		///     The logger instance for logging performance data.
		/// </param>
		/// <param name="scopeFactory">
		///     The factory for creating service scopes to resolve scoped services.
		/// </param>
		/// <param name="env">
		///     The environment in which the application is running (e.g., Development, Staging, Production).
		///     This is used to configure environment-specific logging behaviors.
		/// </param>
		/// <exception cref="ArgumentNullException">
		///     Thrown if any of the parameters are null.
		/// </exception>
		public PerformanceMonitoringMiddleware(RequestDelegate next, ILogger<PerformanceMonitoringMiddleware> logger, IServiceScopeFactory scopeFactory, IWebHostEnvironment env)
		{
			_next = next ?? throw new ArgumentNullException(nameof(next));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
			_env = env ?? throw new ArgumentNullException(nameof(env));
		}

		/// <summary>
		///     Asynchronously invokes the performance monitoring middleware.
		///     Starts a timer, passes the request down the pipeline, and logs 
		///     the request duration and CPU usage after completion.
		/// </summary>
		/// <param name="context">
		///     The <see cref="HttpContext"/> representing the current HTTP request.
		/// </param>
		/// <returns>
		/// <returns>
		///     A task representing the asynchronous operation of processing the request.
		/// </returns>
		public async Task InvokeAsync(HttpContext context)
		{
			var requestId = Guid.NewGuid().ToString();
			var stopwatch = StartRequestTimer();

			await _next(context);

			var requestDuration = StopRequestTimer(stopwatch);
			var cpuUsage = GetCpuUsage();

			await CheckPerformanceAsync(requestDuration);
			ConsoleLogPerformanceMetrics(context, requestId, requestDuration, cpuUsage);
		}

		private static Stopwatch StartRequestTimer()
		{
			return Stopwatch.StartNew();
		}

		private static long StopRequestTimer(Stopwatch stopwatch)
		{
			stopwatch.Stop();
			return stopwatch.ElapsedMilliseconds;
		}

		private static double GetCpuUsage()
		{
			return Process.GetCurrentProcess().TotalProcessorTime.TotalMilliseconds;
		}

		private async Task CheckPerformanceAsync(long requestDuration)
		{
			using var scope = _scopeFactory.CreateScope();
			var loggerService = scope.ServiceProvider.GetRequiredService<ILoggerService>();

			if (requestDuration > performanceThreshold)
			{
				await loggerService.LogSlowPerformanceAsync(requestDuration);
			}
		}

		private void ConsoleLogPerformanceMetrics(HttpContext context, string requestId, long requestDuration, double cpuUsage)
		{
			string metrics = $"Request ID: {requestId}, " +
				$"Request Path: {context.Request.Path}, " +
				$"Response Status Code: {context.Response.StatusCode}, " +
				$"Request Duration: {requestDuration} ms, " +
				$"CPU Usage: {cpuUsage} ms";

			if (_env.IsProduction())
			{
				if (requestDuration > performanceThreshold)
				{
					_logger.LogWarning(metrics);
				}
			}
			else
			{
				_logger.LogInformation(metrics);
			}
		}
	}
}
