namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.RefreshView;

	public static class RefreshView
	{
		public enum RefreshPullDirection
		{
			LeftToRight,
			TopToBottom,
			RightToLeft,
			BottomToTop
		}

		public static readonly BindableProperty RefreshPullDirectionProperty = BindableProperty.Create("RefreshPullDirection", typeof(RefreshPullDirection), typeof(FormsElement), RefreshPullDirection.TopToBottom);

		public static void SetRefreshPullDirection(BindableObject element, RefreshPullDirection value)
		{
			element.SetValue(RefreshPullDirectionProperty, value);
		}

		public static RefreshPullDirection GetRefreshPullDirection(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetRefreshPullDirection(config.Element);
		}

		public static RefreshPullDirection GetRefreshPullDirection(BindableObject element)
		{
			return (RefreshPullDirection)element.GetValue(RefreshPullDirectionProperty);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetRefreshPullDirection(
			this IPlatformElementConfiguration<Windows, FormsElement> config, RefreshPullDirection value)
		{
			SetRefreshPullDirection(config.Element, value);
			return config;
		}
	}

}