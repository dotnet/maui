namespace Xamarin.Forms
{
	interface IResourcesProvider
	{
		bool IsResourcesCreated { get; }
		ResourceDictionary Resources { get; set; }
	}
}