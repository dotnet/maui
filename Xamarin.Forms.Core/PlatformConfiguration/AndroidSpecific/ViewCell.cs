namespace Xamarin.Forms.PlatformConfiguration.AndroidSpecific
{
	using System;
	using FormsCell = Forms.Cell;

	public static class ViewCell
	{
		public static readonly BindableProperty IsContextActionsLegacyModeEnabledProperty = BindableProperty.Create("IsContextActionsLegacyModeEnabled", typeof(bool), typeof(Forms.ViewCell), false, propertyChanged: OnIsContextActionsLegacyModeEnabledPropertyChanged);

		private static void OnIsContextActionsLegacyModeEnabledPropertyChanged(BindableObject element, object oldValue, object newValue)
		{
			var cell = element as FormsCell;
			cell.IsContextActionsLegacyModeEnabled = (bool)newValue;
		}

		public static bool GetIsContextActionsLegacyModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsContextActionsLegacyModeEnabledProperty);
		}

		public static void SetIsContextActionsLegacyModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsContextActionsLegacyModeEnabledProperty, value);
		}

		public static bool GetIsContextActionsLegacyModeEnabled(this IPlatformElementConfiguration<Android, FormsCell> config)
		{
			return GetIsContextActionsLegacyModeEnabled(config.Element);
		}

		public static IPlatformElementConfiguration<Android, FormsCell> SetIsContextActionsLegacyModeEnabled(this IPlatformElementConfiguration<Android, FormsCell> config, bool value)
		{
			SetIsContextActionsLegacyModeEnabled(config.Element, value);
			return config;
		}
	}
}