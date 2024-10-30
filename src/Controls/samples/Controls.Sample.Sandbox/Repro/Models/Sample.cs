namespace AllTheLists.Models;

public class Sample
{
    public string Name { get; set; }
    public string Image {get;set;}
    public string Description { get; set; }
    public Type Page { get; set; }

    public override string ToString()
    {
        return Name;
    }
}