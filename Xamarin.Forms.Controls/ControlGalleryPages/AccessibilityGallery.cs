
namespace Xamarin.Forms.Controls
{
	public class AccessibilityGallery : ContentPage
	{
		public AccessibilityGallery()
		{
			// https://developer.xamarin.com/guides/android/advanced_topics/accessibility/
			// https://developer.xamarin.com/guides/ios/advanced_topics/accessibility/
			// https://msdn.microsoft.com/en-us/windows/uwp/accessibility/basic-accessibility-information

			string screenReader = "";
			string scrollFingers = "";
			string explore = "";

			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					screenReader = "VoiceOver";
					scrollFingers = "three fingers";
					explore = "Use two fingers to swipe up or down the screen to read all of the elements on this page.";
					break;
				case Device.Android:
					screenReader = "TalkBack";
					scrollFingers = "two fingers";
					explore = "Drag one finger across the screen to read each element on the page.";
					break;
				case Device.Windows:
				case Device.WinPhone:
					screenReader = "Narrator";
					scrollFingers = "two fingers";
					break;
				default:
					screenReader = "the native screen reader";
					break;
			}

			var instructions = new Label { Text = $"Please enable {screenReader}. {explore} Use {scrollFingers} to scroll the view. Tap an element once to hear the description. Double tap anywhere on the screen to activate the selected element. Swipe left or right with one finger to switch to the previous or next element." };

			const string EntryPlaceholder = "Your name";
			const string EntryHint = "Type your name.";

			var instructions2 = new Label { Text = $"The following Entry should read aloud \"{EntryPlaceholder}. {EntryHint}\", plus native instructions on how to use an entry element. Note that Android will NOT read the Hint if a Placeholder is provided." };
			var entry = new Entry { Placeholder = EntryPlaceholder };
			entry.SetAccessibilityHint(EntryHint);


			var activityIndicator = new ActivityIndicator();
			activityIndicator.SetAccessibilityName("Progress indicator");


			const string ButtonText = "Update progress";
			var instructions3 = new Label { Text = $"The following Button should read aloud \"{ButtonText}\", plus native instructions on how to use a button." };
			var button = new Button { Text = ButtonText };
			button.Clicked += (sender, e) =>
			{
				activityIndicator.IsRunning = !activityIndicator.IsRunning;
				activityIndicator.SetAccessibilityHint(activityIndicator.IsRunning ? "Running." : "Not running");
			};


			const string ImageHint = "Tap to show an alert.";
			var instructions4 = new Label { Text = $"The following Image should read aloud \"{ImageHint}\". You should be able to tap the image and hear an alert box." };
			var image = new Image { Source = "photo.jpg" };
			image.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => DisplayAlert("Success", "You tapped the image", "OK")) });
			image.SetAccessibilityHint(ImageHint);
			// images are ignored by default on iOS (at least, Forms images are); 
			// make accessible in order to enable the gesture and narration
			image.SetIsInAccessibleTree(true);


			var instructions5 = new Label { Text = $"The following Button should NOT be read aloud, nor should you be able to interact with it while {screenReader} is active." };
			var button2 = new Button { Text = "I am not accessible" };
			// setting this to false seems to have no effect on any platform
			button2.SetIsInAccessibleTree(false);


			var boxView = new BoxView { Color = Color.Purple };
			boxView.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => DisplayAlert("Success", "You tapped the box", "OK")) });
			boxView.SetAccessibilityName("Box");
			boxView.SetAccessibilityHint("Shows a purple box.");
			//boxView.SetIsInAccessibleTree(true);

			var stack = new StackLayout
			{
				Children =
				{
					instructions,
					instructions2,
					entry,
					instructions3,
					button,
					activityIndicator,
					instructions4,
					image,
					instructions5,
					button2,
					boxView
				}
			};

			var scrollView = new ScrollView { Content = stack };

			// TODO: Test Pan/Pinch gestures
			// TODO: Test CarouselView

			Content = scrollView;
		}

	}


	public static class AccessibilityExtensions
	{
		public static void SetAccessibilityName(this VisualElement element, string name)
		{
			element.SetValue(Accessibility.NameProperty, name);
		}

		public static string GetAccessibilityName(this VisualElement element)
		{
			return (string)element.GetValue(Accessibility.NameProperty);
		}

		public static void SetAccessibilityHint(this VisualElement element, string hint)
		{
			element.SetValue(Accessibility.HintProperty, hint);
		}

		public static string GetAccessibilityHint(this VisualElement element)
		{
			return (string)element.GetValue(Accessibility.HintProperty);
		}

		public static void SetIsInAccessibleTree(this VisualElement element, bool value)
		{
			element.SetValue(Accessibility.IsInAccessibleTreeProperty, value);
		}

		public static bool GetIsInAccessibleTree(this VisualElement element)
		{
			return (bool)element.GetValue(Accessibility.IsInAccessibleTreeProperty);
		}
	}
}
