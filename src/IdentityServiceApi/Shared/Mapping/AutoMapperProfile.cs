using AutoMapper;
using IdentityServiceApi.Features.UserManagement.Models.DTOs;
using IdentityServiceApi.Features.UserManagement.Models.Entities;

namespace IdentityServiceApi.Shared.Mapping
{
    /// <summary>
    ///     AutoMapper profile used to define mappings between entity classes and Data Transfer Objects (DTOs).
    ///     This profile is responsible for configuring how the application maps between the User entity and UserDTO.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    ///     @Updated: 2026
    /// </remarks>
    public class AutoMapperProfile : Profile
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AutoMapperProfile"/> class and defines the mappings 
        ///     between the entity and DTO types.
        /// </summary>
        public AutoMapperProfile()
        {
            CreateMap<User, UserDTO>();
        }
    }
}
