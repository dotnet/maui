namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1683, "Auto Capitalization Implementation")]
	public class Issue1683 : TestContentPage
	{
		const string kContainerId = "Container";

		protected override void Init()
		{
			var layout = new StackLayout() { ClassId = kContainerId };

			KeyboardFlags[] flags = new[]
			{
				KeyboardFlags.None,
				KeyboardFlags.CapitalizeWord,
				KeyboardFlags.CapitalizeSentence,
				KeyboardFlags.CapitalizeCharacter,
				KeyboardFlags.CapitalizeNone,
				KeyboardFlags.All
			};

			List<Entry> entryViews = new List<Entry>();
			List<Editor> editorViews = new List<Editor>();
			List<InputView> inputViews = new List<InputView>();

			KeyboardFlags spellCheckForUwp = KeyboardFlags.None;

			if (DeviceInfo.Platform == DevicePlatform.WinUI)
			{
				spellCheckForUwp = KeyboardFlags.Spellcheck;
			}

			foreach (var flag in flags)
			{
				entryViews.Add(new EntryKeyboardFlags() { FlagsToTestFor = flag, Keyboard = Keyboard.Create(flag | spellCheckForUwp), ClassId = $"Entry{flag}" });
				editorViews.Add(new EditorKeyboardFlags() { Keyboard = Keyboard.Create(flag | spellCheckForUwp), ClassId = $"Editor{flag}" });
			}

			entryViews.Add(new EntryKeyboardFlags() { ClassId = "EntryNoKeyboard" });
			editorViews.Add(new EditorKeyboardFlags() { ClassId = "EditorNoKeyboard" });

			inputViews.AddRange(entryViews);
			inputViews.AddRange(editorViews);

			inputViews.Add(new EntryKeyboardFlags() { ClassId = "CustomRenderer" });
			inputViews.Add(new EntryKeyboardFlags() { ClassId = "CustomRendererCapitalizeSentence", FlagsToSet = KeyboardFlags.CapitalizeSentence, FlagsToTestFor = KeyboardFlags.CapitalizeSentence });
			inputViews.Add(new EntryKeyboardFlags() { ClassId = "CustomRendererCapitalizeWord", FlagsToSet = KeyboardFlags.CapitalizeWord, FlagsToTestFor = KeyboardFlags.CapitalizeWord });

#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			if (Device.RuntimePlatform != Device.WinUI)
			{
				inputViews.Add(new EntryKeyboardFlags() { ClassId = "CustomRendererCapitalizeCharacter", FlagsToSet = KeyboardFlags.CapitalizeCharacter });
			}
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete

			if (DeviceInfo.Platform == DevicePlatform.WinUI)
			{
				layout.Children.Add(new Label() { Text = "Capitalization settings only work when using touch keyboard" });
				layout.Children.Add(new Label() { Text = "Character doesn't do anything on UWP" });
			}
			else if (DeviceInfo.Platform == DevicePlatform.iOS)
			{
				layout.Children.Add(new Label() { Text = "All will use Sentence" });
				layout.Children.Add(new Label() { Text = "No Keyboard will use Sentence" });
			}
			else if (DeviceInfo.Platform == DevicePlatform.Android)
			{
				layout.Children.Add(new Label() { Text = "All will use Sentence" });
				layout.Children.Add(new Label() { Text = "No Keyboard will use None" });
			}

			foreach (InputView child in inputViews)
			{
				var inputs = new StackLayout()
				{
					Orientation = StackOrientation.Horizontal
				};

				if (child is Entry)
					(child as Entry).Text = "All the Same.";

				if (child is Editor)
					(child as Editor).Text = "All the Same.";


#pragma warning disable CS0618 // Type or member is obsolete
				child.HorizontalOptions = LayoutOptions.FillAndExpand;
#pragma warning restore CS0618 // Type or member is obsolete
				var theLabel = new Label();

				theLabel.SetBinding(Label.TextProperty, new Binding("ClassId", source: child));
				inputs.Children.Add(theLabel);
				inputs.Children.Add(child);

				layout.Children.Add(inputs);
			}



			Button rotate = new Button()
			{
				Text = "Change Capitalization Settings. Ensure they update correctly",
				AutomationId = "Rotation"
			};


			// This shifts everyones capitalization by one in order
			// to test that updating the field works as expected
			rotate.Clicked += (_, __) =>
			{
				var item1 = entryViews[0];
				entryViews.Remove(item1);
				entryViews.Add(item1);

				var item2 = editorViews[0];
				editorViews.Remove(item2);
				editorViews.Add(item2);

				for (int i = 0; i <= flags.Length; i++)
				{
					var editorView = editorViews[i] as EditorKeyboardFlags;
					var entryView = entryViews[i] as EntryKeyboardFlags;

					if (i == flags.Length)
					{
						entryView.FlagsToTestFor = null;
						entryView.Keyboard = null;
						entryView.ClassId = "EntryNoKeyboard";


						editorView.FlagsToTestFor = null;
						editorView.ClassId = "EntryNoKeyboard";
						editorView.Keyboard = null;
					}
					else
					{
						entryView.FlagsToTestFor = flags[i];
						entryView.Keyboard = Keyboard.Create(flags[i] | spellCheckForUwp);
						entryView.ClassId = $"Entry{flags[i]}";

						editorView.FlagsToTestFor = flags[i];
						editorView.Keyboard = Keyboard.Create(flags[i] | spellCheckForUwp);
						editorView.ClassId = $"Editor{flags[i]}";
					}
				}
			};

			StackLayout content = new StackLayout();
			content.Children.Add(new ScrollView()
			{
				//Set the ScrollView height to adjust its layout to ensure the Rotate button remains visible on Android.
				HeightRequest = 600,
				Content = layout
			});

			content.Children.Add(rotate);

			Content = content;
		}



		public class EditorKeyboardFlags : Editor
		{
			public KeyboardFlags? FlagsToSet { get; set; }


			public KeyboardFlags? FlagsToTestFor { get; set; }
		}


		public class EntryKeyboardFlags : Entry
		{
			public KeyboardFlags? FlagsToSet { get; set; }


			public KeyboardFlags? FlagsToTestFor { get; set; }
		}
	}
}