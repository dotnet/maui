using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Xamarin.Forms.TabbedPage;

	public static class TabbedPage
	{
		public static readonly BindableProperty HeaderIconsEnabledProperty =
			BindableProperty.Create(nameof(HeaderIconsEnabledProperty), typeof(bool), typeof(TabbedPage), true);

		public static readonly BindableProperty HeaderIconsSizeProperty =
			BindableProperty.Create(nameof(HeaderIconsSizeProperty), typeof(Xamarin.Forms.Size), typeof(TabbedPage), new Xamarin.Forms.Size(16, 16));

		public static void SetHeaderIconsEnabled(BindableObject element, bool value)
		{
			element.SetValue(HeaderIconsEnabledProperty, value);
		}

		public static bool GetHeaderIconsEnabled(BindableObject element)
		{
			return (bool)element.GetValue(HeaderIconsEnabledProperty);
		}

		public static bool GetHeaderIconsEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetHeaderIconsEnabled(config.Element);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetHeaderIconsEnabled(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			SetHeaderIconsEnabled(config.Element, value);
			return config;
		}

		public static bool IsHeaderIconsEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetHeaderIconsEnabled(config.Element);
		}

		public static void EnableHeaderIcons(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetHeaderIconsEnabled(config.Element, true);
		}

		public static void DisableHeaderIcons(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetHeaderIconsEnabled(config.Element, false);
		}

		public static void SetHeaderIconsSize(BindableObject element, Xamarin.Forms.Size value)
		{
			element.SetValue(HeaderIconsSizeProperty, value);
		}

		public static Xamarin.Forms.Size GetHeaderIconsSize(BindableObject element)
		{
			return (Xamarin.Forms.Size)element.GetValue(HeaderIconsSizeProperty);
		}

		public static Xamarin.Forms.Size GetHeaderIconsSize(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetHeaderIconsSize(config.Element);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetHeaderIconsSize(
			this IPlatformElementConfiguration<Windows, FormsElement> config, Xamarin.Forms.Size value)
		{
			SetHeaderIconsSize(config.Element, value);
			return config;
		}

	}
}
