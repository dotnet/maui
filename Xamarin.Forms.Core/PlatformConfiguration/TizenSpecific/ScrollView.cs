namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Forms.ScrollView;

	public static class ScrollView
	{
		public static readonly BindableProperty VerticalScrollStepProperty = BindableProperty.Create("VerticalScrollStep", typeof(int), typeof(FormsElement), -1, 
			coerceValue: (bindable, value) =>
			{
				return ((int)value < 0) ? -1 : value;
			});

		public static readonly BindableProperty HorizontalScrollStepProperty = BindableProperty.Create("HorizontalScrollStep", typeof(int), typeof(FormsElement), -1,
			coerceValue: (bindable, value) =>
			{
				return ((int)value < 0) ? -1 : value;
			});

		public static int GetVerticalScrollStep(BindableObject element)
		{
			return (int)element.GetValue(VerticalScrollStepProperty);
		}

		public static void SetVerticalScrollStep(BindableObject element, int scrollStep)
		{
			element.SetValue(VerticalScrollStepProperty, scrollStep);
		}

		public static int GetVerticalScrollStep(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetVerticalScrollStep(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetVerticalScrollStep(this IPlatformElementConfiguration<Tizen, FormsElement> config, int scrollStep)
		{
			SetVerticalScrollStep(config.Element, scrollStep);
			return config;
		}

		public static int GetHorizontalScrollStep(BindableObject element)
		{
			return (int)element.GetValue(HorizontalScrollStepProperty);
		}

		public static void SetHorizontalScrollStep(BindableObject element, int scrollStep)
		{
			element.SetValue(HorizontalScrollStepProperty, scrollStep);
		}

		public static int GetHorizontalScrollStep(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetHorizontalScrollStep(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetHorizontalScrollStep(this IPlatformElementConfiguration<Tizen, FormsElement> config, int scrollStep)
		{
			SetHorizontalScrollStep(config.Element, scrollStep);
			return config;
		}
	}
}
