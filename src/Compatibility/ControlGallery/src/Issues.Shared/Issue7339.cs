using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7339, "[iOS] Material frame renderer not being cleared",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue7339 : TestShell
	{
		protected override void Init()
		{
			Visual = VisualMarker.Material;
			CreateContentPage("Item1").Content =
				new StackLayout()
				{
					Children =
					{
						new Frame()
						{
							Content = new Label()
							{
								Text = "Navigate between flyout items a few times. If app doesn't crash then test has passed"
							}
						}
					}
				};

			CreateContentPage("Item2").Content =
				new StackLayout() { Children = { new Frame() } };
		}

#if UITEST
		[Test]
		public void MaterialFrameDisposesCorrectly()
		{
			TapInFlyout("Item1");
			TapInFlyout("Item2");
			TapInFlyout("Item1");
			TapInFlyout("Item2");
		}
#endif
	}
}
