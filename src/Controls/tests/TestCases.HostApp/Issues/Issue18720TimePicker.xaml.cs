namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.ManualTest, "18720 TimePicker", "Setting the background property of AppCompatEditText (TimePicker) in a handler mapping does not work", PlatformAffected.Android)]
	public partial class Issue18720TimePicker : ContentPage
	{
		public Issue18720TimePicker()
		{
			InitializeComponent();
			UpdateTimePickerBackgroundColor();
			UpdateTimePickerBackground();
		}

		void OnUpdateBackgroundColorButtonClicked(object sender, EventArgs e)
		{
			UpdateTimePickerBackgroundColor();
		}

		void OnClearBackgroundColorButtonClicked(object sender, EventArgs e)
		{
			BackgroundColorTimePicker.BackgroundColor = null;
		}

		void OnUpdateBackgroundButtonClicked(object sender, EventArgs e)
		{
			UpdateTimePickerBackground();
		}

		void OnClearBackgroundButtonClicked(object sender, EventArgs e)
		{
			BackgroundTimePicker.Background = null;
		}

		void OnTestButtonClicked(object sender, EventArgs e)
		{
			TestLayout.IsVisible = false;
		}

		void UpdateTimePickerBackgroundColor()
		{
			Random rnd = new Random();
			Color backgroundColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			BackgroundColorTimePicker.BackgroundColor = backgroundColor;
		}

		void UpdateTimePickerBackground()
		{
			Random rnd = new Random();
			Color startColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			Color endColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);

			BackgroundTimePicker.Background = new LinearGradientBrush
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

	public class Issue18720TimePicker1 : TimePicker
	{

	}

	public class Issue18720TimePicker2 : TimePicker
	{

	}

	public static class Issue18720TimePickerExtensions
	{
		public static MauiAppBuilder Issue18720TimePickerAddMappers(this MauiAppBuilder builder)
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping(nameof(Issue18720TimePicker1), (handler, view) =>
				{
					if (view is Issue18720TimePicker1)
					{
#if ANDROID
						handler.PlatformView.Background = null;
						handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Pink);
						handler.PlatformView.SetTextColor(Android.Graphics.Color.WhiteSmoke);
#endif
					}
				});

				Microsoft.Maui.Handlers.TimePickerHandler.Mapper.AppendToMapping(nameof(Issue18720TimePicker2), (handler, view) =>
				{
					if (view is Issue18720TimePicker2)
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