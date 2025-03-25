#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.Page;

	/// <summary>
	/// Provides the Page Windows Platform-Specific Functionality.
	/// </summary>
	public static class Page
	{
		#region ToolbarPlacement

		/// <summary>
		/// Backing store for the attached property that controls the placement of the toolbar.
		/// </summary>
		public static readonly BindableProperty ToolbarPlacementProperty =
			BindableProperty.CreateAttached("ToolbarPlacement", typeof(ToolbarPlacement),
				typeof(FormsElement), ToolbarPlacement.Default);

		/// <summary>
		/// Returns a value that controls the placement of the toolbar.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <returns>A value that controls the placement of the toolbar.</returns>
		public static ToolbarPlacement GetToolbarPlacement(BindableObject element)
		{
			return (ToolbarPlacement)element.GetValue(ToolbarPlacementProperty);
		}

		/// <summary>
		/// Sets a value that controls the placement of the toolbar.
		/// </summary>
		/// <param name="element">The platform specific element on which to perform the operation.</param>
		/// <param name="toolbarPlacement">The new toolbar placement.</param>
		public static void SetToolbarPlacement(BindableObject element, ToolbarPlacement toolbarPlacement)
		{
			element.SetValue(ToolbarPlacementProperty, toolbarPlacement);
		}

		/// <summary>
		/// Returns a value that controls the placement of the toolbar.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns>A value that controls the placement of the toolbar.</returns>
		public static ToolbarPlacement GetToolbarPlacement(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (ToolbarPlacement)config.Element.GetValue(ToolbarPlacementProperty);
		}

		/// <summary>
		/// Sets a value that controls the placement of the toolbar.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">The new property value to assign.</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetToolbarPlacement(
			this IPlatformElementConfiguration<Windows, FormsElement> config, ToolbarPlacement value)
		{
			config.Element.SetValue(ToolbarPlacementProperty, value);
			return config;
		}

		#endregion

		#region ToolbarDynamicOverflowEnabled

		/// <summary>
		/// Indicates whether toolbar items automatically move to the overflow menu when space is limited.
		/// </summary>
		public static readonly BindableProperty ToolbarDynamicOverflowEnabledProperty =
			BindableProperty.CreateAttached("ToolbarDynamicOverflowEnabled", typeof(bool),
				typeof(FormsElement), true);

		/// <summary>
		/// Gets a value that indicates whether toolbar items automatically move to the overflow menu when space is limited.
		/// </summary>
		/// <param name="element">A page, the <see cref="VisualElement"/> that occupies the entire screen.</param>
		/// <returns><see langword="true"/> if toolbar items automatically move to the overflow menu when space is limited; otherwise, <see langword="false"/>.</returns>
		public static bool GetToolbarDynamicOverflowEnabled(BindableObject element)
		{
			return (bool)element.GetValue(ToolbarDynamicOverflowEnabledProperty);
		}

		/// <summary>
		/// Sets a value that indicates whether toolbar items automatically move to the overflow menu when space is limited.
		/// </summary>
		/// <param name="element">A page, the <see cref="VisualElement"/> that occupies the entire screen.</param>
		/// <param name="value">A value that indicates whether toolbar items automatically move to the overflow menu when space is limited</param>
		public static void SetToolbarDynamicOverflowEnabled(BindableObject element, bool value)
		{
			element.SetValue(ToolbarDynamicOverflowEnabledProperty, value);
		}

		/// <summary>
		/// Gets a value that indicates whether toolbar items automatically move to the overflow menu when space is limited.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <returns><see langword="true"/> if toolbar items automatically move to the overflow menu when space is limited; otherwise, <see langword="false"/>.</returns>
		public static bool GetToolbarDynamicOverflowEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (bool)config.Element.GetValue(ToolbarDynamicOverflowEnabledProperty);
		}

		/// <summary>
		/// Sets a value that indicates whether toolbar items automatically move to the overflow menu when space is limited.
		/// </summary>
		/// <param name="config">The platform specific configuration that contains the element on which to perform the operation.</param>
		/// <param name="value">A value that indicates whether toolbar items automatically move to the overflow menu when space is limited</param>
		/// <returns>The updated configuration object on which developers can make successive method calls.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetToolbarDynamicOverflowEnabled(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			config.Element.SetValue(ToolbarDynamicOverflowEnabledProperty, value);
			return config;
		}

		#endregion
	}
}
