#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an IMenuFlyout about updates to an IMenuFlyoutHandler
	/// </summary>
	public record ContextFlyoutItemHandlerUpdate(int Index, IMenuElement MenuElement);
}
