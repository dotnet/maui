using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 3667, "[Enhancement] Add text-transforms to Label", PlatformAffected.All)]
	public class Issue3667 : TestContentPage
	{
		string text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua.";

		protected override void Init()
		{
			var text2 = "Malesuada fames ac turpis egestas maecenas pharetra convallis. Dictum varius duis at consectetur lorem donec massa. Augue interdum velit euismod in pellentesque.";
			var transform = TextTransform.None;

			var list = new ITextElement[] {
				new Label { Text = text },
				new Entry { Text = "Entry text" },
				new Editor { Text = "Editor text" },
				new SearchBar { Text = "SearchBar text" },
				new Button { Text = "Button text" },
			};

			var statusLabel = new Label
			{
				Text = "Current TextTransform is None",
				BackgroundColor = Color.Aqua,
				TextTransform = transform
			};
			var but = new Button
			{
				Text = "Change TextTransform",
				Command = new Command(() =>
				{
					if (++transform > TextTransform.Uppercase)
						transform = TextTransform.None;
					foreach (var item in list)
						item.TextTransform = transform;
					statusLabel.Text = $"Current TextTransform is {transform}";
				})
			};

			var layout = new StackLayout
			{
				Children = {
					statusLabel,
					but
				}
			};

			foreach (var item in list)
			{
				item.TextTransform = transform;
				layout.Children.Add(item as View);
			}

			layout.Children.Add(new Label
			{
				Text = "[Lowercase] " + text2,
				TextTransform = TextTransform.Lowercase
			});
			layout.Children.Add(new Label
			{
				Text = "[Uppercase] " + text2,
				TextTransform = TextTransform.Uppercase
			});

			Content = layout;
		}

#if UITEST
		[Test]
		public void Issue3667Tests ()
		{
			RunningApp.WaitForElement(query => query.Text(text));
			RunningApp.Tap("Change TextTransform");
			RunningApp.WaitForElement(query => query.Text(text));
			RunningApp.Tap("Change TextTransform");
			RunningApp.WaitForElement(query => query.Text(text.ToLowerInvariant()));
			RunningApp.Tap("Change TextTransform");
			RunningApp.WaitForElement(query => query.Text(text.ToUpperInvariant()));
			RunningApp.Tap("Change TextTransform");
			RunningApp.WaitForElement(query => query.Text(text));
		}
#endif
	}
}


