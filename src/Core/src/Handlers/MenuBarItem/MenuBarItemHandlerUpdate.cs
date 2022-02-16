#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an IMenuBarItem about updates to an IMenuBarItemHandler
	/// </summary>
	public record MenuBarItemHandlerUpdate(int Index, IMenuElement MenuElement);
}
