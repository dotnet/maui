namespace Xamarin.Forms.PlatformConfiguration.macOSSpecific
{
	using FormsElement = Forms.Page;

	public static class Page
	{
		#region TabsStyle
		public static readonly BindableProperty TabOrderProperty = BindableProperty.Create("TabOrder", typeof(VisualElement[]), typeof(Page), null);

		public static VisualElement[] GetTabOrder(BindableObject element)
		{
			return (VisualElement[])element.GetValue(TabOrderProperty);
		}

		public static void SetTabOrder(BindableObject element, params VisualElement[] value)
		{
			element.SetValue(TabOrderProperty, value);
		}

		public static VisualElement[] GetTabOrder(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetTabOrder(config.Element);
		}

		public static IPlatformElementConfiguration<macOS, FormsElement> SetTabOrder(this IPlatformElementConfiguration<macOS, FormsElement> config, params VisualElement[] value)
		{
			SetTabOrder(config.Element, value);
			return config;
		}
		#endregion
	}
}