#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific
{
	using Microsoft.Maui.Graphics;
	using FormsElement = Maui.Controls.Cell;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Cell']/Docs/*" />
	public static class Cell
	{
		/// <summary>Bindable property for <see cref="DefaultBackgroundColor"/>.</summary>
		public static readonly BindableProperty DefaultBackgroundColorProperty = BindableProperty.Create(nameof(DefaultBackgroundColor), typeof(Color), typeof(Cell), null);

		/// <param name="element">The element parameter.</param>
		public static Color GetDefaultBackgroundColor(BindableObject element)
			=> (Color)element.GetValue(DefaultBackgroundColorProperty);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='SetDefaultBackgroundColor'][1]/Docs/*" />
		public static void SetDefaultBackgroundColor(BindableObject element, Color value)
			=> element.SetValue(DefaultBackgroundColorProperty, value);

		/// <param name="config">The config parameter.</param>
		public static Color DefaultBackgroundColor(this IPlatformElementConfiguration<iOS, FormsElement> config)
			=> GetDefaultBackgroundColor(config.Element);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='SetDefaultBackgroundColor'][2]/Docs/*" />
		public static IPlatformElementConfiguration<iOS, FormsElement> SetDefaultBackgroundColor(this IPlatformElementConfiguration<iOS, FormsElement> config, Color value)
		{
			SetDefaultBackgroundColor(config.Element, value);
			return config;
		}
	}
}
