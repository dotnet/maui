using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7181, "[Bug] Cannot update ToolbarItem text and icon", PlatformAffected.Android)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue7181 : TestShell
	{
		const string ToolbarBtn = "Toolbar button";
		const string DefaultToolbarItemText = "Toolbar test";
		const string AfterClickToolbarItemText = "Button Clicked";
		const string SetToolbarIconBtn = "Set toolbar icon button";

		int _clicks = 0;
		ToolbarItem _toolbarItem;

		protected override void Init()
		{
			var page = CreateContentPage("Test page");

			_toolbarItem = new ToolbarItem(DefaultToolbarItemText, string.Empty, OnToolbarClicked)
			{
				AutomationId = ToolbarBtn
			};

			page.ToolbarItems.Add(_toolbarItem);
			page.Content = new StackLayout()
			{
				Children =
				{
					new Label
					{
						Text = "You should be able to change toolbar text"
					},
					new Button
					{
						AutomationId = SetToolbarIconBtn,
						Text = "Click to change toolbarIcon",
						Command = new Command(()=> _toolbarItem.IconImageSource = "coffee.png" )
					}
				}
			};
		}

		private void OnToolbarClicked() =>
			_toolbarItem.Text = $"{AfterClickToolbarItemText} {_clicks++}";

#if UITEST && __ANDROID__
		[Test]
		public void ShellToolbarItemTests()
		{
			var count = 0;
			var toolbarButton = RunningApp.WaitForElement(ToolbarBtn);
			Assert.AreEqual(toolbarButton[0].Text, DefaultToolbarItemText);

			for (int i = 0; i < 5; i++)
			{
				RunningApp.Tap(ToolbarBtn);

				toolbarButton = RunningApp.WaitForElement(ToolbarBtn);
				Assert.AreEqual($"{AfterClickToolbarItemText} {count++}", toolbarButton[0].Text);
			}

			RunningApp.Tap(SetToolbarIconBtn);
		}
#endif
	}
}
