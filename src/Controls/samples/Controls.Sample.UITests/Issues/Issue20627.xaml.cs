using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Mopups.Services;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 20627, "WebView error if we leave page while it's navigating", PlatformAffected.iOS)]
	public partial class Issue20627 : ContentPage
    {
        public Issue20627()
        {
            InitializeComponent();
        }

        void OnOpenPopupClicked(object sender, EventArgs e)
        {
            MopupService.Instance.PushAsync(new Issue20627Popup());
        }
    }

}
