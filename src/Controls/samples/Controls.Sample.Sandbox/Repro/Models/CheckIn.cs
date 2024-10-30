namespace AllTheLists.Models;
public class CheckIn
{
    public string Author { get; set; }
    public string AuthorIcon { get; set; }
    public Venue Venue { get; set; }
    public Product Product { get; set; }
    public string Comment { get; set; }
    public double Rating { get; set; }
    public string ServingStyle { get; set; }
    public DateTime CreatedAt { get; set; }
    public string SocialImage {get;set;}

}