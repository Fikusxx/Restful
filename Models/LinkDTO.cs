namespace Library.Models;

public class LinkDTO
{
	public string Href { get; private set; }
	public string Rel { get; private set; }
	public string Method { get; private set; }

    public LinkDTO(string href, string rel, string method)
    {
        Href = href;
        Rel = rel;
        Method = method;
    }
}
