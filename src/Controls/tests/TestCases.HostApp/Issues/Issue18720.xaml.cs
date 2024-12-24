namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 18720, "Setting the background property of AppCompatEditText (Entry) in a handler mapping does not work", PlatformAffected.Android)]
	public partial class Issue18720 : ContentPage
	{
		public Issue18720()
		{
			InitializeComponent();
			UpdateEntryBackgroundColor();
			UpdateEntryBackground();
		}

		void OnUpdateBackgroundColorButtonClicked(object sender, System.EventArgs e)
		{
			UpdateEntryBackgroundColor();
		}

		void OnClearBackgroundColorButtonClicked(object sender, System.EventArgs e)
		{
			BackgroundColorEntry.BackgroundColor = null;
		}

		void OnUpdateBackgroundButtonClicked(object sender, System.EventArgs e)
		{
			UpdateEntryBackground();
		}

		void OnClearBackgroundButtonClicked(object sender, System.EventArgs e)
		{
			BackgroundEntry.Background = null;
		}

		void OnTestButtonClicked(object sender, EventArgs e)
		{
			TestLayout.IsVisible = false;
		}

		void UpdateEntryBackgroundColor()
		{
			Random rnd = new Random();
			Color backgroundColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			BackgroundColorEntry.BackgroundColor = backgroundColor;
		}

		void UpdateEntryBackground()
		{
			Random rnd = new Random();
			Color startColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			Color endColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);

			BackgroundEntry.Background = new LinearGradientBrush
			{
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = startColor },
					new GradientStop { Color = endColor, Offset = 1 }
				}
			};
		}
	}

	public class Issue18720Entry1 : Entry
	{

	}

	public class Issue18720Entry2 : Entry
	{

	}

	public static class Issue18720Extensions
	{
		public static MauiAppBuilder Issue18720AddMappers(this MauiAppBuilder builder)
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Issue18720Entry1), (handler, view) =>
				{
					if (view is Issue18720Entry1)
					{
#if ANDROID
						handler.PlatformView.Background = null;
						handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Pink);
						handler.PlatformView.SetTextColor(Android.Graphics.Color.WhiteSmoke);
#endif
					}
				});

				Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping(nameof(Issue18720Entry2), (handler, view) =>
				{
					if (view is Issue18720Entry2)
					{
#if ANDROID
						Android.Graphics.Drawables.GradientDrawable gd = new Android.Graphics.Drawables.GradientDrawable();
						gd.SetCornerRadius(10);
						gd.SetStroke(2, Android.Graphics.Color.Violet);
						handler.PlatformView.Background = gd;
						handler.PlatformView.SetTextColor(Android.Graphics.Color.DeepPink);
#endif
					}
				});
			});

			return builder;
		}
	}
}