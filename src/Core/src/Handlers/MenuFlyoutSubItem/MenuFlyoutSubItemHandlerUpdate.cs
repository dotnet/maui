#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an IMenuFlyoutSubItem about updates to an IMenuFlyoutSubItemHandler
	/// </summary>
	public record MenuFlyoutSubItemHandlerUpdate(int Index, IMenuElement MenuElement);
}
