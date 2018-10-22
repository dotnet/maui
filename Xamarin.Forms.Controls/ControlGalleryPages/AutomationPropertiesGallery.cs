
namespace Xamarin.Forms.Controls
{
	public class AutomationPropertiesGallery : ContentPage
	{
		public AutomationPropertiesGallery()
		{
			// https://developer.xamarin.com/guides/android/advanced_topics/accessibility/
			// https://developer.xamarin.com/guides/ios/advanced_topics/accessibility/
			// https://msdn.microsoft.com/en-us/windows/uwp/accessibility/basic-accessibility-information

			const string EntryPlaceholder = "Your name.";
			const string EntryHelpText = "Type your name.";
			const string ImageName = "Roof";
			const string ImageHelpText = "Tap to show an alert.";
			const string BoxHelpText = "Shows a purple box.";
			const string BoxName = "Box";

			string screenReader = "";
			string scrollFingers = "";
			string explore = "";
			string labeledByInstructions = "";
			string imageInstructions = "";
			string boxInstructions = "";
			string toolbarInstructions = "";
			string toolbarItemName = "Get some coffee";
			string toolbarItem2Text = "Go";
			string toolbarItemHint2 = "Somewhere else";

			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					screenReader = "VoiceOver";
					scrollFingers = "three fingers";
					explore = "Use two fingers to swipe up or down the screen to read all of the elements on this page.";
					labeledByInstructions = $"The following Entry should read aloud \"{EntryPlaceholder}.\", plus native instructions on how to use an Entry element. This text comes from the placeholder.";
					imageInstructions = $"The following Image should read aloud \"{ImageName}. {ImageHelpText}\". You should be able to tap the image and hear an alert box.";
					boxInstructions = $"The following Box should read aloud \"{BoxName}. {BoxHelpText}\". You should be able to tap the box and hear an alert box.";
					toolbarInstructions = $"The Toolbar should have a coffee cup icon. Activating the coffee cup should read aloud \"{toolbarItemName}\". The Toolbar should also show the text \"{toolbarItem2Text}\". Activating this item should read aloud \"{toolbarItem2Text}. {toolbarItemHint2}\".";
					break;
				case Device.Android:
					screenReader = "TalkBack";
					scrollFingers = "two fingers";
					explore = "Drag one finger across the screen to read each element on the page.";
					labeledByInstructions = $"The following Entry should read aloud \"EditBox {EntryPlaceholder} for {EntryHelpText}.\", plus native instructions on how to use an Entry element. This text comes from the Entry placeholder and text of the Label.";
					imageInstructions = $"The following Image should read aloud \"{ImageName}. {ImageHelpText}\". You should be able to tap the image and hear an alert box.";
					boxInstructions = $"The following Box should read aloud \"{BoxName}. {BoxHelpText}\". You should be able to tap the box and hear an alert box.";
					toolbarInstructions = $"The Toolbar should have a coffee cup icon. Activating the coffee cup should read aloud \"{toolbarItemName}\". The Toolbar should also show the text \"{toolbarItem2Text}\". Activating this item should read aloud \"{toolbarItem2Text}\".";
					break;
				case Device.UWP:
					screenReader = "Narrator";
					scrollFingers = "two fingers";
					explore = "Use three fingers to swipe up the screen to read all of the elements on this page.";
					labeledByInstructions = $"The following Entry should read aloud \"{EntryHelpText}\", plus native instructions on how to use an Entry element. This text comes from the text of the label.";
					imageInstructions = $"The following Image should read aloud \"{ImageName}. {ImageHelpText}\". Windows does not currently support TapGestures while the Narrator is active.";
					boxInstructions = $"The following Box should read aloud \"{BoxName}. {BoxHelpText}\". Windows does not currently support TapGestures while the Narrator is active.";
					toolbarInstructions = $"The Toolbar should have a coffee cup icon. Activating the coffee cup should read aloud \"{toolbarItemName}\". The Toolbar should also show the text \"{toolbarItem2Text}\". Activating this item should read aloud \"{toolbarItem2Text}. {toolbarItemHint2}\".";
					break;
				default:
					screenReader = "the native screen reader";
					break;
			}

			var instructions = new Label { Text = $"Please enable {screenReader}. {explore} Use {scrollFingers} to scroll the view. Tap an element once to hear the name and HelpText and native instructions. Double tap anywhere on the screen to activate the selected element. Swipe left or right with one finger to switch to the previous or next element." };
			Title = "Accessibility";
			NavigationPage.SetBackButtonTitle(this, Title);

			NavigationPage.SetHasBackButton(this, true);
			this.SetAutomationPropertiesName("Accessibility Gallery Page");
			this.SetAutomationPropertiesHelpText("Demonstrates accessibility settings");

			var toolbarItem = new ToolbarItem { Icon = "coffee.png" };
			toolbarItem.SetAutomationPropertiesName(toolbarItemName);
			ToolbarItems.Add(toolbarItem);
			toolbarItem.Command = new Command(() => { Navigation.PushAsync(new ContentPage()); });
			var toolbarItem2 = new ToolbarItem { Text = toolbarItem2Text };
			toolbarItem2.SetAutomationPropertiesHelpText(toolbarItemHint2);
			ToolbarItems.Add(toolbarItem2);

			var toolbarInstructionsLbl = new Label { Text = toolbarInstructions };
			var instructions2 = new Label { Text = labeledByInstructions };
			var entryLabel = new Label { Text = EntryHelpText, VerticalOptions = LayoutOptions.Center };
			var entry = new Entry { Placeholder = EntryPlaceholder };
			entry.SetAutomationPropertiesLabeledBy(entryLabel);

			var entryGroup = new Grid();
			entryGroup.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Auto) });
			entryGroup.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			entryGroup.AddChild(entryLabel, 0, 0);
			entryGroup.AddChild(entry, 1, 0);


			var textCellA = new TextCell
			{
				Text = "A"
			};
			textCellA.SetAutomationPropertiesName("Option A");

			var textCellB = new TextCell
			{
				Text = "B"
			};
			textCellB.SetAutomationPropertiesName("Option B");

			var textCellC = new TextCell
			{
				Text = "C"
			};
			textCellC.SetAutomationPropertiesName("Option C");

			var textCellD = new TextCell
			{
				Text = "D"
			};
			textCellD.SetAutomationPropertiesName("Option D");

			TableView tbl = new TableView
			{
				Intent = TableIntent.Menu,
				Root = new TableRoot
					{
						new TableSection("TableView")
						{
							textCellA,
							textCellB,
							textCellC,
							textCellD,
						}
					}
			};


			var activityIndicator = new ActivityIndicator();
			activityIndicator.SetAutomationPropertiesName("Progress indicator");


			const string ButtonText = "Update progress";
			const string ButtonHelpText = "Tap to start/stop the activity indicator.";
			var instructions3 = new Label { Text = $"The following Button should read aloud \"{ButtonText}.\", plus native instructions on how to use a button." };
			var button = new Button { Text = ButtonText };
			button.SetAutomationPropertiesHelpText(ButtonHelpText);
			button.Clicked += (sender, e) =>
			{
				activityIndicator.IsRunning = !activityIndicator.IsRunning;
				activityIndicator.SetAutomationPropertiesHelpText(activityIndicator.IsRunning ? "Running." : "Not running");
			};


			var instructions4 = new Label { Text = imageInstructions };
			var image = new Image { Source = "photo.jpg" };
			// The tap gesture will NOT work on Win
			image.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => DisplayAlert("Success", "You tapped the image", "OK")) });
			image.SetAutomationPropertiesName(ImageName);
			image.SetAutomationPropertiesHelpText(ImageHelpText);

			var instructions6 = new Label { Text = boxInstructions };
			var boxView = new BoxView { Color = Color.Purple };
			// The tap gesture will NOT work on Win
			boxView.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => DisplayAlert("Success", "You tapped the box", "OK")) });
			boxView.SetAutomationPropertiesName(BoxName);
			boxView.SetAutomationPropertiesHelpText(BoxHelpText);
			// BoxViews are ignored by default on Win; 
			// make accessible in order to enable the gesture and narration
			boxView.SetAutomationPropertiesIsInAccessibleTree(true);


			var stack = new StackLayout
			{
				Children =
				{
					instructions,
					toolbarInstructionsLbl,
					instructions2,
					entryGroup,
					tbl,
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

	public static class AutomationPropertiesExtensions
	{
		public static void SetAutomationPropertiesName(this Element element, string name)
		{
			element.SetValue(AutomationProperties.NameProperty, name);
		}

		public static string GetAutomationPropertiesName(this Element element)
		{
			return (string)element.GetValue(AutomationProperties.NameProperty);
		}

		public static void SetAutomationPropertiesHelpText(this Element element, string HelpText)
		{
			element.SetValue(AutomationProperties.HelpTextProperty, HelpText);
		}

		public static string GetAutomationPropertiesHelpText(this Element element)
		{
			return (string)element.GetValue(AutomationProperties.HelpTextProperty);
		}

		public static void SetAutomationPropertiesIsInAccessibleTree(this Element element, bool value)
		{
			element.SetValue(AutomationProperties.IsInAccessibleTreeProperty, value);
		}

		public static bool GetAutomationPropertiesIsInAccessibleTree(this Element element)
		{
			return (bool)element.GetValue(AutomationProperties.IsInAccessibleTreeProperty);
		}

		public static void SetAutomationPropertiesLabeledBy(this Element element, Element value)
		{
			element.SetValue(AutomationProperties.LabeledByProperty, value);
		}

		public static Element GetAutomationPropertiesLabeledBy(this Element element)
		{
			return (Element)element.GetValue(AutomationProperties.LabeledByProperty);
		}
	}
}
