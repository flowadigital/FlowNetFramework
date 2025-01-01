namespace FlowNetFramework.Commons.Models
{
    public class Pagination
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        public string? OrderBy { get; set; }

        public bool? IsDescending { get; set; } = false;
    }
}
