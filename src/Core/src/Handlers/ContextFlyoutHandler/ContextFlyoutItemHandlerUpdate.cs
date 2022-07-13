#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an IContextFlyout about updates to an IContextFlyoutHandler
	/// </summary>
	public record ContextFlyoutItemHandlerUpdate(int Index, IMenuElement MenuElement);
}
