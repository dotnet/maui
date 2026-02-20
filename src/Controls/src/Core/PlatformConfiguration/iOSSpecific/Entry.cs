#nullable disable
using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.Entry;

	/// <summary>The entry instance that Microsoft.Maui.Controls created on the iOS platform.</summary>
	public static class Entry
	{
		/// <summary>Bindable property for <see cref="AdjustsFontSizeToFitWidth"/>.</summary>
		public static readonly BindableProperty AdjustsFontSizeToFitWidthProperty =
			BindableProperty.Create("AdjustsFontSizeToFitWidth", typeof(bool),
				typeof(Entry), false);

		/// <summary>Bindable property for attached property <c>CursorColor</c>.</summary>
		public static readonly BindableProperty CursorColorProperty = BindableProperty.Create("CursorColor", typeof(Color), typeof(Entry), null);

		/// <summary>Returns a Boolean value that tells whether the entry control automatically adjusts the font size of text that the user enters.</summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether the entry control automatically adjusts the font size of text that the user enters.</returns>
		public static bool GetAdjustsFontSizeToFitWidth(BindableObject element)
		{
			return (bool)element.GetValue(AdjustsFontSizeToFitWidthProperty);
		}

		/// <summary>Sets whether the Entry automatically adjusts font size to fit the width on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value"><see langword="true"/> to enable auto font size adjustment; otherwise, <see langword="false"/>.</param>
		public static void SetAdjustsFontSizeToFitWidth(BindableObject element, bool value)
		{
			element.SetValue(AdjustsFontSizeToFitWidthProperty, value);
		}

		/// <summary>Returns a Boolean value that tells whether the entry control automatically adjusts the font size.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>A Boolean value that tells whether the entry control automatically adjusts the font size.</returns>
		public static bool AdjustsFontSizeToFitWidth(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetAdjustsFontSizeToFitWidth(config.Element);
		}

		/// <summary>Sets whether the Entry automatically adjusts font size to fit the width on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to enable auto font size adjustment; otherwise, <see langword="false"/>.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetAdjustsFontSizeToFitWidth(
			this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetAdjustsFontSizeToFitWidth(config.Element, value);
			return config;
		}

		/// <summary>Enables automatic font size adjustment on the platform-specific element.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> EnableAdjustsFontSizeToFitWidth(
			this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			SetAdjustsFontSizeToFitWidth(config.Element, true);
			return config;
		}

		/// <summary>Disables automatic font size adjustment on the platform-specific element.</summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> DisableAdjustsFontSizeToFitWidth(
			this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			SetAdjustsFontSizeToFitWidth(config.Element, false);
			return config;
		}

		/// <summary>Gets the cursor (caret) color for the Entry on iOS.</summary>
		/// <param name="element">The element to get the value from.</param>
		/// <returns>The cursor color.</returns>
		public static Color GetCursorColor(BindableObject element)
		{
			return (Color)element.GetValue(CursorColorProperty);
		}

		/// <summary>Sets the cursor (caret) color for the Entry on iOS.</summary>
		/// <param name="element">The element to set the value on.</param>
		/// <param name="value">The cursor color to apply.</param>
		public static void SetCursorColor(BindableObject element, Color value)
		{
			element.SetValue(CursorColorProperty, value);
		}

		/// <summary>Gets the cursor (caret) color for the Entry on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The cursor color.</returns>
		public static Color GetCursorColor(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetCursorColor(config.Element);
		}

		/// <summary>Sets the cursor (caret) color for the Entry on iOS.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The cursor color to apply.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<iOS, FormsElement> SetCursorColor(this IPlatformElementConfiguration<iOS, FormsElement> config, Color value)
		{
			SetCursorColor(config.Element, value);
			return config;
		}
	}
}
