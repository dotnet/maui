namespace Microsoft.Maui
{
	public interface IHighlightLayer
	{
		public IWindow Window { get; }

		public bool AddHighlight(Maui.IView view);

		public bool RemoveHighlight(Maui.IView view);

		public void ClearHighlights();
	}
}
