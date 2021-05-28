using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
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
			Shell.SetBackgroundColor(this, Colors.BlueViolet);
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
			Assert.AreEqual(StatusLabelText, label.ReadText());
			RunningApp.Tap(PostClearTopTab);
		}
#endif
	}
}
