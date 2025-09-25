#nullable disable
using System;

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.Label;

	/// <summary>Provides access to reading order detection on the Windows platform.</summary>
	public static class Label
	{
		/// <summary>Bindable property for attached property <c>DetectReadingOrderFromContent</c>.</summary>
		public static readonly BindableProperty DetectReadingOrderFromContentProperty = BindableProperty.Create("DetectReadingOrderFromContent", typeof(bool), typeof(FormsElement), false);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Label.xml" path="//Member[@MemberName='SetDetectReadingOrderFromContent'][1]/Docs/*" />
		public static void SetDetectReadingOrderFromContent(BindableObject element, bool value)
		{
			element.SetValue(DetectReadingOrderFromContentProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Label.xml" path="//Member[@MemberName='GetDetectReadingOrderFromContent'][2]/Docs/*" />
		public static bool GetDetectReadingOrderFromContent(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (bool)config.Element.GetValue(DetectReadingOrderFromContentProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Label.xml" path="//Member[@MemberName='GetDetectReadingOrderFromContent'][1]/Docs/*" />
		public static bool GetDetectReadingOrderFromContent(BindableObject element)
		{
			return (bool)element.GetValue(DetectReadingOrderFromContentProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Label.xml" path="//Member[@MemberName='SetDetectReadingOrderFromContent'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetDetectReadingOrderFromContent(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			config.Element.SetValue(DetectReadingOrderFromContentProperty, value);
			return config;
		}
	}

}
