namespace Library.Resources;

public class AuthorsResourceParameters : ResourseParameters
{
	protected override int MaxPageSize { get; set; } = 15;
	public string? MainCategory { get; set; }
	public string? SearchQuery { get; set; }
}

public class ResourseParameters
{
	protected virtual int MaxPageSize { get; set; } = 10;
	public int PageNumber { get; set; } = 1;
	private int pageSize = 5;
	public int PageSize
	{
		get => pageSize;
		set => pageSize = (value > MaxPageSize) ? MaxPageSize : value;
	}
}
