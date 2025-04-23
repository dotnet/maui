namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 28945, "Add Focus propagation to MauiView", PlatformAffected.iOS)]
	public class Issue28945 : ContentPage
	{
		public Issue28945()
		{
#if IOS
			var layout = new StackLayout();

			var content = new Issue28945ContentView
			{
				BackgroundColor = Colors.Red,
				HeightRequest = 200
			};

			layout.Add(content);

			Content = layout;
#endif
		}
	}

#if IOS
	public class Issue28945ContentView : ContentView
	{
		public Issue28945ContentView()
		{
			Focused += OnFocused;

			GestureRecognizers.Add(new TapGestureRecognizer
			{
				NumberOfTapsRequired = 1,
				Command = new Command(async () =>
				{
					await this.Window.Page.DisplayAlert("Tapped", "Tapped Issue28945ContentView", "Ok");
				})
			});
		}

		async void OnFocused(object sender, FocusEventArgs e)
		{
			await Window.Page.DisplayAlert("Focused", "Works", "Ok");
		}
	}

	public class Issue28945ContentViewPlatform : Microsoft.Maui.Platform.ContentView
	{
		public Issue28945ContentViewPlatform()
		{
			UserInteractionEnabled = true;
		}

		public override bool CanBecomeFocused => true;
		public override bool CanBecomeFirstResponder => true;

		public override void DidUpdateFocus(UIKit.UIFocusUpdateContext context, UIKit.UIFocusAnimationCoordinator coordinator)
		{
			base.DidUpdateFocus(context, coordinator);

			Console.WriteLine("Issue28945ContentViewPlatform DidUpdateFocus");
		}
	}

	public class Issue28945ContentViewPlatformHandler : Microsoft.Maui.Handlers.ContentViewHandler
	{
		public Issue28945ContentViewPlatformHandler()
		{
		}

		protected override Microsoft.Maui.Platform.ContentView CreatePlatformView()
		{
			return new Issue28945ContentViewPlatform();
		}

	}
#endif

	public static class Issue28945Extensions
	{
		public static MauiAppBuilder Issue28945AddMappers(this MauiAppBuilder builder)
		{
			builder.ConfigureMauiHandlers(handlers =>
			{	
#if IOS
				handlers.AddHandler(typeof(Issue28945ContentView), typeof(Issue28945ContentViewPlatformHandler));
#endif
			});

			return builder;
		}
	}
}