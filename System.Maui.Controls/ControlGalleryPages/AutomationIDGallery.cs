using System;

namespace Xamarin.Forms.Controls
{
	public class AutomationIdGallery : ContentPage
	{

		public AutomationIdGallery ()
		{
			var scrollView = new ScrollView { AutomationId = "scrollMain" };
			var rootLayout = new StackLayout { AutomationId = "stckMain" };

			var btn = new Button {
				AutomationId = "btnTest1",
				Text = "Test1",
				Command = new Command (async () => await Navigation.PushModalAsync (new TestPage1 ()))
			};

			var btn2 = new Button {
				AutomationId = "btnTest2",
				Text = "Test2",
				Command = new Command (async () => await Navigation.PushModalAsync (new TestPage2 ()))
			};
	
			rootLayout.Children.Add (btn);
			rootLayout.Children.Add (btn2);

			var toolBarItem = new ToolbarItem { AutomationId = "tbItemHello", Text= "Hello", Command = new Command(async ()=> { await DisplayAlert("Hello","","ok"); }) };
			var toolBarItem2 = new ToolbarItem { AutomationId = "tbItemHello2", Order= ToolbarItemOrder.Secondary, Text= "Hello2", Command = new Command(async ()=> { await DisplayAlert("Hello2","","ok"); }) };

			ToolbarItems.Add (toolBarItem);
			ToolbarItems.Add (toolBarItem2);

			scrollView.Content = rootLayout;
			Content = scrollView;
		}

		internal class TestPage1 : ContentPage
		{
			public TestPage1 ()
			{
                var rootLayout = new StackLayout { AutomationId = "stckMain" , Padding = new Thickness(10, 50, 10, 0) };
				var btn = new Button {
					AutomationId = "popModal",
					Text = "Pop",
					Command = new Command (async () => await Navigation.PopModalAsync ())
				};
				rootLayout.Children.Add (btn);
				rootLayout.Children.Add (new ActivityIndicator { AutomationId = "actHello", IsRunning = true });
				rootLayout.Children.Add (new BoxView {
					AutomationId = "bxvHello",
					WidthRequest = 40,
					HeightRequest = 40,
					BackgroundColor = Color.Red
				});
				rootLayout.Children.Add (new Button { AutomationId = "btnHello", Text = "Hello" });
				rootLayout.Children.Add (new DatePicker { AutomationId = "dtPicker", Date = DateTime.Parse ("01/01/2014") });
				rootLayout.Children.Add (new TimePicker { AutomationId = "tPicker", Time = new TimeSpan (14, 45, 50)  });
				rootLayout.Children.Add (new Label { AutomationId = "lblHello", Text = "Hello Label" });
				rootLayout.Children.Add (new Editor { AutomationId = "editorHello", Text = "Hello Editor" });
				rootLayout.Children.Add (new Entry { AutomationId = "entryHello", Text = "Hello Entry" });

				Content = rootLayout;
			}
		}

		internal class TestPage2 : ContentPage
		{
			public TestPage2 ()
			{
                var rootLayout = new StackLayout { AutomationId = "stckMain" , Padding = new Thickness(10, 50, 10, 0) };
				var btn = new Button {
					AutomationId = "popModal",
					Text = "Pop",
					Command = new Command (async () => await Navigation.PopModalAsync ())
				};
				rootLayout.Children.Add (btn);
				rootLayout.Children.Add (new Image { AutomationId = "imgHello", Source = "menuIcon" });
				rootLayout.Children.Add (new ListView {
					AutomationId = "lstView",
					ItemsSource = new string[2] { "one", "two" },
					HeightRequest = 50
				});
				rootLayout.Children.Add (new Picker { AutomationId = "pickerHello", Items = { "one", "two" } });
				rootLayout.Children.Add (new ProgressBar { AutomationId = "progressHello", Progress = 2 });
				rootLayout.Children.Add (new SearchBar { AutomationId = "srbHello", Text = "Hello Search" });
				rootLayout.Children.Add (new Slider { AutomationId = "sliHello", Value = 0.5 });
				rootLayout.Children.Add (new Stepper { AutomationId = "stepperHello", Value = 5 });
				rootLayout.Children.Add (new Switch { AutomationId = "switchHello" });
				rootLayout.Children.Add (new WebView {
					AutomationId = "webviewHello",
					WidthRequest = 100,
					HeightRequest = 50,
					Source = new UrlWebViewSource { Url = "http://blog.xamarin.com/" }
				});
				
				Content = rootLayout;
			}
		}


	}
}

