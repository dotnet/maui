using System;

namespace Xamarin.Forms.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Forms.InputView;

	public static class InputView
	{
		public static readonly BindableProperty DetectReadingOrderFromContentProperty = BindableProperty.Create("DetectReadingOrderFromContent", typeof(bool), typeof(FormsElement), false);

		public static void SetDetectReadingOrderFromContent(BindableObject element, bool value)
		{
			element.SetValue(DetectReadingOrderFromContentProperty, value);
		}

		public static bool GetDetectReadingOrderFromContent(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (bool)config.Element.GetValue(DetectReadingOrderFromContentProperty);
		}

		public static bool GetDetectReadingOrderFromContent(BindableObject element)
		{
			return (bool)element.GetValue(DetectReadingOrderFromContentProperty);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetDetectReadingOrderFromContent(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			config.Element.SetValue(DetectReadingOrderFromContentProperty, value);
			return config;
		}
	}

}