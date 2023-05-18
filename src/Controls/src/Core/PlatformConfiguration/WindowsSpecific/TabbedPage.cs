#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.TabbedPage;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.TabbedPage']/Docs/*" />
	public static class TabbedPage
	{
		/// <summary>Bindable property for attached property <c>HeaderIconsEnabled</c>.</summary>
		public static readonly BindableProperty HeaderIconsEnabledProperty =
			BindableProperty.Create(nameof(HeaderIconsEnabledProperty), typeof(bool), typeof(TabbedPage), true);

		/// <summary>Bindable property for attached property <c>HeaderIconsSize</c>.</summary>
		public static readonly BindableProperty HeaderIconsSizeProperty =
			BindableProperty.Create(nameof(HeaderIconsSizeProperty), typeof(Size), typeof(TabbedPage), new Size(16, 16));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="//Member[@MemberName='SetHeaderIconsEnabled'][1]/Docs/*" />
		public static void SetHeaderIconsEnabled(BindableObject element, bool value)
		{
			element.SetValue(HeaderIconsEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="//Member[@MemberName='GetHeaderIconsEnabled'][1]/Docs/*" />
		public static bool GetHeaderIconsEnabled(BindableObject element)
		{
			return (bool)element.GetValue(HeaderIconsEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="//Member[@MemberName='GetHeaderIconsEnabled'][2]/Docs/*" />
		public static bool GetHeaderIconsEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetHeaderIconsEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="//Member[@MemberName='SetHeaderIconsEnabled'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetHeaderIconsEnabled(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			SetHeaderIconsEnabled(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="//Member[@MemberName='IsHeaderIconsEnabled']/Docs/*" />
		public static bool IsHeaderIconsEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetHeaderIconsEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="//Member[@MemberName='EnableHeaderIcons']/Docs/*" />
		public static void EnableHeaderIcons(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetHeaderIconsEnabled(config.Element, true);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="//Member[@MemberName='DisableHeaderIcons']/Docs/*" />
		public static void DisableHeaderIcons(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetHeaderIconsEnabled(config.Element, false);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="//Member[@MemberName='SetHeaderIconsSize'][1]/Docs/*" />
		public static void SetHeaderIconsSize(BindableObject element, Size value)
		{
			element.SetValue(HeaderIconsSizeProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="//Member[@MemberName='GetHeaderIconsSize'][1]/Docs/*" />
		public static Size GetHeaderIconsSize(BindableObject element)
		{
			return (Size)element.GetValue(HeaderIconsSizeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="//Member[@MemberName='GetHeaderIconsSize'][2]/Docs/*" />
		public static Size GetHeaderIconsSize(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetHeaderIconsSize(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/TabbedPage.xml" path="//Member[@MemberName='SetHeaderIconsSize'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetHeaderIconsSize(
			this IPlatformElementConfiguration<Windows, FormsElement> config, Size value)
		{
			SetHeaderIconsSize(config.Element, value);
			return config;
		}

	}
}
