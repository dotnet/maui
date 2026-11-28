using System.ComponentModel;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View used to initiating a search.
	/// </summary>
	public interface ISearchBar : IView, ITextInput, ITextAlignment
	{
		/// <summary>
		/// Gets the color of the cancel button.
		/// </summary>
		Color CancelButtonColor { get; }

		/// <summary>
		/// Gets the color of the Search icon.
		/// </summary>
		Color SearchIconColor { get; }

		/// <summary>
		/// Gets an enumeration value that controls the appearance of the return button.
		/// </summary>
		ReturnType ReturnType { get; }

		/// <summary>
		/// Notify when the user presses the Search button.
		/// </summary>
		void SearchButtonPressed();
	}

	/// <summary>
	/// Provides optional control over the cancel and clear buttons of an <see cref="ISearchBar"/>.
	/// </summary>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface ISearchBarClearButtonVisibility
	{
		/// <summary>
		/// Gets when the platform's cancel and clear buttons are shown.
		/// </summary>
		/// <remarks>
		/// <see cref="ClearButtonVisibility.Never"/> hides both buttons.
		/// <see cref="ClearButtonVisibility.WhileEditing"/> shows them only while the search bar
		/// has focus and its text is not null or empty.
		/// Search bars that do not implement this interface retain their existing behavior.
		/// </remarks>
		ClearButtonVisibility ClearButtonVisibility { get; }
	}

	internal static class SearchBarVisibilityExtensions
	{
		internal static bool AllowsCancelButton(this ISearchBar searchBar) =>
			searchBar is not ISearchBarClearButtonVisibility visibility ||
			(visibility.ClearButtonVisibility == ClearButtonVisibility.WhileEditing && searchBar.IsFocused);
	}
}