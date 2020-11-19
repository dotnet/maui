namespace Xamarin.Forms.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Forms.ItemsView;

	public static class ItemsView
	{
		public static readonly BindableProperty FocusedItemScrollPositionProperty = BindableProperty.Create("FocusedItemScrollPosition", typeof(ScrollToPosition), typeof(FormsElement), ScrollToPosition.MakeVisible);


		public static ScrollToPosition GetFocusedItemScrollPosition(BindableObject element)
		{
			return (ScrollToPosition)element.GetValue(FocusedItemScrollPositionProperty);
		}

		public static void SetFocusedItemScrollPosition(BindableObject element, ScrollToPosition position)
		{
			element.SetValue(FocusedItemScrollPositionProperty, position);
		}

		public static ScrollToPosition GetFocusedItemScrollPosition(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetFocusedItemScrollPosition(config.Element);
		}

		public static IPlatformElementConfiguration<Tizen, FormsElement> SetFocusedItemScrollPosition(this IPlatformElementConfiguration<Tizen, FormsElement> config, ScrollToPosition position)
		{
			SetFocusedItemScrollPosition(config.Element, position);
			return config;
		}
	}
}
