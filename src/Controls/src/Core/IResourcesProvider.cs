namespace Microsoft.Maui.Controls
{
	public interface IResourcesProvider
	{
		bool IsResourcesCreated { get; }
		ResourceDictionary Resources { get; set; }
	}
}