#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific
{
	using FormsElement = Maui.Controls.Label;

	/// <summary>Provides access to the font weight for labels on the Tizen platform.</summary>
	public static class Label
	{
		/// <summary>Bindable property for <see cref="FontWeight"/>.</summary>
		public static readonly BindableProperty FontWeightProperty = BindableProperty.Create("FontWeight", typeof(string), typeof(FormsElement), FontWeight.None);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Label.xml" path="//Member[@MemberName='GetFontWeight'][1]/Docs/*" />
		public static string GetFontWeight(BindableObject element)
		{
			return (string)element.GetValue(FontWeightProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Label.xml" path="//Member[@MemberName='SetFontWeight'][1]/Docs/*" />
		public static void SetFontWeight(BindableObject element, string weight)
		{
			element.SetValue(FontWeightProperty, weight);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Label.xml" path="//Member[@MemberName='GetFontWeight'][2]/Docs/*" />
		public static string GetFontWeight(this IPlatformElementConfiguration<Tizen, FormsElement> config)
		{
			return GetFontWeight(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.TizenSpecific/Label.xml" path="//Member[@MemberName='SetFontWeight'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Tizen, FormsElement> SetFontWeight(this IPlatformElementConfiguration<Tizen, FormsElement> config, string weight)
		{
			SetFontWeight(config.Element, weight);
			return config;
		}
	}
}
