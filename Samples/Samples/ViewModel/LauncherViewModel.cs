using System;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
    public class LauncherViewModel : BaseViewModel
    {
        public string LaunchUri { get; set; }

        public ICommand LaunchCommand { get; }

        public ICommand CanLaunchCommand { get; }

        public ICommand LaunchMailCommand { get; }

        public ICommand LaunchBrowserCommand { get; }

        public LauncherViewModel()
        {
            LaunchCommand = new Command(OnLaunch);
            LaunchMailCommand = new Command(OnLaunchMail);
            LaunchBrowserCommand = new Command(OnLaunchBrowser);
            CanLaunchCommand = new Command(CanLaunch);
        }

        async void OnLaunchBrowser()
        {
            await Launcher.OpenAsync("https://github.com/xamarin/Essentials");
        }

        async void OnLaunch()
        {
            try
            {
                await Launcher.OpenAsync(LaunchUri);
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync($"Uri {LaunchUri} could not be launched: {ex}");
            }
        }

        async void OnLaunchMail()
        {
            await Launcher.OpenAsync("mailto:");
        }

        async void CanLaunch()
        {
            try
            {
                var canBeLaunched = await Launcher.CanOpenAsync(LaunchUri);
                await DisplayAlertAsync($"Uri {LaunchUri} can be launched: {canBeLaunched}");
            }
            catch (Exception ex)
            {
                await DisplayAlertAsync($"Uri {LaunchUri} could not be verified as launchable: {ex}");
            }
        }
    }
}
