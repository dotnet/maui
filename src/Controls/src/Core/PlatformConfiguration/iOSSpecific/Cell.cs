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

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='GetDefaultBackgroundColor']/Docs/*" />
		public static Color GetDefaultBackgroundColor(BindableObject element)
			=> (Color)element.GetValue(DefaultBackgroundColorProperty);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='SetDefaultBackgroundColor'][1]/Docs/*" />
		public static void SetDefaultBackgroundColor(BindableObject element, Color value)
			=> element.SetValue(DefaultBackgroundColorProperty, value);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='DefaultBackgroundColor']/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
		public static Color DefaultBackgroundColor(this IPlatformElementConfiguration<iOS, FormsElement> config)
#pragma warning restore CS0618 // Type or member is obsolete
			=> GetDefaultBackgroundColor(config.Element);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific/Cell.xml" path="//Member[@MemberName='SetDefaultBackgroundColor'][2]/Docs/*" />
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		public static IPlatformElementConfiguration<iOS, FormsElement> SetDefaultBackgroundColor(this IPlatformElementConfiguration<iOS, FormsElement> config, Color value)
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		{
			SetDefaultBackgroundColor(config.Element, value);
			return config;
		}
	}
}
