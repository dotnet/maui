using AndroidX.Core.View;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
	/// <summary>
	/// Interface for views that need to handle their own window insets behavior
	/// </summary>
	internal interface IHandleWindowInsets
	{
		/// <summary>
		/// Handles window insets for this view
		/// </summary>
		/// <param name="view">The view receiving the insets</param>
		/// <param name="insets">The window insets</param>
		/// <returns>The processed window insets</returns>
		WindowInsetsCompat? HandleWindowInsets(AView view, WindowInsetsCompat insets);

		/// <summary>
		/// Resets any previously applied insets on this view
		/// </summary>
		/// <param name="view">The view to reset</param>
		void ResetWindowInsets(AView view);
	}

	/// <summary>
	/// Interface for views that need window insets to be re-applied after layout transitions.
	/// Views implementing this interface will have RequestApplyInsets posted when they extend
	/// beyond screen bounds (e.g., during fragment transitions).
	/// </summary>
	/// <remarks>
	/// This is primarily used by Shell to handle fragment transitions where views are temporarily
	/// positioned off-screen. Without this, views might get incorrect safe area calculations
	/// during the transition. TabbedPage and other views that intentionally position content
	/// off-screen should NOT implement this interface to avoid infinite inset request loops.
	/// </remarks>
	internal interface IRequestInsetsOnTransition
	{
	}

	/// <summary>
	/// Marker interface for ContentPage to indicate it should receive transition inset updates.
	/// This is separate from IRequestInsetsOnTransition (which is for Shell hierarchy) because
	/// we need to check that the actual view being processed is a ContentPage, not just that
	/// it's under a Shell.
	/// </summary>
	/// <remarks>
	/// Only ContentPage should implement this interface. Other content views like ContentView,
	/// ScrollView, Border, etc. should NOT implement this to avoid the infinite loop issue
	/// when they are positioned off-screen (e.g., in TabbedPage inactive tabs).
	/// </remarks>
	internal interface IContentPageController
	{
	}
}