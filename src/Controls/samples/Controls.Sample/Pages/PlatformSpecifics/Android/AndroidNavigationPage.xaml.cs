using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
    public partial class AndroidNavigationPage : NavigationPage
    {
        public AndroidNavigationPage(Page page)
        {
            InitializeComponent();
            PushAsync(page);
        }
    }
}