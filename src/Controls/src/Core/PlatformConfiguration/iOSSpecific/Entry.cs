using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Maui.Controls.Entry;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Entry']/Docs" />
	public static class Entry
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='AdjustsFontSizeToFitWidthProperty']/Docs" />
		public static readonly BindableProperty AdjustsFontSizeToFitWidthProperty =
			BindableProperty.Create("AdjustsFontSizeToFitWidth", typeof(bool),
				typeof(Entry), false);
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='CursorColorProperty']/Docs" />
		public static readonly BindableProperty CursorColorProperty = BindableProperty.Create("CursorColor", typeof(Color), typeof(Entry), null);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='GetAdjustsFontSizeToFitWidth']/Docs" />
		public static bool GetAdjustsFontSizeToFitWidth(BindableObject element)
		{
			return (bool)element.GetValue(AdjustsFontSizeToFitWidthProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='SetAdjustsFontSizeToFitWidth'][1]/Docs" />
		public static void SetAdjustsFontSizeToFitWidth(BindableObject element, bool value)
		{
			element.SetValue(AdjustsFontSizeToFitWidthProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='AdjustsFontSizeToFitWidth']/Docs" />
		public static bool AdjustsFontSizeToFitWidth(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetAdjustsFontSizeToFitWidth(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='SetAdjustsFontSizeToFitWidth'][2]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetAdjustsFontSizeToFitWidth(
			this IPlatformElementConfiguration<iOS, FormsElement> config, bool value)
		{
			SetAdjustsFontSizeToFitWidth(config.Element, value);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='EnableAdjustsFontSizeToFitWidth']/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> EnableAdjustsFontSizeToFitWidth(
			this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			SetAdjustsFontSizeToFitWidth(config.Element, true);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='DisableAdjustsFontSizeToFitWidth']/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> DisableAdjustsFontSizeToFitWidth(
			this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			SetAdjustsFontSizeToFitWidth(config.Element, false);
			return config;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='GetCursorColor'][1]/Docs" />
		public static Color GetCursorColor(BindableObject element)
		{
			return (Color)element.GetValue(CursorColorProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='SetCursorColor'][1]/Docs" />
		public static void SetCursorColor(BindableObject element, Color value)
		{
			element.SetValue(CursorColorProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='GetCursorColor'][2]/Docs" />
		public static Color GetCursorColor(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetCursorColor(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Entry.xml" path="//Member[@MemberName='SetCursorColor'][2]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetCursorColor(this IPlatformElementConfiguration<iOS, FormsElement> config, Color value)
		{
			SetCursorColor(config.Element, value);
			return config;
		}
	}
}
