using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Maui.Controls.Sample.Pages
{
    public partial class CollapseWidthAdjusterContentView : ContentView
    {
        public static readonly BindableProperty ParentPageProperty = BindableProperty.Create("ParentPage", typeof(Microsoft.Maui.Controls.FlyoutPage), typeof(CollapseWidthAdjusterContentView), null, propertyChanged:OnParentPagePropertyChanged);

        public Microsoft.Maui.Controls.FlyoutPage ParentPage
        {
            get { return (Microsoft.Maui.Controls.FlyoutPage)GetValue(ParentPageProperty); }
            set { SetValue(ParentPageProperty, value); }
        }

        public CollapseWidthAdjusterContentView()
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
                var instance = element as CollapseWidthAdjusterContentView;
                instance.entry.Text = instance.ParentPage.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().CollapsedPaneWidth().ToString();
            }
        }
    }
}
