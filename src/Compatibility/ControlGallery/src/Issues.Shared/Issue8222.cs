using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 8222, "UWP Text in Editor is now vertically centered.", PlatformAffected.UWP)]
	public class Issue8222 : TestContentPage
	{
		Editor theEditor;

		public Issue8222()
		{
			Title = "Issue 8222";
		}

		protected override void Init()
		{
			Label instructions = new Label
			{
				Text = "Check the Editor control. Text should be aligned with the top."
			};

			theEditor = new Editor
			{
				HeightRequest = 100,
			};

			var stack = new StackLayout();

			stack.Children.Add(instructions);
			stack.Children.Add(theEditor);

			Content = stack;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			theEditor.Text = "This text should be top-aligned in the Editor control";
		}
	}
}

