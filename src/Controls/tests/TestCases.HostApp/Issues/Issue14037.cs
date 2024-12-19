#if ANDROID
using System.Globalization;
using System.Text;
using Android.App;
using AndroidX.Activity.Result;
using Maui.Controls.Sample.Platform;

namespace Maui.Controls.Sample.Issues;

[Issue(
	IssueTracker.Github,
	14037,
	"MauiAppCompatActivity.AllowFragmentRestore=False prevents getting result from Activity",
	PlatformAffected.Android)]
public class Issue14037 : TestContentPage
{
	protected Issue14037ViewModel ViewModel { get; private set; }

	protected override void Init()
	{
		Title = nameof(Issue14037);
		ViewModel = new Issue14037ViewModel();
		BindingContext = ViewModel;

		var button = new Button
		{
			AutomationId = "LaunchActivityForResult",
			Text = "Launch Activity For Result",
		};
		button.SetBinding(Button.CommandProperty, nameof(ViewModel.LaunchActivityForResultCommand));

		var result = new Label
		{
			AutomationId = "Result",
		};
		result.SetBinding(Label.TextProperty, nameof(ViewModel.ActivityResult), converter: new ActivityResultToStringConverter());

		Content = new StackLayout
		{
			Margin = 25,
			Spacing = 10,
			Children =
			{
				new Label
				{
					AutomationId = "DescriptionValue",
					Text = "This test will launch an activity for a result. " +
						   "If it succeeds, the result will " +
						   "be printed here. To ensure this works in cases where " +
						   "the main activity is destroyed, please enable the 'always_finish_activities' " +
						   "global setting on the Android device/emulator.",
					LineBreakMode = LineBreakMode.WordWrap,
				},
				button,
				result,
			}
		};
	}
}

public class Issue14037ViewModel : ViewModelBase
{
	public Command LaunchActivityForResultCommand { get; }

	public Issue14037ViewModel()
	{
		LaunchActivityForResultCommand = new Command(OnLaunchActivityForResult);
	}

	public ActivityResult ActivityResult
	{
		get => GetProperty<ActivityResult>();
		set => SetProperty(value);
	}

	private async void OnLaunchActivityForResult() => ActivityResult = await Issue14037LifecycleObserver.Launch();
}

public class ActivityResultToStringConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		if (value is ActivityResult result && result.Data is not null)
		{
			var sb = new StringBuilder()
				.AppendLine("ActivityResult:")
				.AppendLine($"  ResultCode: {Enum.GetName(typeof(Result), result.ResultCode)}")
				.AppendLine($"  Data:")
				.AppendLine($"    Extras:");

			foreach(var key in result.Data.Extras.KeySet())
			{
				sb.AppendLine($"      Key: {key}");
				sb.AppendLine($"      Value: {result.Data.Extras.GetString(key)}");
			}

			return sb.ToString();
		}

		return string.Empty;
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
#endif
