using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Xamarin.Forms;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows;
using Microsoft.Maui.Controls.Compatibility.ControlGallery;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;

[assembly: Dependency(typeof(SecondaryWindowService))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Windows
{
	class SecondaryWindowService : ISecondaryWindowService
	{
		public async Task OpenSecondaryWindow(Type pageType)
		{
			CoreApplicationView newView = CoreApplication.CreateNewView();
			int newViewId = 0;
			await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				var frame = new Microsoft.UI.Xaml.Controls.Frame();

				//The page instance must be created inside the new UI Thread.
				ContentPage instance = (ContentPage)Activator.CreateInstance(pageType);
				frame.Navigate(instance);
				Window.Current.Content = frame;
				Window.Current.Activate();

				newViewId = ApplicationView.GetForCurrentView().Id;
			});
			bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(newViewId);
		}
	}
}
