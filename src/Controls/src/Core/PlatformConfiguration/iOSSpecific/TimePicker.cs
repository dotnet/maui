#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using FormsElement = Microsoft.Maui.Controls.TimePicker;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/TimePicker.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.TimePicker']/Docs/*" />
	public static class TimePicker
	{
		/// <summary>Bindable property for <see cref="UpdateMode"/>.</summary>
		public static readonly BindableProperty UpdateModeProperty = BindableProperty.Create(
			nameof(UpdateMode),
			typeof(UpdateMode),
			typeof(TimePicker),
			default(UpdateMode));

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/TimePicker.xml" path="//Member[@MemberName='GetUpdateMode']/Docs/*" />
		public static UpdateMode GetUpdateMode(BindableObject element)
		{
			return (UpdateMode)element.GetValue(UpdateModeProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/TimePicker.xml" path="//Member[@MemberName='SetUpdateMode'][1]/Docs/*" />
		public static void SetUpdateMode(BindableObject element, UpdateMode value)
		{
			element.SetValue(UpdateModeProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/TimePicker.xml" path="//Member[@MemberName='UpdateMode']/Docs/*" />
		public static UpdateMode UpdateMode(this IPlatformElementConfiguration<iOS, FormsElement> config)
		{
			return GetUpdateMode(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/TimePicker.xml" path="//Member[@MemberName='SetUpdateMode'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetUpdateMode(this IPlatformElementConfiguration<iOS, FormsElement> config, UpdateMode value)
		{
			SetUpdateMode(config.Element, value);
			return config;
		}
	}
}
