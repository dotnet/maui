#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an IMenuFlyoutItem about updates to an IMenuFlyoutItemHandler
	/// </summary>
	public record MenuFlyoutItemHandlerUpdate(int Index, IMenuFlyoutItem MenuFlyoutItemBaseItem);
}
