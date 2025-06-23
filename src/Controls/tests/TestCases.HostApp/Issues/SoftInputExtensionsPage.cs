#nullable enable
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 11565, "Soft Input Extension Methods", PlatformAffected.All)]
	public class SoftInputExtensionsPage : TestContentPage
	{
		private Entry? basicEntry;

		public SoftInputExtensionsPage()
		{
		}

		protected override void Init()
		{
			Title = "Soft Input Extension Methods";
			this.basicEntry = new Entry() { AutomationId = "BasicEntry", Placeholder = "Basic Entry" };
			var resultLabel = new Label() { AutomationId = "Result" };
			Content = new VerticalStackLayout()
			{
				new Button()
				{
					Text = "Show Keyboard",
					AutomationId = "ShowKeyboard",
					Command = new Command(() => {
						this.basicEntry.ShowSoftInputAsync(CancellationToken.None);
						SetResultLabel(resultLabel, basicEntry);
					})
				},
				new Button()
				{
					Text = "Hide Keyboard",
					AutomationId = "HideKeyboard",
					Command = new Command(() => {
						this.basicEntry.HideSoftInputAsync(CancellationToken.None);
						SetResultLabel(resultLabel, basicEntry);
					})
				},
				this.basicEntry,
				resultLabel
			};
		}

		private async void SetResultLabel(Label resultLabel, ITextInput input)
		{
			await Task.Delay(1000).ContinueWith(t =>
			{
				this.Dispatcher.Dispatch(() =>
				{

					var isSoftInputShowing = input.IsSoftInputShowing();

#if WINDOWS || MACCATALYST
					// Windows has an optional soft keyboard, which may or may not be enabled in CI.
					// Catalyst does not have a soft keyboard.
					// But the SoftInput extensions still select or deselect the entry.
					isSoftInputShowing = ((View)input).IsFocused;
#endif

					resultLabel.Text = $"{isSoftInputShowing}";
				});
			});
		}
	}
}
