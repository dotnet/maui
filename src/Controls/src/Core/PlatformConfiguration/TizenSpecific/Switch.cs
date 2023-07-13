#nullable disable

namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using Microsoft.Maui.Graphics;
	using MauiElement = Maui.Controls.Switch;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Switch.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific.Switch']/Docs/*" />
	public static class Switch
	{
		/// <summary>Bindable property for <see cref="Color"/>.</summary>
		public static readonly BindableProperty ColorProperty = BindableProperty.Create(nameof(Color), typeof(Color), typeof(MauiElement), null);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Switch.xml" path="//Member[@MemberName='GetColor'][1]/Docs/*" />
		public static Color GetColor(BindableObject element)
		{
			return (Color)element.GetValue(ColorProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Switch.xml" path="//Member[@MemberName='SetColor'][1]/Docs/*" />
		public static void SetColor(BindableObject element, Color color)
		{
			element.SetValue(ColorProperty, color);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Switch.xml" path="//Member[@MemberName='GetColor'][2]/Docs/*" />
		public static Color GetColor(this IPlatformElementConfiguration<Tizen, MauiElement> config)
		{
			return GetColor(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Switch.xml" path="//Member[@MemberName='SetColor'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, MauiElement> SetColor(this IPlatformElementConfiguration<Tizen, MauiElement> config, Color color)
		{
			SetColor(config.Element, color);
			return config;
		}
	}
}
