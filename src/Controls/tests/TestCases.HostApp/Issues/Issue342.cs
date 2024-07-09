using System;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.ManualTest, "342", "NRE when Image is not assigned source", PlatformAffected.WinPhone)]
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
	[Issue(IssueTracker.ManualTest, "342 Delayed", "NRE when Image is delayed source", PlatformAffected.WinPhone)]
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
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			Device.StartTimer(TimeSpan.FromSeconds(2), () =>
			{
				_image.Source = "cover1.jpg";
				return false;
			});
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete
		}
	}
}
