namespace IdentityServiceApi.Models.Shared
{
    /// <summary>
    ///     Represents metadata used to assist with pagination in API responses,
    ///     providing information such as the total number of items, page size, current page, 
    ///     and total pages. This helps clients navigate through large datasets efficiently.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public class PaginationModel
    {
        /// <summary>
        ///     Gets or sets the total count of items available.
        ///     This represents the total number of records in the dataset.
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        ///     Gets or sets the size of each page.
        ///     This determines how many items are displayed per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        ///     Gets or sets the current page number.
        ///     Indicates the current page of results being viewed.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        ///     Gets or sets the total number of pages.
        ///     Calculated based on the TotalCount and PageSize.
        /// </summary>
        public int TotalPages { get; set; }
    }
}
