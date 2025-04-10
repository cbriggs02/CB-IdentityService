using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityServiceApi.Models.Entities
{
	/// <summary>
	///     Represents a country entity in the system.
	/// </summary>
	/// <remarks>
	///     @Author: Christian Briglio  
	///     @Created: 2025  
	/// </remarks>
	public class Country
	{
		/// <summary>
		///     Gets or sets the unique identifier for the country entry.
		/// </summary>
		[SwaggerSchema(ReadOnly = true)]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }

		/// <summary>
		///     Gets or sets the name of the country.
		/// </summary>
		[Required(ErrorMessage = "Country name is required")]
		[StringLength(100)]
		public string Name { get; set; } = string.Empty;
    }
}
