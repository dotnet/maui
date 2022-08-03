namespace Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific
{
	using System;
	using FormsCell = Maui.Controls.Cell;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ViewCell.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific.ViewCell']/Docs" />
	public static class ViewCell
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ViewCell.xml" path="//Member[@MemberName='IsContextActionsLegacyModeEnabledProperty']/Docs" />
		public static readonly BindableProperty IsContextActionsLegacyModeEnabledProperty = BindableProperty.Create("IsContextActionsLegacyModeEnabled", typeof(bool), typeof(Maui.Controls.ViewCell), false, propertyChanged: OnIsContextActionsLegacyModeEnabledPropertyChanged);

		private static void OnIsContextActionsLegacyModeEnabledPropertyChanged(BindableObject element, object oldValue, object newValue)
		{
			var cell = element as FormsCell;
			cell.IsContextActionsLegacyModeEnabled = (bool)newValue;
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ViewCell.xml" path="//Member[@MemberName='GetIsContextActionsLegacyModeEnabled'][1]/Docs" />
		public static bool GetIsContextActionsLegacyModeEnabled(BindableObject element)
		{
			return (bool)element.GetValue(IsContextActionsLegacyModeEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ViewCell.xml" path="//Member[@MemberName='SetIsContextActionsLegacyModeEnabled'][1]/Docs" />
		public static void SetIsContextActionsLegacyModeEnabled(BindableObject element, bool value)
		{
			element.SetValue(IsContextActionsLegacyModeEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ViewCell.xml" path="//Member[@MemberName='GetIsContextActionsLegacyModeEnabled'][2]/Docs" />
		public static bool GetIsContextActionsLegacyModeEnabled(this IPlatformElementConfiguration<Android, FormsCell> config)
		{
			return GetIsContextActionsLegacyModeEnabled(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific/ViewCell.xml" path="//Member[@MemberName='SetIsContextActionsLegacyModeEnabled'][2]/Docs" />
		public static IPlatformElementConfiguration<Android, FormsCell> SetIsContextActionsLegacyModeEnabled(this IPlatformElementConfiguration<Android, FormsCell> config, bool value)
		{
			SetIsContextActionsLegacyModeEnabled(config.Element, value);
			return config;
		}
	}
}
