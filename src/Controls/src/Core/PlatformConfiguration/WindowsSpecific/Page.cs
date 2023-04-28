#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.Page;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Page.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.Page']/Docs/*" />
	public static class Page
	{
		#region ToolbarPlacement

		/// <summary>Bindable property for <see cref="ToolbarPlacement"/>.</summary>
		public static readonly BindableProperty ToolbarPlacementProperty =
			BindableProperty.CreateAttached("ToolbarPlacement", typeof(ToolbarPlacement),
				typeof(FormsElement), ToolbarPlacement.Default);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Page.xml" path="//Member[@MemberName='GetToolbarPlacement'][1]/Docs/*" />
		public static ToolbarPlacement GetToolbarPlacement(BindableObject element)
		{
			return (ToolbarPlacement)element.GetValue(ToolbarPlacementProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Page.xml" path="//Member[@MemberName='SetToolbarPlacement'][1]/Docs/*" />
		public static void SetToolbarPlacement(BindableObject element, ToolbarPlacement toolbarPlacement)
		{
			element.SetValue(ToolbarPlacementProperty, toolbarPlacement);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Page.xml" path="//Member[@MemberName='GetToolbarPlacement'][2]/Docs/*" />
		public static ToolbarPlacement GetToolbarPlacement(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (ToolbarPlacement)config.Element.GetValue(ToolbarPlacementProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Page.xml" path="//Member[@MemberName='SetToolbarPlacement'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetToolbarPlacement(
			this IPlatformElementConfiguration<Windows, FormsElement> config, ToolbarPlacement value)
		{
			config.Element.SetValue(ToolbarPlacementProperty, value);
			return config;
		}

		#endregion

		#region ToolbarDynamicOverflowEnabled

		/// <summary>Bindable property for attached property <c>ToolbarDynamicOverflowEnabled</c>.</summary>
		public static readonly BindableProperty ToolbarDynamicOverflowEnabledProperty =
			BindableProperty.CreateAttached("ToolbarDynamicOverflowEnabled", typeof(bool),
				typeof(FormsElement), true);

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Page.xml" path="//Member[@MemberName='GetToolbarDynamicOverflowEnabled'][1]/Docs/*" />
		public static bool GetToolbarDynamicOverflowEnabled(BindableObject element)
		{
			return (bool)element.GetValue(ToolbarDynamicOverflowEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Page.xml" path="//Member[@MemberName='SetToolbarDynamicOverflowEnabled'][1]/Docs/*" />
		public static void SetToolbarDynamicOverflowEnabled(BindableObject element, bool value)
		{
			element.SetValue(ToolbarDynamicOverflowEnabledProperty, value);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Page.xml" path="//Member[@MemberName='GetToolbarDynamicOverflowEnabled'][2]/Docs/*" />
		public static bool GetToolbarDynamicOverflowEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (bool)config.Element.GetValue(ToolbarDynamicOverflowEnabledProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/Page.xml" path="//Member[@MemberName='SetToolbarDynamicOverflowEnabled'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetToolbarDynamicOverflowEnabled(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			config.Element.SetValue(ToolbarDynamicOverflowEnabledProperty, value);
			return config;
		}

		#endregion
	}
}
