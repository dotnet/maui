using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.ManualReview)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 8088, "Mixing IsClippedToBounds settings in a single layout", PlatformAffected.Android)]
	public class MultipleClipToBounds : TestContentPage
	{
		Label _label1;
		Label _label2;
		ContentView _layout1;
		ContentView _layout2;

		Button _button1;
		Button _button2;

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label
			{
				Text = "Toggle the IsClippedToBounds settings below. Toggling the settings "
						+ "for the first layout should not affect the second layout " 
						+ "(and vice versa). If toggling the settings for one layout affects" 
						+ " the other layout, this test has failed."
			};

			var box1 = new BoxView { BackgroundColor = Color.Coral, TranslationX = -10, TranslationY = -10 };
			var box2 = new BoxView { BackgroundColor = Color.LightGreen, TranslationX = -10, TranslationY = -10 };

			_layout1 = new ContentView
			{
				BackgroundColor = Color.RosyBrown,
				Margin = new Thickness(50),
				Content = box1
			};

			_label1 = new Label();

			_layout2 = new ContentView
			{
				BackgroundColor = Color.RosyBrown,
				Margin = new Thickness(50),
				Content = box2
			};

			_label2 = new Label();

			_button1 = new Button();

			_button2 = new Button();

			_button1.Clicked += (sender, args) =>
			{
				_layout1.IsClippedToBounds = !_layout1.IsClippedToBounds;
				UpdateLabels();
			};

			_button2.Clicked += (sender, args) =>
			{
				_layout2.IsClippedToBounds = !_layout2.IsClippedToBounds;
				UpdateLabels();
			};

			layout.Children.Add(instructions);
			layout.Children.Add(_button1);
			layout.Children.Add(_button2);
			layout.Children.Add(_layout1);
			layout.Children.Add(_label1);
			layout.Children.Add(_layout2);
			layout.Children.Add(_label2);

			UpdateLabels();

			Content = layout;
		}

		void UpdateLabels()
		{
			_label1.Text =
				$"The coral Box above {(_layout1.IsClippedToBounds ? "should" : "should not")} be clipped by the brown container.";

			_label2.Text =
				$"The green Box above {(_layout2.IsClippedToBounds ? "should" : "should not")} be clipped by the brown container.";

			_button1.Text = $"Toggle L1 (currently {_layout1.IsClippedToBounds})";
			_button2.Text = $"Toggle L2 (currently {_layout2.IsClippedToBounds})";
		}
	}
}