#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using MauiElement = Controls.CollectionView;

	public static class CollectionView
	{
		#region SingleSelectionFollowsFocus

		/// <summary>
		/// Bindable property to indicate whether selection follows focus in single selection mode.
		/// </summary>
		public static readonly BindableProperty SingleSelectionFollowsFocusProperty =
			BindableProperty.CreateAttached("SingleSelectionFollowsFocus", typeof(bool),
				typeof(CollectionView), false);

		/// <summary>
		/// Gets the value of the SingleSelectionFollowsFocus property from the specified element.
		/// </summary>
		/// <param name="element">The element from which to read the property.</param>
		/// <returns>A boolean value indicating whether selection follows focus.</returns>
		public static bool GetSingleSelectionFollowsFocus(BindableObject element)
		{
			return (bool)element.GetValue(SingleSelectionFollowsFocusProperty);
		}

		/// <summary>
		/// Sets the value of the SingleSelectionFollowsFocus property on the specified element.
		/// </summary>
		/// <param name="element">The element on which to set the property.</param>
		/// <param name="value">The value to set for the property.</param>
		public static void SetSingleSelectionFollowsFocus(BindableObject element, bool value)
		{
			element.SetValue(SingleSelectionFollowsFocusProperty, value);
		}

		/// <summary>
		/// Gets the value of the SingleSelectionFollowsFocus property from the specified platform configuration.
		/// </summary>
		/// <param name="config">The platform configuration from which to read the property.</param>
		/// <returns>A boolean value indicating whether selection follows focus.</returns>
		public static bool GetSingleSelectionFollowsFocus(this IPlatformElementConfiguration<Windows, MauiElement> config)
		{
			return (bool)config.Element.GetValue(SingleSelectionFollowsFocusProperty);
		}

		/// <summary>
		/// Sets the value of the SingleSelectionFollowsFocus property on the specified platform configuration.
		/// </summary>
		/// <param name="config">The platform configuration on which to set the property.</param>
		/// <param name="value">The value to set for the property.</param>
		/// <returns>The platform configuration with the updated property value.</returns>
		public static IPlatformElementConfiguration<Windows, MauiElement> SetSingleSelectionFollowsFocus(
			this IPlatformElementConfiguration<Windows, MauiElement> config, bool value)
		{
			config.Element.SetValue(SingleSelectionFollowsFocusProperty, value);
			return config;
		}

		#endregion
	}
}
