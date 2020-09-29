using System;

namespace Xamarin.Forms.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Forms.Entry;

	public static class Entry
	{
		public static readonly BindableProperty AdjustsFontSizeToFitWidthProperty =
			BindableProperty.Create("AdjustsFontSizeToFitWidth", typeof(bool),
				typeof(Entry), false);
		public static readonly BindableProperty CursorColorProperty = BindableProperty.Create("CursorColor", typeof(Color), typeof(Entry), Color.Default);

		public static bool GetAdjustsFontSizeToFitWidth(BindableObject element)
		{
			return (bool)element.GetValue(AdjustsFontSizeToFitWidthProperty);
		}

		public static void SetAdjustsFontSizeToFitWidth(BindableObject element, bool value)
		{
			element.SetValue(AdjustsFontSizeToFitWidthProperty, value);
		}

		public static bool AdjustsFontSizeToFitWidth(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetAdjustsFontSizeToFitWidth(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetAdjustsFontSizeToFitWidth(
			this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetAdjustsFontSizeToFitWidth(config.Element, value);
			return config;
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> EnableAdjustsFontSizeToFitWidth(
			this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			SetAdjustsFontSizeToFitWidth(config.Element, true);
			return config;
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> DisableAdjustsFontSizeToFitWidth(
			this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			SetAdjustsFontSizeToFitWidth(config.Element, false);
			return config;
		}

		public static Color GetCursorColor(BindableObject element)
		{
			return (Color)element.GetValue(CursorColorProperty);
		}

		public static void SetCursorColor(BindableObject element, Color value)
		{
			element.SetValue(CursorColorProperty, value);
		}

		public static Color GetCursorColor(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetCursorColor(config.Element);
		}

		public static IPlatformElementConfiguration<iOS, FormsElement> SetCursorColor(this IPlatformElementConfiguration<iOS, FormsElement> config, Color value)
		{
			SetCursorColor(config.Element, value);
			return config;
		}
	}
}