#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.TabbedPage;

	/// <summary>Provides control over header icons on the Windows platform.</summary>
	public static class TabbedPage
	{
		/// <summary>Bindable property for attached property <c>HeaderIconsEnabled</c>.</summary>
		public static readonly BindableProperty HeaderIconsEnabledProperty =
			BindableProperty.Create(nameof(HeaderIconsEnabledProperty), typeof(bool), typeof(TabbedPage), true);

		/// <summary>Bindable property for attached property <c>HeaderIconsSize</c>.</summary>
		public static readonly BindableProperty HeaderIconsSizeProperty =
			BindableProperty.Create(nameof(HeaderIconsSizeProperty), typeof(Size), typeof(TabbedPage), new Size(16, 16));

		/// <summary>Sets whether tab header icons are displayed on Windows.</summary>
		/// <param name="element">The element to configure.</param>
		/// <param name="value"><see langword="true"/> to enable header icons.</param>
		public static void SetHeaderIconsEnabled(BindableObject element, bool value)
		{
			element.SetValue(HeaderIconsEnabledProperty, value);
		}

		/// <summary>Gets whether tab header icons are displayed on Windows.</summary>
		/// <param name="element">The element to query.</param>
		/// <returns><see langword="true"/> if header icons are enabled.</returns>
		public static bool GetHeaderIconsEnabled(BindableObject element)
		{
			return (bool)element.GetValue(HeaderIconsEnabledProperty);
		}

		/// <summary>Gets whether tab header icons are displayed on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns><see langword="true"/> if header icons are enabled.</returns>
		public static bool GetHeaderIconsEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetHeaderIconsEnabled(config.Element);
		}

		/// <summary>Sets whether tab header icons are displayed on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value"><see langword="true"/> to enable header icons.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetHeaderIconsEnabled(
			this IPlatformElementConfiguration<Windows, FormsElement> config, bool value)
		{
			SetHeaderIconsEnabled(config.Element, value);
			return config;
		}

		/// <summary>Returns a Boolean value that tells whether header icons are enabled.</summary>
		/// <param name="config">The platform configuration for the element on which to perform the operation.</param>
		/// <returns><see langword="true"/> if header icons are enabled. Otherwise, <see langword="false"/>.</returns>
		public static bool IsHeaderIconsEnabled(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetHeaderIconsEnabled(config.Element);
		}

		/// <summary>Enables header icons.</summary>
		/// <param name="config">The platform configuration for the element on which to perform the operation.</param>
		public static void EnableHeaderIcons(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetHeaderIconsEnabled(config.Element, true);
		}

		/// <summary>Disables header icons.</summary>
		/// <param name="config">The platform configuration for the element on which to perform the operation.</param>
		public static void DisableHeaderIcons(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetHeaderIconsEnabled(config.Element, false);
		}

		/// <summary>Sets the size of tab header icons on Windows.</summary>
		/// <param name="element">The element to configure.</param>
		/// <param name="value">The icon size.</param>
		public static void SetHeaderIconsSize(BindableObject element, Size value)
		{
			element.SetValue(HeaderIconsSizeProperty, value);
		}

		/// <summary>Gets the size of tab header icons on Windows.</summary>
		/// <param name="element">The element to query.</param>
		/// <returns>The icon size.</returns>
		public static Size GetHeaderIconsSize(BindableObject element)
		{
			return (Size)element.GetValue(HeaderIconsSizeProperty);
		}

		/// <summary>Gets the size of tab header icons on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <returns>The icon size.</returns>
		public static Size GetHeaderIconsSize(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return GetHeaderIconsSize(config.Element);
		}

		/// <summary>Sets the size of tab header icons on Windows.</summary>
		/// <param name="config">The platform configuration.</param>
		/// <param name="value">The icon size.</param>
		/// <returns>The updated platform configuration.</returns>
		public static IPlatformElementConfiguration<Windows, FormsElement> SetHeaderIconsSize(
			this IPlatformElementConfiguration<Windows, FormsElement> config, Size value)
		{
			SetHeaderIconsSize(config.Element, value);
			return config;
		}

	}
}
