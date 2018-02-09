using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1625, "Slider value is not changed for the first position change", PlatformAffected.Android)]
	public class Github1625 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		protected override void Init()
		{
			var slider = new Slider();
			slider.Maximum = 10;
			slider.Minimum = 1;
			slider.Value = 5;

			var valueLabel = new Label();
			var stack = new StackLayout { Orientation = StackOrientation.Vertical, Spacing = 15 };

			valueLabel.SetBinding(Label.TextProperty, new Binding("Value", source: slider));
			stack.Children.Add(valueLabel);
			stack.Children.Add(slider);

			var button = new Button
			{
				Text = "Set to 7",
				Command = new Command(() => slider.Value = 7)
			};
			stack.Children.Add(button);

			var label = new Label
			{
				Text = "On start, slider value should show 5 even though SeekBar is 4.996. Sliding back and forth should update the label and the tracker. Tapping on a slider position should do the same. Tapping on the button should show 7 even though SeekBar is 6.994."
			};
			stack.Children.Add(label);

			Content = stack;
		}
	}
}