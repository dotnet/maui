#nullable disable
using System;

namespace Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Maui.Controls.FlyoutPage;

	/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/FlyoutPage.xml" path="Type[@FullName='Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific.FlyoutPage']/Docs/*" />
	public static class FlyoutPage
	{
		#region CollapsedStyle

		/// <summary>Bindable property for <see cref="CollapseStyle"/>.</summary>
		public static readonly BindableProperty CollapseStyleProperty =
			BindableProperty.CreateAttached("CollapseStyle", typeof(CollapseStyle),
				typeof(FlyoutPage), CollapseStyle.Full, propertyChanged: OnCollapseStylePropertyChanged);

		static void OnCollapseStylePropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
#if WINDOWS
			if (bindable is Microsoft.Maui.Controls.FlyoutPage flyoutPage)
			{
				flyoutPage.Handler.UpdateValue(nameof(CollapseStyleProperty));
			}
#endif
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/FlyoutPage.xml" path="//Member[@MemberName='GetCollapseStyle'][1]/Docs/*" />
		public static CollapseStyle GetCollapseStyle(BindableObject element)
		{
			return (CollapseStyle)element.GetValue(CollapseStyleProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/FlyoutPage.xml" path="//Member[@MemberName='SetCollapseStyle'][1]/Docs/*" />
		public static void SetCollapseStyle(BindableObject element, CollapseStyle collapseStyle)
		{
			element.SetValue(CollapseStyleProperty, collapseStyle);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/FlyoutPage.xml" path="//Member[@MemberName='GetCollapseStyle'][2]/Docs/*" />
		public static CollapseStyle GetCollapseStyle(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (CollapseStyle)config.Element.GetValue(CollapseStyleProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/FlyoutPage.xml" path="//Member[@MemberName='SetCollapseStyle'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> SetCollapseStyle(
			this IPlatformElementConfiguration<Windows, FormsElement> config, CollapseStyle value)
		{
			config.Element.SetValue(CollapseStyleProperty, value);
			return config;
		}

		/// <param name="config">The config parameter.</param>
		public static IPlatformElementConfiguration<Windows, FormsElement> UsePartialCollapse(
			this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetCollapseStyle(config, CollapseStyle.Partial);
			return config;
		}

		#endregion

		#region CollapsedPaneWidth

		/// <summary>Bindable property for attached property <c>CollapsedPaneWidth</c>.</summary>
		public static readonly BindableProperty CollapsedPaneWidthProperty =
			BindableProperty.CreateAttached("CollapsedPaneWidth", typeof(double),
				typeof(FlyoutPage), 48d, validateValue: (bindable, value) => (double)value >= 0);

		/// <param name="element">The element parameter.</param>
		public static double GetCollapsedPaneWidth(BindableObject element)
		{
			return (double)element.GetValue(CollapsedPaneWidthProperty);
		}

		/// <param name="element">The element parameter.</param>
		/// <param name="collapsedPaneWidth">The collapsedPaneWidth parameter.</param>
		public static void SetCollapsedPaneWidth(BindableObject element, double collapsedPaneWidth)
		{
			element.SetValue(CollapsedPaneWidthProperty, collapsedPaneWidth);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/FlyoutPage.xml" path="//Member[@MemberName='CollapsedPaneWidth'][1]/Docs/*" />
		public static double CollapsedPaneWidth(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (double)config.Element.GetValue(CollapsedPaneWidthProperty);
		}

		/// <include file="../../../../docs/Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific/FlyoutPage.xml" path="//Member[@MemberName='CollapsedPaneWidth'][2]/Docs/*" />
		public static IPlatformElementConfiguration<Windows, FormsElement> CollapsedPaneWidth(
			this IPlatformElementConfiguration<Windows, FormsElement> config, double value)
		{
			config.Element.SetValue(CollapsedPaneWidthProperty, value);
			return config;
		}

		#endregion
	}
}
