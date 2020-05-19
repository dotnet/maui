using System;
using System.Collections.Generic;
using System.Linq;
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
	[Issue(IssueTracker.Github, 1601, "Exception thrown when `Removing Content Using LayoutCompression", PlatformAffected.Android)]
	public class Issue1601 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid();
			Forms.CompressedLayout.SetIsHeadless(grid, true);
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			grid.RowDefinitions.Add(new RowDefinition { Height = 40 });

			var boxView = new BoxView { BackgroundColor = Color.Red };
			var backgroundContainer = new Grid();
			Forms.CompressedLayout.SetIsHeadless(backgroundContainer, true);
			backgroundContainer.Children.Add(boxView);
			grid.Children.Add(backgroundContainer);

			var stack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};
			Forms.CompressedLayout.SetIsHeadless(stack, true);
			var button = new Button { Text = "CRASH!" };
			stack.Children.Add(button);
			grid.Children.Add(stack, 0, 1);
			
			button.Clicked += (s, e) => Content = null;

			Content = grid;
		}

#if UITEST
		[Test]
		public void Issue1601Test()
		{
			RunningApp.Screenshot("Start G1601");
			RunningApp.WaitForElement(q => q.Marked("CRASH!"));
			RunningApp.Screenshot("I see the button");
			RunningApp.Tap (q => q.Marked ("CRASH!"));
			RunningApp.Screenshot("Didn't crash!");
		}
#endif
	}
}
