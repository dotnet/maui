namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, "18720 Editor", "Setting the background property of AppCompatEditText (Editor) in a handler mapping does not work", PlatformAffected.Android)]
	public partial class Issue18720Editor : ContentPage
	{
		public Issue18720Editor()
		{
			InitializeComponent();
			UpdateEditorBackgroundColor();
			UpdateEditorBackground();
		}

		void OnUpdateBackgroundColorButtonClicked(object sender, System.EventArgs e)
		{
			UpdateEditorBackgroundColor();
		}

		void OnClearBackgroundColorButtonClicked(object sender, System.EventArgs e)
		{
			BackgroundColorEditor.BackgroundColor = null;
		}

		void OnUpdateBackgroundButtonClicked(object sender, System.EventArgs e)
		{
			UpdateEditorBackground();
		}

		void OnClearBackgroundButtonClicked(object sender, System.EventArgs e)
		{
			BackgroundEditor.Background = null;
		}

		void OnTestButtonClicked(object sender, EventArgs e)
		{
			TestLayout.IsVisible = false;
		}

		void UpdateEditorBackgroundColor()
		{
			Random rnd = new Random();
			Color backgroundColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			BackgroundColorEditor.BackgroundColor = backgroundColor;
		}

		void UpdateEditorBackground()
		{
			Random rnd = new Random();
			Color startColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			Color endColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);

			BackgroundEditor.Background = new LinearGradientBrush
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

	public class Issue18720Editor1 : Editor
	{

	}

	public class Issue18720Editor2 : Editor
	{

	}

	public static class Issue18720EditorExtensions
	{
		public static MauiAppBuilder Issue18720EditorAddMappers(this MauiAppBuilder builder)
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(nameof(Issue18720Editor1), (handler, view) =>
				{
					if (view is Issue18720Editor1)
					{
#if ANDROID
						handler.PlatformView.Background = null;
						handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Pink);
						handler.PlatformView.SetTextColor(Android.Graphics.Color.WhiteSmoke);
#endif
					}
				});

				Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping(nameof(Issue18720Editor2), (handler, view) =>
				{
					if (view is Issue18720Editor2)
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