namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 28945, "Add Focus propagation to MauiView", PlatformAffected.iOS)]
	public class Issue28945 : ContentPage
	{

	}

#if IOS
	public class Issue28945ContentView : ContentView
	{
		public Issue28945ContentView()
		{
			Focused += OnFocused;

			var layout = new StackLayout();

			var button = new Button
			{
				Text = "Set the focus"
			};

			button.Clicked += (sender, args) =>
			{
				this.Focus();
			};

			Content = layout;
		}

		async void OnFocused(object sender, FocusEventArgs e)
		{
			await Window.Page.DisplayAlert("Focused", "Works", "Ok");
		}
	}

	public class Issue28945ContentViewPlatform : Microsoft.Maui.Platform.ContentView
	{
		public override bool CanBecomeFocused => true;
		public override bool CanBecomeFirstResponder => true;
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