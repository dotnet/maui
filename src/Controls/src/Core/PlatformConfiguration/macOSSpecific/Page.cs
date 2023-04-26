#nullable disable
namespace Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific
{
	using FormsElement = Maui.Controls.Page;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/Page.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific.Page']/Docs/*" />
	public static class Page
	{
		#region TabsStyle
		/// <summary>Bindable property for attached property <c>TabOrder</c>.</summary>
		public static readonly BindableProperty TabOrderProperty = BindableProperty.Create("TabOrder", typeof(VisualElement[]), typeof(Page), null);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/Page.xml" path="//Member[@MemberName='GetTabOrder'][1]/Docs/*" />
		public static VisualElement[] GetTabOrder(BindableObject element)
		{
			return (VisualElement[])element.GetValue(TabOrderProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/Page.xml" path="//Member[@MemberName='SetTabOrder'][1]/Docs/*" />
		public static void SetTabOrder(BindableObject element, params VisualElement[] value)
		{
			element.SetValue(TabOrderProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/Page.xml" path="//Member[@MemberName='GetTabOrder'][2]/Docs/*" />
		public static VisualElement[] GetTabOrder(this IPlatformElementConfiguration<macOS, FormsElement> config)
		{
			return GetTabOrder(config.Element);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.macOSSpecific/Page.xml" path="//Member[@MemberName='SetTabOrder'][2]/Docs/*" />
		public static IPlatformElementConfiguration<macOS, FormsElement> SetTabOrder(this IPlatformElementConfiguration<macOS, FormsElement> config, params VisualElement[] value)
		{
			SetTabOrder(config.Element, value);
			return config;
		}
		#endregion
	}
}
