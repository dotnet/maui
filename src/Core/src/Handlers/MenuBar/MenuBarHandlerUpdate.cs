#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an IMenuBar about updates to an IMenuBarHandler
	/// </summary>
	public record MenuBarHandlerUpdate(int Index, IMenuBarItem MenuBarItem);
}
