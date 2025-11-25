using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Pages
{
	public partial class WindowsCollapseWidthAdjusterPage : ContentPage
	{
		public static readonly BindableProperty ParentPageProperty = BindableProperty.Create("ParentPage", typeof(Microsoft.Maui.Controls.FlyoutPage), typeof(WindowsCollapseWidthAdjusterPage), null, propertyChanged: OnParentPagePropertyChanged);

		public Microsoft.Maui.Controls.FlyoutPage ParentPage
		{
			get { return (Microsoft.Maui.Controls.FlyoutPage)GetValue(ParentPageProperty); }
			set { SetValue(ParentPageProperty, value); }
		}

		public WindowsCollapseWidthAdjusterPage()
		{
			InitializeComponent();
		}

		void OnChangeButtonClicked(object sender, EventArgs e)
		{
			double width;
			if (double.TryParse(entry.Text, out width))
			{
				ParentPage.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().CollapsedPaneWidth(width);
			}
		}

		static void OnParentPagePropertyChanged(BindableObject element, object oldValue, object newValue)
		{
			if (newValue != null)
			{
				var instance = (WindowsCollapseWidthAdjusterPage)element;
				instance!.entry.Text = instance.ParentPage.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().CollapsedPaneWidth().ToString();
			}
		}
	}
}
