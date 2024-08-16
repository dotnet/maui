using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4782, "[Android] Null drawable crashes Image Button",
		PlatformAffected.iOS)]
	public class Issue4782 : TestContentPage
	{
		const string _success = "Success";
		public class Issue4782ImageButton : ImageButton { }

		protected override void Init()
		{
			Content = new StackLayout()
			{
				Children =
				{
					new Label()
					{
						Text = "If app didn't crash then test passed",
						AutomationId = _success
					},
					new Issue4782ImageButton()
				}
			};
		}
	}
}