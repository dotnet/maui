namespace AllTheLists.Models;
public class Review
{
    public string Author { get; set; }
    public string Comment { get; set; }
    public string Car { get; set; }
    public string ChargerType { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool Status { get; set; }

    public string StatusImage => Status ? "circle_check_solid.png" : "circle_xmark_solid.png";
}