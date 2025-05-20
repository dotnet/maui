namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24264, "RadioButton should not leak memory", PlatformAffected.iOS)]
public class Issue24264 : NavigationPage
{
	public Issue24264() : base(new TestPage())
	{
	}

	public class TestPage : TestContentPage
	{
		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "Click 'Push Page' twice to test RadioButton cleanup",
				FontSize = 16
			};

			var result = new Label
			{
				Text = "Success",
				IsVisible = false,
				AutomationId = "successLabel"
			};

			var checkButton = new Button
			{
				Text = "Check Result",
				AutomationId = "resultButton",
				IsVisible = false
			};

			var refs = new List<WeakReference>();

			checkButton.Command = new Command(async () =>
			{
				if (refs.Count < 2)
				{
					instructions.Text = "Push Page again";
					return;
				}

				try
				{
					await GarbageCollectionHelper.WaitForGC(2000, refs.ToArray());
					result.IsVisible = true;
					instructions.Text = "Test passed";
				}
				catch
				{
					instructions.Text = "Memory Leak Detected";
					result.IsVisible = false;
				}
			});

			var pushButton = new Button
			{
				Text = "Push Page",
				AutomationId = "pushbutton",
				Command = new Command(async () =>
				{
					if (refs.Count >= 2)
					{
						refs.Clear();
					}

					var page = new RadioButtonPage();
					var weakReference = new WeakReference(page);
					await Navigation.PushAsync(page);
					await (weakReference.Target as Page).Navigation.PopAsync();
					refs.Add(weakReference);

					if (refs.Count >= 2)
					{
						checkButton.IsVisible = true;
						instructions.Text = "You can now check for memory leaks";
					}
					else
					{
						instructions.Text = "Push Page again";
					}
				})
			};

			Content = new StackLayout
			{
				Children =
				{
					instructions,
					pushButton,
					checkButton,
					result
				}
			};
		}

		class RadioButtonPage : ContentPage
		{
			public RadioButtonPage()
			{
				var layout = new VerticalStackLayout();
				layout.SetValue(RadioButtonGroup.GroupNameProperty, "testGroup");
				layout.SetValue(RadioButtonGroup.SelectedValueProperty, "dog");
				layout.Children.Add(new RadioButton { Content = "Cat", Value = "cat" });
				layout.Children.Add(new RadioButton { Content = "Dog", Value = "dog" });
				layout.Children.Add(new RadioButton { Content = "Fish", Value = "fish" });
				Content = layout;
			}

			~RadioButtonPage()
			{
				System.Diagnostics.Debug.WriteLine("~RadioButtonPage finalized");
			}
		}
	}
}