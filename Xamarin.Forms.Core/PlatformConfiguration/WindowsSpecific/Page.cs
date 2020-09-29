using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Forms.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Forms.Page;

	public static class Page
	{
		#region ToolbarPlacement

		public static readonly BindableProperty ToolbarPlacementProperty =
			BindableProperty.CreateAttached("ToolbarPlacement", typeof(ToolbarPlacement),
				typeof(FormsElement), ToolbarPlacement.Default);

		public static ToolbarPlacement GetToolbarPlacement(BindableObject element)
		{
			return (ToolbarPlacement)element.GetValue(ToolbarPlacementProperty);
		}

		public static void SetToolbarPlacement(BindableObject element, ToolbarPlacement toolbarPlacement)
		{
			element.SetValue(ToolbarPlacementProperty, toolbarPlacement);
		}

		public static ToolbarPlacement GetToolbarPlacement(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (ToolbarPlacement)config.Element.GetValue(ToolbarPlacementProperty);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetToolbarPlacement(
			this IPlatformElementConfiguration<Windows, FormsElement> config, ToolbarPlacement value)
		{
			config.Element.SetValue(ToolbarPlacementProperty, value);
			return config;
		}

		#endregion

		#region ToolbarDynamicOverflowEnabled

		public static readonly BindableProperty ToolbarDynamicOverflowEnabledProperty =
			BindableProperty.CreateAttached("ToolbarDynamicOverflowEnabled", typeof(bool),
				typeof(FormsElement), true);

		public static bool GetToolbarDynamicOverflowEnabled(BindableObject element)
		{
			return (bool)element.GetValue(ToolbarDynamicOverflowEnabledProperty);
		}

		public static void SetToolbarDynamicOverflowEnabled(BindableObject element, bool value)
		{
			element.SetValue(ToolbarDynamicOverflowEnabledProperty, value);
		}

		public static bool GetToolbarDynamicOverflowEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (bool)config.Element.GetValue(ToolbarDynamicOverflowEnabledProperty);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetToolbarDynamicOverflowEnabled(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			config.Element.SetValue(ToolbarDynamicOverflowEnabledProperty, value);
			return config;
		}

		#endregion
	}
}