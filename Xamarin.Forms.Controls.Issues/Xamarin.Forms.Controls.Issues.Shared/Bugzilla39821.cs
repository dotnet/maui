using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39821, "ViewExtension.TranslateTo cannot be invoked on Main thread")]
	public class Bugzilla39821 : TestContentPage 
	{
		protected override void Init()
		{
			var box = new BoxView { BackgroundColor = Color.Blue, WidthRequest = 50, HeightRequest = 50, HorizontalOptions = LayoutOptions.Center };

			var instructions = new Label { Text = "Click the 'Animate' button to run animation on the box. If the animations complete without crashing, this test has passed." };

			var success = new Label { Text = "Success", IsVisible = false };

			var button = new Button() { Text = "Animate" };

			Content = new StackLayout
			{
				VerticalOptions = LayoutOptions.Fill,
				HorizontalOptions = LayoutOptions.Fill,
				Children =
				{
					instructions,
					success,
					button,
					new AbsoluteLayout
					{
						Children = { box },
						HorizontalOptions = LayoutOptions.Fill,
						VerticalOptions = LayoutOptions.Fill
					}
				}
			};

			button.Clicked += async (sender, args) => {
				// Run a bunch of animations from the thread pool 
				await Task.WhenAll(
					Task.Run(async () => await Translate(box)),
					Task.Run(async () => await CheckTranslateRunning(box)),
					Task.Run(async () => await AnimateScale(box)),
					Task.Run(async () => await Rotate(box)),
					Task.Run(async () => await Animate(box)),
					Task.Run(async () => await Kinetic(box)),
					Task.Run(async () => await Cancel(box))
					);

				success.IsVisible = true;
			};
		}

#pragma warning disable 1998 // considered for removal
		async Task CheckTranslateRunning(BoxView box)
#pragma warning restore 1998
		{
			Debug.WriteLine(box.AnimationIsRunning("TranslateTo") ? "Translate is running" : "Translate is not running");
		}

		static async Task Translate(BoxView box)
		{
			var currentX = box.X;
			var currentY = box.Y;

			await box.TranslateTo(currentX, currentY + 100);
			await box.TranslateTo(currentX, currentY);
		}

		static async Task AnimateScale(BoxView box)
		{
			await box.ScaleTo(2);
			await box.ScaleTo(0.5);
		}

		static async Task Rotate(BoxView box)
		{
			await box.RelRotateTo(360);
		}

#pragma warning disable 1998 // considered for removal
		async Task Cancel(BoxView box)
#pragma warning restore 1998
		{
			box.AbortAnimation("animate");
			box.AbortAnimation("kinetic");
		}

#pragma warning disable 1998 // considered for removal
		async Task Animate(BoxView box)
#pragma warning restore 1998
		{
			box.Animate("animate", d => d, d => { }, 100, 1);
		}

#pragma warning disable 1998 // considered for removal
		async Task Kinetic(BoxView box)
#pragma warning restore 1998
		{
			var resultList = new List<Tuple<double, double>>();

			box.AnimateKinetic("kinetic", (distance, velocity) =>
			{
				resultList.Add(new Tuple<double, double>(distance, velocity));
				return true;
			}, 100, 1);
		}

#if UITEST
		[Test]
		public void DoesNotCrash()
		{
			RunningApp.Tap(q => q.Marked("Animate"));
			RunningApp.WaitForElement(q => q.Marked("Success"));
		}
#endif
	}
}