#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.AppCompat
{
	using FormsElement = Maui.Controls.NavigationPage;

	/// <summary>Appcompat platform specific navigation page.</summary>
	public static class NavigationPage
	{
		/// <summary>Bindable property for attached property <c>BarHeight</c>.</summary>
		public static readonly BindableProperty BarHeightProperty = BindableProperty.Create("BarHeight", typeof(int), typeof(NavigationPage), default(int));

		/// <summary>Returns the height of the navigation bar.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>The height of the navigation bar.</returns>
		public static int GetBarHeight(BindableObject element)
		{
			return (int)element.GetValue(BarHeightProperty);
		}

		/// <summary>Sets the height of the navigation bar.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="value">The new navigation bar height value to assign.</param>
		public static void SetBarHeight(BindableObject element, int value)
		{
			element.SetValue(BarHeightProperty, value);
		}

		/// <summary>Returns the height of the navigation bar.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The height of the navigation bar.</returns>
		public static int GetBarHeight(this IPlatformElementConfiguration<Android, FormsElement> config)
		{
			return GetBarHeight(config.Element);
		}

		/// <summary>Sets the height of the navigation bar.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The new navigation bar height value to assign.</param>
		/// <returns>The platform specific configuration that contains the element on which to perform the operation.</returns>
		public static IPlatformElementConfiguration<Android, FormsElement> SetBarHeight(this IPlatformElementConfiguration<Android, FormsElement> config, int value)
		{
			SetBarHeight(config.Element, value);
			return config;
		}
	}
}
