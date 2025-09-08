using AndroidX.Core.View;
using AView = Android.Views.View;

namespace Microsoft.Maui.Platform
{
    /// <summary>
    /// Interface for views that need to handle their own window insets behavior
    /// </summary>
    public interface IHandleWindowInsets
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
}