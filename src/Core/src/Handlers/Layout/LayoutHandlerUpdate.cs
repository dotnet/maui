#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an ILayout about updates to an ILayoutHandler
	/// </summary>
	public record LayoutHandlerUpdate(int Index, IView View);
}
