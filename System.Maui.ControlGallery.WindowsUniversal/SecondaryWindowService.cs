using System;
using System.Threading.Tasks;
using global::Windows.ApplicationModel.Core;
using global::Windows.UI.Core;
using global::Windows.UI.ViewManagement;
using global::Windows.UI.Xaml;
using System.Maui;
using System.Maui.ControlGallery.WindowsUniversal;
using System.Maui.Controls;
using System.Maui.Platform.UWP;

[assembly: Dependency(typeof(SecondaryWindowService))]
namespace System.Maui.ControlGallery.WindowsUniversal
{
	class SecondaryWindowService : ISecondaryWindowService
	{
		public async Task OpenSecondaryWindow(Type pageType)
		{
			CoreApplicationView newView = CoreApplication.CreateNewView();
			int newViewId = 0;
			await newView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				var frame = new global::Windows.UI.Xaml.Controls.Frame();

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
