namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using MauiElement = Controls.Switch;
#pragma warning disable RS0016 // Add public types and members to the declared API

	public static class Switch
	{
		/// <summary>Bindable property for <c>ShowStatusLabel</c>.</summary>
		public static readonly BindableProperty ShowStatusLabelProperty = BindableProperty.Create("ShowStatusLabel", typeof(bool), typeof(MauiElement), true);

		public static bool GetShowStatusLabel(BindableObject element)
		{
			return (bool)element.GetValue(ShowStatusLabelProperty);
		}

		public static void SetShowStatusLabel(BindableObject element, bool showLabel)
		{
			element.SetValue(ShowStatusLabelProperty, showLabel);
		}

		public static bool GetShowStatusLabel(this IPlatformElementConfiguration<Windows, MauiElement> config)
		{
			return GetShowStatusLabel(config.Element);
		}

		public static IPlatformElementConfiguration<Windows, MauiElement> SetShowStatusLabel(this IPlatformElementConfiguration<Windows, MauiElement> config, bool showLabel)
		{
			SetShowStatusLabel(config.Element, showLabel);
			return config;
		}
#pragma warning restore RS0016 // Add public types and members to the declared API

	}
}
