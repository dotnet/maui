
namespace Xamarin.Forms.Controls
{
	public class AccessibilityGallery : ContentPage
	{
		public AccessibilityGallery()
		{
			// https://developer.xamarin.com/guides/android/advanced_topics/accessibility/
			// https://developer.xamarin.com/guides/ios/advanced_topics/accessibility/
			// https://msdn.microsoft.com/en-us/windows/uwp/accessibility/basic-accessibility-information

			const string EntryPlaceholder = "Your name.";
			const string EntryHint = "Type your name.";
			const string ImageName = "Roof";
			const string ImageHint = "Tap to show an alert.";
			const string BoxHint = "Shows a purple box.";
			const string BoxName = "Box";

			string screenReader = "";
			string scrollFingers = "";
			string explore = "";
			string labeledByInstructions = "";
			string imageInstructions = "";
			string boxInstructions = "";

			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					screenReader = "VoiceOver";
					scrollFingers = "three fingers";
					explore = "Use two fingers to swipe up or down the screen to read all of the elements on this page.";
					labeledByInstructions = $"The following Entry should read aloud \"{EntryPlaceholder}.\", plus native instructions on how to use an Entry element. This text comes from the placeholder.";
					imageInstructions = $"The following Image should read aloud \"{ImageName}. {ImageHint}\". You should be able to tap the image and hear an alert box.";
					boxInstructions = $"The following Box should read aloud \"{BoxName}. {BoxHint}\". You should be able to tap the box and hear an alert box.";
					break;
				case Device.Android:
					screenReader = "TalkBack";
					scrollFingers = "two fingers";
					explore = "Drag one finger across the screen to read each element on the page.";
					labeledByInstructions = $"The following Entry should read aloud \"EditBox {EntryPlaceholder} for {EntryHint}.\", plus native instructions on how to use an Entry element. This text comes from the Entry placeholder and text of the Label.";
					imageInstructions = $"The following Image should read aloud \"{ImageName}. {ImageHint}\". You should be able to tap the image and hear an alert box.";
					boxInstructions = $"The following Box should read aloud \"{BoxName}. {BoxHint}\". You should be able to tap the box and hear an alert box.";
					break;
				case Device.WinRT:
				case Device.UWP:
				case Device.WinPhone:
					screenReader = "Narrator";
					scrollFingers = "two fingers";
					explore = "Use three fingers to swipe up the screen to read all of the elements on this page.";
					labeledByInstructions = $"The following Entry should read aloud \"{EntryHint}\", plus native instructions on how to use an Entry element. This text comes from the text of the label.";
					imageInstructions = $"The following Image should read aloud \"{ImageName}. {ImageHint}\". Windows does not currently support TapGestures while the Narrator is active.";
					boxInstructions = $"The following Box should read aloud \"{BoxName}. {BoxHint}\". Windows does not currently support TapGestures while the Narrator is active.";
					break;
				default:
					screenReader = "the native screen reader";
					break;
			}

			var instructions = new Label { Text = $"Please enable {screenReader}. {explore} Use {scrollFingers} to scroll the view. Tap an element once to hear the name and description and native instructions. Double tap anywhere on the screen to activate the selected element. Swipe left or right with one finger to switch to the previous or next element." };

			var instructions2 = new Label { Text = labeledByInstructions };
			var entryLabel = new Label { Text = EntryHint, VerticalOptions = LayoutOptions.Center };
			var entry = new Entry { Placeholder = EntryPlaceholder };
			entry.SetAccessibilityLabeledBy(entryLabel);

			var entryGroup = new Grid();
			entryGroup.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			entryGroup.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			entryGroup.AddChild(entryLabel, 0, 0);
			entryGroup.AddChild(entry, 1, 0);



			var activityIndicator = new ActivityIndicator();
			activityIndicator.SetAccessibilityName("Progress indicator");


			const string ButtonText = "Update progress";
			const string ButtonHint = "Tap to start/stop the activity indicator.";
			var instructions3 = new Label { Text = $"The following Button should read aloud \"{ButtonText}.\", plus native instructions on how to use a button." };
			var button = new Button { Text = ButtonText };
			button.SetAccessibilityHint(ButtonHint);
			button.Clicked += (sender, e) =>
			{
				activityIndicator.IsRunning = !activityIndicator.IsRunning;
				activityIndicator.SetAccessibilityHint(activityIndicator.IsRunning ? "Running." : "Not running");
			};


			var instructions4 = new Label { Text = imageInstructions };
			var image = new Image { Source = "photo.jpg" };
			// The tap gesture will NOT work on Win
			image.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => DisplayAlert("Success", "You tapped the image", "OK")) });
			image.SetAccessibilityName(ImageName);
			image.SetAccessibilityHint(ImageHint);
			// Images are ignored by default on iOS (at least, Forms Images are); 
			// make accessible in order to enable the gesture and narration
			image.SetIsInAccessibleTree(true);


			var instructions6 = new Label { Text = boxInstructions };
			var boxView = new BoxView { Color = Color.Purple };
			// The tap gesture will NOT work on Win
			boxView.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => DisplayAlert("Success", "You tapped the box", "OK")) });
			boxView.SetAccessibilityName(BoxName);
			boxView.SetAccessibilityHint(BoxHint);
			// BoxViews are ignored by default on iOS and Win; 
			// make accessible in order to enable the gesture and narration
			boxView.SetIsInAccessibleTree(true);


			var stack = new StackLayout
			{
				Children =
				{
					instructions,
					instructions2,
					entryGroup,
					instructions3,
					button,
					activityIndicator,
					instructions4,
					image,
					boxView
				}
			};

			var scrollView = new ScrollView { Content = stack };
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

		public static void SetAccessibilityLabeledBy(this VisualElement element, Element value)
		{
			element.SetValue(Accessibility.LabeledByProperty, value);
		}

		public static Element GetAccessibilityLabeledBy(this VisualElement element)
		{
			return (Element)element.GetValue(Accessibility.LabeledByProperty);
		}
	}
}
