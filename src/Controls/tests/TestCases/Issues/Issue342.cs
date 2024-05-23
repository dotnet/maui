using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 342, "NRE when Image is not assigned source", PlatformAffected.WinPhone)]
	public class Issue342NoSource : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 342";
			Content = new StackLayout
			{
				Children = {
					new Label {
						Text = "Uninitialized image"
					},
					new Image ()
				}
			};
		}

		// Should not throw exception when user does not include image
	}

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 342, "NRE when Image is delayed source", PlatformAffected.WinPhone)]
	public class Issue342DelayedSource : TestContentPage
	{
		protected override void Init()
		{
			Title = "Issue 342";

			_image = new Image();

			Content = new StackLayout
			{
				AutomationId = "TestReady",
				Children = {
					new Label {
						Text = "Delayed image"
					},
					_image
				}
			};

			AddSourceAfterDelay();
		}

		// Should not throw exception when user does not include image
		Image _image;

		void AddSourceAfterDelay()
		{
			Device.StartTimer(TimeSpan.FromSeconds(2), () =>
			{
				_image.Source = "cover1.jpg";
				return false;
			});
		}
	}
}
