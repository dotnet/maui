using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 31191, "SafeArea keyboard overlap calculation should only inset by actual covered area", PlatformAffected.iOS)]
    public partial class Issue31191 : ContentPage
    {
        private bool _keyboardVisible;

        public Issue31191()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            UpdateSafeAreaDisplay();
        }

        private void OnTestButtonClicked(object sender, EventArgs e)
        {
            // Update the display to show current state
            UpdateSafeAreaDisplay();
        }

        private void OnEntryFocused(object sender, FocusEventArgs e)
        {
            _keyboardVisible = true;
            
            // Add a small delay to allow keyboard animation to complete
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500);
                UpdateSafeAreaDisplay();
            });
        }

        private void OnEntryUnfocused(object sender, FocusEventArgs e)
        {
            _keyboardVisible = false;
            
            // Add a small delay to allow keyboard animation to complete
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500);
                UpdateSafeAreaDisplay();
            });
        }

        private void UpdateSafeAreaDisplay()
        {
            if (SafeAreaLabel != null)
            {
                var safeAreaInsets = this.On<iOS>().SafeAreaInsets();
                SafeAreaLabel.Text = $"Safe Area Bottom: {safeAreaInsets.Bottom:F1}, Keyboard: {(_keyboardVisible ? "Visible" : "Hidden")}";
            }
        }
    }
}