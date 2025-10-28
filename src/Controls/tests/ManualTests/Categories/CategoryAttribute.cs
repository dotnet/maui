namespace Microsoft.Maui.ManualTests.Categories;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class CategoryAttribute : Attribute
{
	public CategoryAttribute(string id, string title)
	{
		Id = id;
		Title = title;
	}

	public string Id { get; set; }

	public string Title { get; set; }
}
