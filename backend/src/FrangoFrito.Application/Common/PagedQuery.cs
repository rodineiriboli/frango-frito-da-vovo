namespace FrangoFrito.Application.Common;

public sealed record PagedQuery(int Page = 1, int PageSize = 10, string? Search = null)
{
    public int SafePage => Page < 1 ? 1 : Page;
    public int SafePageSize => PageSize switch
    {
        < 1 => 10,
        > 100 => 100,
        _ => PageSize
    };
}
