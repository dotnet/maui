using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.Platform.WPF;

namespace Xamarin.Forms.ControlGallery.WPF
{
	// To test this issue modify the csproj to modify the Application entry point.
	// <StartupObject>Xamarin.Forms.ControlGallery.WPF.Issue11923</StartupObject>
	// And uncomment the static Main method.
	class Issue11923
	{
		/*
		[STAThread]
		static void Main(string[] args)
		{
			var application = new System.Windows.Application();
			Forms.SetFlags("CarouselView_Experimental", "MediaElement_Experimental", "RadioButton_Experimental");
			FormsMaps.Init("");
			Forms.Init();
			var formsApplicationPage = new FormsApplicationPage();
			formsApplicationPage.LoadApplication(new Controls.App());

			Task.Run(() =>
			{
				Task.Delay(1000).Wait();

				application.Dispatcher.InvokeAsync(() =>
				{
					var window = new Window()
					{
						Height = 600,
						Width = 600
					};

					var mainPage = new Issue1();
					var formsContentLoader = new FormsContentLoader();
					var content = formsContentLoader.LoadContentAsync(window, null, mainPage, new CancellationToken()).Result;

					window.Content = content;
					window.Show();
				});
			});

			application.Run();
		}
		*/
	}
}
