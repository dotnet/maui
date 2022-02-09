#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an IMenuFlyoutItemBase about updates to an IMenuFlyoutItemBaseHandler
	/// </summary>
	public record MenuFlyoutItemBaseHandlerUpdate(int Index, IMenuFlyoutItemBase MenuFlyoutItemBaseItem);
}
