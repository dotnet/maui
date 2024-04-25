using CommunityToolkit.Maui.Views;
using Mopups.Pages;
using Mopups.PreBaked.Services;
using Mopups.Services;
using Microsoft.Maui.Controls;
using System;

namespace WebViewCrashMidLoad
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            MopupService.Instance.PushAsync(new PopupPage1());
        }
    }

}
