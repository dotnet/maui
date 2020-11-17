using System;
using System.Collections.Generic;
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
	[Category(UITestCategories.Shell)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6184, "Throws exception when set isEnabled to false in ShellItem index > 5", PlatformAffected.iOS)]
	public partial class Issue6184 : TestShell
	{
		public Issue6184()
		{
#if APP

			InitializeComponent();
#endif
		}

		protected override void Init()
		{
		}

#if UITEST && __IOS__
		[Test]
		public void GitHubIssue6184()
		{
			RunningApp.WaitForElement(q => q.Marked("More"));
			RunningApp.Tap(q => q.Marked("More"));
			RunningApp.Tap(q => q.Marked("Issue 5"));
			RunningApp.WaitForElement(q => q.Marked("Issue 5"));
		}
#endif
	}

	[Preserve(AllMembers = true)]
	public class PageInstruction : ContentPage
	{

		Label pageNumber;
		public int PageNumber
		{
			set
			{
				pageNumber.Text = $"Page Number: {value}";
			}
		}

		public PageInstruction()
		{
			pageNumber = new Label();
			var stack = new StackLayout();
			var label = new Label
			{
				Text = "Press the more page, and see if the Cells with Title \"Issue 5\", \"Issue 9\", \"Issue 18\" are Disabled. If don't the test fails",
				FontSize = 20
			};
			stack.Children.Add(label);
			stack.Children.Add(pageNumber);
			Content = stack;
		}
	}
}

