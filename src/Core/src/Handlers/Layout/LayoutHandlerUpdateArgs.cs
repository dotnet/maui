#nullable enable

namespace Microsoft.Maui.Handlers
{
	/// <summary>
	/// Communicates information from an ILayout about updates to an ILayoutHandler
	/// </summary>
	public struct LayoutHandlerUpdateArgs
	{
		/// <summary>
		/// The index of IView update to be made
		/// </summary>
		public int Index;

		/// <summary>
		/// The IView to be updated in the ILayoutHandler
		/// </summary>
		public IView View;
	}
}
