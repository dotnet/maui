
namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Forms.ProgressBar;

	public static class ProgressBar
	{
		public static readonly BindableProperty ProgressBarPulsingStatusProperty =
			BindableProperty.Create("ProgressBarPulsingStatus", typeof(bool),
			typeof(FormsElement), false);

		public static bool GetPulsingStatus(BindableObject element)
		{
			return (bool)element.GetValue(ProgressBarPulsingStatusProperty);
		}

		public static void SetPulsingStatus(BindableObject element, bool isPulsing)
		{
			string style = VisualElement.GetStyle(element);
			if (style == ProgressBarStyle.Pending)
			{
				element.SetValue(ProgressBarPulsingStatusProperty, isPulsing);
			}
		}

		public static bool GetPulsingStatus(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetPulsingStatus(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetPulsingStatus(this IPlatformElementConfiguration<Tizen, FormsElement> config, bool isPulsing)
		{
			SetPulsingStatus(config.Element, isPulsing);
			return config;
		}
	}
}