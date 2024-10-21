using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using AButton = Android.Widget.Button;

namespace Maui.Controls.Sample.Platform;

[Activity(Label = nameof(Issue14037Activity))]
public class Issue14037Activity : Activity
{
	protected override void OnCreate(Bundle savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		SetContentView(BuildContentView());
	}

	private LinearLayout BuildContentView()
	{
		var extraKeyInput = BuildTextInput("Extra Key:");
		var extraValueInput = BuildTextInput("Extra Value:");

		var button = new AButton(this)
		{
			Text = "Return Result",
		};
		button.Click += (sender, e) =>
		{
			var resultIntent = new Intent();
			var extra_key = GetInputValue(extraKeyInput);
			var extra_value = GetInputValue(extraValueInput);

			resultIntent.PutExtra(extra_key, extra_value);
			SetResult(Result.Ok, resultIntent);
			Finish();
		};

		var layout = new LinearLayout(this)
		{
			Orientation = Orientation.Vertical,
		};

		layout.AddView(extraKeyInput);
		layout.AddView(extraValueInput);
		layout.AddView(button);

		return layout;
	}

	private LinearLayout BuildTextInput(string labelText)
	{
		var label = new TextView(this)
		{
			Text = labelText,
		};

		var value = new EditText(this)
		{
			LayoutParameters = new LinearLayout.LayoutParams(0, LinearLayout.LayoutParams.WrapContent, 1)
		};

		var layout = new LinearLayout(this)
		{
			Orientation = Orientation.Horizontal,
			LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent),
		};

		layout.AddView(label);
		layout.AddView(value);

		return layout;
	}

	private string GetInputValue(LinearLayout layout)
	{
		if (layout.GetChildAt(1) is EditText input)
		{
			return input.Text;
		}

		return string.Empty;
	}
}