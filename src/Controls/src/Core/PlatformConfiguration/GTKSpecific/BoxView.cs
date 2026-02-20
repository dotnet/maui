#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.GTKSpecific
{
	using FormsElement = Maui.Controls.BoxView;

	/// <summary>Controls the presence of the corner radius of box views on the GTK platform.</summary>
	public static class BoxView
	{
		/// <summary>Bindable property for attached property <c>HasCornerRadius</c>.</summary>
		public static readonly BindableProperty HasCornerRadiusProperty =
			BindableProperty.Create("HasCornerRadius", typeof(bool),
				typeof(BoxView), default(bool));

		/// <summary>Gets the value that controls whether the box view has a corner radius.</summary>
		/// <param name="element">The platform-specific element.</param>
		/// <returns><see langword="true"/> if the box view has a corner radius; otherwise, <see langword="false"/>.</returns>
		public static bool GetHasCornerRadius(BindableObject element)
		{
			return (bool)element.GetValue(HasCornerRadiusProperty);
		}

		/// <summary>Sets a value that controls whether the box view has a corner radius.</summary>
		/// <param name="element">The platform-specific element.</param>
		/// <param name="tabPosition">The new property value to assign.</param>
		public static void SetHasCornerRadius(BindableObject element, bool tabPosition)
		{
			element.SetValue(HasCornerRadiusProperty, tabPosition);
		}

		/// <summary>Gets the value that controls whether the box view has a corner radius.</summary>
		/// <param name="config">The platform-specific configuration.</param>
		/// <returns><see langword="true"/> if the box view has a corner radius; otherwise, <see langword="false"/>.</returns>
		public static bool GetHasCornerRadius(
			this IPlatformElementConfiguration<GTK, FormsElement> config)
		{
			return GetHasCornerRadius(config.Element);
		}

		/// <summary>Sets a value that controls whether the box view has a corner radius.</summary>
		/// <param name="config">The platform-specific configuration.</param>
		/// <param name="value">The new property value to assign.</param>
		/// <returns>The updated configuration object.</returns>
		public static IPlatformElementConfiguration<GTK, FormsElement> SetHasCornerRadius(
			this IPlatformElementConfiguration<GTK, FormsElement> config, bool value)
		{
			SetHasCornerRadius(config.Element, value);

			return config;
		}
	}
}
