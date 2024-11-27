namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.ManualTest, "18720 DatePicker", "Setting the background property of AppCompatEditText (DatePicker) in a handler mapping does not work", PlatformAffected.Android)]
	public partial class Issue18720DatePicker : ContentPage
	{
		public Issue18720DatePicker()
		{
			InitializeComponent();
			UpdateDatePickerBackgroundColor();
			UpdateDatePickerBackground();
		}

		void OnUpdateBackgroundColorButtonClicked(object sender, EventArgs e)
		{
			UpdateDatePickerBackgroundColor();
		}

		void OnClearBackgroundColorButtonClicked(object sender, EventArgs e)
		{
			BackgroundColorDatePicker.BackgroundColor = null;
		}

		void OnUpdateBackgroundButtonClicked(object sender, EventArgs e)
		{
			UpdateDatePickerBackground();
		}

		void OnClearBackgroundButtonClicked(object sender, EventArgs e)
		{
			BackgroundDatePicker.Background = null;
		}

		void OnTestButtonClicked(object sender, EventArgs e)
		{
			TestLayout.IsVisible = false;
		}

		void UpdateDatePickerBackgroundColor()
		{
			Random rnd = new Random();
			Color backgroundColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			BackgroundColorDatePicker.BackgroundColor = backgroundColor;
		}

		void UpdateDatePickerBackground()
		{
			Random rnd = new Random();
			Color startColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);
			Color endColor = Color.FromRgba(rnd.Next(256), rnd.Next(256), rnd.Next(256), 255);

			BackgroundDatePicker.Background = new LinearGradientBrush
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

	public class Issue18720DatePicker1 : DatePicker
	{

	}

	public class Issue18720DatePicker2 : DatePicker
	{

	}

	public static class Issue18720DatePickerExtensions
	{
		public static MauiAppBuilder Issue18720DatePickerAddMappers(this MauiAppBuilder builder)
		{
			builder.ConfigureMauiHandlers(handlers =>
			{
				Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(nameof(Issue18720DatePicker1), (handler, view) =>
				{
					if (view is Issue18720DatePicker1)
					{
#if ANDROID
						handler.PlatformView.Background = null;
						handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Pink);
						handler.PlatformView.SetTextColor(Android.Graphics.Color.WhiteSmoke);
#endif
					}
				});

				Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping(nameof(Issue18720DatePicker2), (handler, view) =>
				{
					if (view is Issue18720DatePicker2)
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