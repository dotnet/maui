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
	[Issue(IssueTracker.Github, 5461, "[Android] ScrollView crashes when setting ScrollbarFadingEnabled to false in Custom Renderer",
		PlatformAffected.Android)]
	public class Issue5461 : TestContentPage
	{
		const string Success = "If you can see this, the test has passed";
		protected override void Init()
		{
			ScrollView scrollView = new ScrollbarFadingEnabledFalseScrollView()
			{
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = Success
						}
					},
					HeightRequest = 2000
				}
			};

			Content = scrollView;
		}

		public class ScrollbarFadingEnabledFalseScrollView : ScrollView { }
	}
}
