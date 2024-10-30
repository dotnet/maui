namespace AllTheLists.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }

    public string Company {get;set;}
    public decimal Price { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
    public string Image {get;set;}
    public string Type {get;set;}

    public string SalesCategory { get; set; }

    public string Category { get; set; }

    public int ColorWays { get; set; } = 1;  

    public bool HasImage => !string.IsNullOrWhiteSpace(ImageUrl);

    public string SocialImageUrl { get; internal set; }
}
