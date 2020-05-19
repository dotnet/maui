namespace System.Maui
{
	interface IResourcesProvider
	{
		bool IsResourcesCreated { get; }
		ResourceDictionary Resources { get; set; }
	}
}