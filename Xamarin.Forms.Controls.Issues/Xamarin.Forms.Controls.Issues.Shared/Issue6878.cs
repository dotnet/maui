using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 6878,
		"ShellItem.Items.Clear() crashes when the ShellItem has bottom tabs", PlatformAffected.All)]

#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue6878 : TestShell
	{
		const string ClearShellItems = "ClearShellItems";
		const string StatusLabel = "StatusLabel";
		const string StatusLabelText = "Everything is fine 😎";
		const string TopTab = "Top Tab";
		const string PostClearTopTab = "Post clear Top Tab";

		StackLayout _stackContent;

		protected override void Init()
		{
			_stackContent = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						AutomationId = StatusLabel,
						Text = StatusLabelText
					}
				}
			};

			_stackContent.Children.Add(BuildClearButton());
			AddTopTab(TopTab).Content = _stackContent;

			CurrentItem = Items.Last();

			AddTopTab(TopTab);
			AddBottomTab("Bottom tab");
			Shell.SetBackgroundColor(this, Color.BlueViolet);
		}

		Button BuildClearButton()
		{
			return new Button()
			{
				Text = "Click to clear ShellItem.Items",
				Command = new Command(() =>
				{
					Items[0].Items.Clear();
					Items.Clear();
					AddTopTab(TopTab).Content = _stackContent;
					CurrentItem = Items.Last();

					AddTopTab(PostClearTopTab);
				}),
				AutomationId = ClearShellItems
			};
		}

#if UITEST
		[Test]
		public void ShellItemItemsClearTests()
		{
			RunningApp.WaitForElement(StatusLabel);
			RunningApp.Tap(ClearShellItems);

			var label = RunningApp.WaitForElement(StatusLabel)[0];
			Assert.AreEqual(label.Text, StatusLabelText);
			RunningApp.Tap(PostClearTopTab);
		}
#endif
	}
}
