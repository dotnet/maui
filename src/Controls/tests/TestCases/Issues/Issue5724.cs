using System.Threading;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 5724, "Next Moves To Next Entry and Done Closes Input View", PlatformAffected.Android)]
	public class Issue5724 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new VerticalStackLayout();

			var entry1 = new Entry
			{
				Text = "Entry 1",
				ReturnType = ReturnType.Next,
				AutomationId = "Entry1"
			};

			entry1.Focused += async (_, _) =>
			{
				// Make sure keyboard opens
				await entry1.ShowSoftInputAsync(CancellationToken.None);
			};

			var entry2 = new Entry
			{
				Text = "Entry 2",
				ReturnType = ReturnType.Next,
				AutomationId = "Entry2"
			};

			layout.Add(entry1);
			layout.Add(entry2);

			var entry3 = new Entry()
			{
				Text = "Entry Done",
				ReturnType = ReturnType.Done,
				AutomationId = "EntryDone"
			};

			entry3.Focused += async (_, _) =>
			{
				// Make sure keyboard opens
				await entry3.ShowSoftInputAsync(CancellationToken.None);
			};

			layout.Add(entry3);

			layout.Add(new Entry()
			{
				Text = "Entry Done",
				ReturnType = ReturnType.Done,
				AutomationId = "EntryDone2"
			});

			layout.Add(new Button()
			{
				Text = "Send Next Button",
				AutomationId = "SendNext",
				Command = new Command(() =>
				{
#if ANDROID
					Handler.MauiContext.Context
						.GetActivity()
						.CurrentFocus
						?.OnCreateInputConnection(new Android.Views.InputMethods.EditorInfo())
						?.PerformEditorAction(Android.Views.InputMethods.ImeAction.Next);
#endif
				})
			});

			layout.Add(new Button()
			{
				Text = "Send Done Button",
				AutomationId = "SendDone",
				Command = new Command(() =>
				{
#if ANDROID
					Handler.MauiContext.Context
						.GetActivity()
						.CurrentFocus
						?.OnCreateInputConnection(new Android.Views.InputMethods.EditorInfo())
						?.PerformEditorAction(Android.Views.InputMethods.ImeAction.Done);
#endif
				})
			});

			Content = layout;
		}
	}
}
