using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a container that provides pull to refresh functionality for scrollable content.
	/// </summary>
	public interface IRefreshView : IView
	{
		/// <summary>
		/// Gets a bool that indicate if the View is loading or not.
		/// </summary>
		bool IsRefreshing { get; set; }

		/// <summary>
		/// Gets the loading indicator color.
		/// </summary>
		Paint? RefreshColor { get; }

		/// <summary>
		/// The scrollable content to refresh.
		/// </summary>
		IView Content { get; }
	}
}
