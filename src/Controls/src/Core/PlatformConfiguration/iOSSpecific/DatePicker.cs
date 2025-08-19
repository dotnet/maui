#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Microsoft.Maui.Controls.DatePicker;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/DatePicker.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.DatePicker']/Docs/*" />
	public static class DatePicker
	{
		/// <summary>Bindable property for <see cref="UpdateMode"/>.</summary>
		public static readonly BindableProperty UpdateModeProperty = BindableProperty.Create(
			nameof(UpdateMode),
			typeof(UpdateMode),
			typeof(DatePicker),
			default(UpdateMode));

		/// <param name="element">The element parameter.</param>
		public static UpdateMode GetUpdateMode(BindableObject element)
		{
			return (UpdateMode)element.GetValue(UpdateModeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/DatePicker.xml" path="//Member[@MemberName='SetUpdateMode'][1]/Docs/*" />
		public static void SetUpdateMode(BindableObject element, UpdateMode value)
		{
			element.SetValue(UpdateModeProperty, value);
		}

		/// <param name="config">The config parameter.</param>
		public static UpdateMode UpdateMode(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetUpdateMode(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/DatePicker.xml" path="//Member[@MemberName='SetUpdateMode'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetUpdateMode(this IPlatformElementConfiguration<iOS, FormsElement> config, UpdateMode value)
		{
			SetUpdateMode(config.Element, value);
			return config;
		}
	}
}
