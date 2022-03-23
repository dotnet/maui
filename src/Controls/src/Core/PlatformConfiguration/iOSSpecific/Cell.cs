namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using Microsoft.Maui.Graphics;
	using FormsElement = Maui.Controls.Cell;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Cell']/Docs" />
	public static class Cell
	{
		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='DefaultBackgroundColorProperty']/Docs" />
		public static readonly BindableProperty DefaultBackgroundColorProperty = BindableProperty.Create(nameof(DefaultBackgroundColor), typeof(Color), typeof(Cell), null);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='GetDefaultBackgroundColor']/Docs" />
		public static Color GetDefaultBackgroundColor(BindableObject element)
			=> (Color)element.GetValue(DefaultBackgroundColorProperty);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='SetDefaultBackgroundColor'][1]/Docs" />
		public static void SetDefaultBackgroundColor(BindableObject element, Color value)
			=> element.SetValue(DefaultBackgroundColorProperty, value);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='DefaultBackgroundColor']/Docs" />
		public static Color DefaultBackgroundColor(this IPlatformElementConfiguration<iOS, FormsElement> config)
			=> GetDefaultBackgroundColor(config.Element);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='SetDefaultBackgroundColor'][2]/Docs" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetDefaultBackgroundColor(this IPlatformElementConfiguration<iOS, FormsElement> config, Color value)
		{
			SetDefaultBackgroundColor(config.Element, value);
			return config;
		}
	}
}
