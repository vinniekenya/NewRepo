using System.Collections.Generic;

public class NewsResponse
{
    public List<Article> Articles { get; set; }
}

public class Article
{
    public string Title { get; set; }
    public string Description { get; set; }
}