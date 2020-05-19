using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using System;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40161, "Issue Description", PlatformAffected.Default)]
	public class Bugzilla40161 : TestContentPage
	{
		// If an image is swapped out for another image must the size of an image be recomputed? 
		// That would work but is slow. As an optimization, if an image's size is controlled by 
		// it's parents and the parents dictate that any image will be a specific size then there
		// is no need to layout the image. Consider the following scenarios:

		// (a) An aboslute layout dictates the size of a child image when the child specifies that it
		// should fill the space allocated by the aboslute layout. In this case the optimization 
		// should be enabled; the layout pass for the replaced image can be skipped. The replaced image
		// should occupy the same space as the orig image.

		// (b) The image size is *not* dicatated by the absolute layout if it chooses not to fill the 
		// space the absolute layout  allocates it and instead chooses to simply be centered with in 
		// that space. In this case the layout pass for the replaced image must be run to compute the 
		// size of the replaced image. This was the case reported by the bug that led to this UITest.
		protected override void Init()
		{
			var absolute = new AbsoluteLayout()
			{
				// The size of an AbsoluteLayout whose H/V options equal Fill will match its
				// parent container. Given that, the layout engine will optimize any re-layout of 
				// such an AbsoluteLayout by not recomputing its size if its parent container 
				// does not change size. Such an AbsoluteLayout is marked as special by setting
				// its AbsoluteLayout.LayoutConstraint to fixed. All it's children will inherit
				// the special setting.
				HorizontalOptions = LayoutOptions.Fill,
				VerticalOptions = LayoutOptions.Fill,
			};

			var imageA = "seth.png";
			var imageB = "test.jpg";

			var image = new Image()
			{
				Source = imageA,
				Aspect = Aspect.AspectFill,

				// Children of an AbsoluteLayout can potentially inherit its LayoutConstraint.
				// This should happen if the child H/V options are also set to Fill AND its size
				// is all proportional. In that case the child fills the size allocated to it by 
				// the layout and so it's size should be re-computed iff the layout's size has
				// changed. This behavior is achived by inheriting the layout's LayoutConstraint.

				// *IF* however the H/V options are Center then the Image should be rendered to 
				// to *AT MOST* the image size regardless of whether the region allocated by
				// the absolute layout is larger than the image. 
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
			};

			AbsoluteLayout.SetLayoutFlags(image, AbsoluteLayoutFlags.All);
			AbsoluteLayout.SetLayoutBounds(image, new Rectangle(0, 0, 1, 1));
			absolute.Children.Add(image);

			var stack = new StackLayout();
			stack.Children.Add(absolute);

			bool flipSwap = false;
			var swap = new Button() { Text = "SWAP" };
			swap.Clicked += (object sender, EventArgs e) => 
			{
				if (flipSwap)
					image.Source = imageA;
				else
					image.Source = imageB;

				flipSwap = !flipSwap;
			};
			stack.Children.Add(swap);

			bool flipLayout = false;
			var layout = new Button() { Text = "LAYOUT" };
			layout.Clicked += (object sender, EventArgs e) => 
			{
				if (flipLayout)
				{
					image.HorizontalOptions = LayoutOptions.Center;
					image.VerticalOptions = LayoutOptions.Center;
				}
				else
				{
					image.HorizontalOptions = LayoutOptions.Fill;
					image.VerticalOptions = LayoutOptions.Fill;
				}

				flipLayout = !flipLayout;
			};
			stack.Children.Add(layout);

			var counter = new Label() { Text = "counter", AutomationId="counter" };
			var height = new Label() { Text = "height", AutomationId="height" };
			var width = new Label() { Text = "width", AutomationId= "width" };
			stack.Children.Add(counter);
			stack.Children.Add(height);
			stack.Children.Add(width);

			var count = 0;
			var refresh = new Button() { Text = "REFRESH" };
			refresh.Clicked += (object sender, EventArgs e) =>
			{
				height.Text = $"h={Math.Round(image.Height)}";
				width.Text = $"w={Math.Round(image.Width)}";
				counter.Text = $"step={count++}";
			};
			stack.Children.Add(refresh);

			Content = stack;
		}

#if UITEST
		[Test]
		public void Issue1Test ()
		{
			RunningApp.Screenshot ("I am at Issue 40161");
			RunningApp.WaitForElement (q => q.Marked ("REFRESH"));
			RunningApp.Screenshot ("I see the first image");

			RunningApp.Tap ("SWAP");
			RunningApp.Tap ("REFRESH");
			RunningApp.WaitForElement(q => q.Marked("step=0"));
			RunningApp.Screenshot ("I swap the image");
			RunningApp.WaitForElement(q => q.Marked("w=50"));
		}
#endif
	}
}