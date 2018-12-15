using System;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;
using Xamarin.Forms;
using Xamarin.Forms.Platform.MacOS;

namespace Samples.Mac
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : FormsApplicationDelegate
    {
        NSWindow window;

        public AppDelegate()
        {
            var style = NSWindowStyle.Closable | NSWindowStyle.Resizable | NSWindowStyle.Titled;

            var screenSize = NSScreen.MainScreen.Frame.Size;
            var rect = new CGRect(0, 0, 1024, 768);
            rect.Offset((screenSize.Width - rect.Width) / 2, (screenSize.Height - rect.Height) / 2);

            window = new NSWindow(rect, style, NSBackingStore.Buffered, false)
            {
                Title = "Xamarin.Essentials",
                TitleVisibility = NSWindowTitleVisibility.Hidden,
            };
        }

        public override NSWindow MainWindow => window;

        public override void DidFinishLaunching(NSNotification notification)
        {
            Forms.Init();

            // LoadApplication(new App());

            // HACK: There appears to be some issue with the ListView on macOS,
            //       so we are just going to use the menu bar for now with a
            //       dummy start/home page.
            LoadMenuBasedSampleApp();

            base.DidFinishLaunching(notification);
        }

        void LoadMenuBasedSampleApp()
        {
            var nav = new NavigationPage(new ContentPage
            {
                Title = "Xamarin.Essentials",
                Content = new Label
                {
                    Text = "Select a sample from the \"Samples\" menu...",
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                },
            });
            LoadApplication(new App { MainPage = nav });

            // load the menu
            var mainMenu = NSApplication.SharedApplication.MainMenu;
            var samplesMenu = mainMenu.ItemWithTitle("Samples");

            // deselect the samples when we return to the home page
            nav.Popped += (s, e) =>
            {
                if (nav.StackDepth == 1)
                {
                    SelectSampleMenuItem(null);
                }
            };

            // add the samples to the main menu
            var home = new ViewModel.HomeViewModel();
            var allSamples = home.FilteredItems.ToList();
            for (var i = 0; i < allSamples.Count; i++)
            {
                var sample = allSamples[i];
                var menuItem = new NSMenuItem(sample.Name, OnSampleSelected);
                menuItem.Tag = i;
                menuItem.ToolTip = sample.Description;
                samplesMenu.Submenu.AddItem(menuItem);
            }

            async void OnSampleSelected(object sender, EventArgs e)
            {
                if (sender is NSMenuItem menuItem)
                {
                    SelectSampleMenuItem(menuItem);

                    var sample = allSamples[(int)menuItem.Tag];
                    await nav.PushAsync((Page)Activator.CreateInstance(sample.PageType));
                }
            }

            void SelectSampleMenuItem(NSMenuItem menuItem)
            {
                // deselect previous
                foreach (var mi in samplesMenu.Submenu.Items)
                {
                    mi.State = NSCellStateValue.Off;
                }

                // select this one
                if (menuItem != null)
                {
                    menuItem.State = NSCellStateValue.On;
                }
            }
        }

        public override bool ApplicationShouldTerminateAfterLastWindowClosed(NSApplication sender) => true;
    }
}
