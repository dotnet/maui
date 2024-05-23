using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1908, "Image reuse", PlatformAffected.Android)]
	public class Issue1908 : TestContentPage
	{

		public Issue1908()
		{

		}

		protected override void Init()
		{
			StackLayout listView = new StackLayout();

			for (int i = 0; i < 1000; i++)
			{
				listView.Children.Add(new Image() { Source = "oasis.jpg", ClassId = $"OASIS{i}", AutomationId = $"OASIS{i}" });
			}

			Content = new ScrollView() { Content = listView };
		}
	}
}
