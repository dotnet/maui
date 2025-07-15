using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 30403, "Image under WinUI does not respect VerticalOptions and HorizontalOptions", PlatformAffected.UWP)]
    public partial class Issue30403 : ContentPage
    {
        public Issue30403()
        {
            InitializeComponent();
        }
    }
}