using System;

namespace Xamarin.Forms.PlatformConfiguration.WindowsSpecific
{
	using FormsElement = Forms.FlyoutPage;

	public static class FlyoutPage
	{
		#region CollapsedStyle

		public static readonly BindableProperty CollapseStyleProperty =
			BindableProperty.CreateAttached("CollapseStyle", typeof(CollapseStyle),
				typeof(FlyoutPage), CollapseStyle.Full);

		public static CollapseStyle GetCollapseStyle(BindableObject element)
		{
			return (CollapseStyle)element.GetValue(CollapseStyleProperty);
		}

		public static void SetCollapseStyle(BindableObject element, CollapseStyle collapseStyle)
		{
			element.SetValue(CollapseStyleProperty, collapseStyle);
		}

		public static CollapseStyle GetCollapseStyle(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (CollapseStyle)config.Element.GetValue(CollapseStyleProperty);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> SetCollapseStyle(
			this IPlatformElementConfiguration<Windows, FormsElement> config, CollapseStyle value)
		{
			config.Element.SetValue(CollapseStyleProperty, value);
			return config;
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> UsePartialCollapse(
			this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			SetCollapseStyle(config, CollapseStyle.Partial);
			return config;
		}

		#endregion

		#region CollapsedPaneWidth

		public static readonly BindableProperty CollapsedPaneWidthProperty =
			BindableProperty.CreateAttached("CollapsedPaneWidth", typeof(double),
				typeof(FlyoutPage), 48d, validateValue: (bindable, value) => (double)value >= 0);

		public static double GetCollapsedPaneWidth(BindableObject element)
		{
			return (double)element.GetValue(CollapsedPaneWidthProperty);
		}

		public static void SetCollapsedPaneWidth(BindableObject element, double collapsedPaneWidth)
		{
			element.SetValue(CollapsedPaneWidthProperty, collapsedPaneWidth);
		}

		public static double CollapsedPaneWidth(this IPlatformElementConfiguration<Windows, FormsElement> config)
		{
			return (double)config.Element.GetValue(CollapsedPaneWidthProperty);
		}

		public static IPlatformElementConfiguration<Windows, FormsElement> CollapsedPaneWidth(
			this IPlatformElementConfiguration<Windows, FormsElement> config, double value)
		{
			config.Element.SetValue(CollapsedPaneWidthProperty, value);
			return config;
		}

		#endregion
	}

	[Obsolete("MasterDetailPage is obsolete as of version 5.0.0. Please use FlyoutPage instead.")]
	public static class MasterDetailPage
	{
		#region CollapsedStyle

		public static readonly BindableProperty CollapseStyleProperty = FlyoutPage.CollapseStyleProperty;

		public static CollapseStyle GetCollapseStyle(BindableObject element)
		{
			return (CollapseStyle)element.GetValue(CollapseStyleProperty);
		}

		public static void SetCollapseStyle(BindableObject element, CollapseStyle collapseStyle)
		{
			element.SetValue(CollapseStyleProperty, collapseStyle);
		}

		public static CollapseStyle GetCollapseStyle(this IPlatformElementConfiguration<Windows, Forms.MasterDetailPage> config)
		{
			return (CollapseStyle)config.Element.GetValue(CollapseStyleProperty);
		}

		public static IPlatformElementConfiguration<Windows, Forms.MasterDetailPage> SetCollapseStyle(
			this IPlatformElementConfiguration<Windows, Forms.MasterDetailPage> config, CollapseStyle value)
		{
			config.Element.SetValue(CollapseStyleProperty, value);
			return config;
		}

		public static IPlatformElementConfiguration<Windows, Forms.MasterDetailPage> UsePartialCollapse(
			this IPlatformElementConfiguration<Windows, Forms.MasterDetailPage> config)
		{
			SetCollapseStyle(config, CollapseStyle.Partial);
			return config;
		}

		#endregion

		#region CollapsedPaneWidth

		public static readonly BindableProperty CollapsedPaneWidthProperty = FlyoutPage.CollapsedPaneWidthProperty;

		public static double GetCollapsedPaneWidth(BindableObject element)
		{
			return (double)element.GetValue(CollapsedPaneWidthProperty);
		}

		public static void SetCollapsedPaneWidth(BindableObject element, double collapsedPaneWidth)
		{
			element.SetValue(CollapsedPaneWidthProperty, collapsedPaneWidth);
		}

		public static double CollapsedPaneWidth(this IPlatformElementConfiguration<Windows, Forms.MasterDetailPage> config)
		{
			return (double)config.Element.GetValue(CollapsedPaneWidthProperty);
		}

		public static IPlatformElementConfiguration<Windows, Forms.MasterDetailPage> CollapsedPaneWidth(
			this IPlatformElementConfiguration<Windows, Forms.MasterDetailPage> config, double value)
		{
			config.Element.SetValue(CollapsedPaneWidthProperty, value);
			return config;
		}

		#endregion
	}
}