namespace FlowNetFramework.Commons.Models.Responses;

public class PagedResponse<T>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public T Data { get; set; }

    public PagedResponse(T data, int pageNumber, int pageSize, int TotalRecords)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        this.TotalRecords = TotalRecords;
        Data = data;
    }
}
