namespace Microsoft.Maui.ManualTests.Tests;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TestAttribute : Attribute
{
	public TestAttribute(string id, string title, string category)
	{
		Id = id;
		Title = title;
		Category = category;
	}

	public string Id { get; set; }

	public string Title { get; set; }

	public string Category { get; set; }
}