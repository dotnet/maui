using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;


#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "Shell Flyout Width and Height",
		PlatformAffected.All)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class ShellFlyoutSizing : TestShell
	{
		protected override void Init()
		{
			AddContentPage(new ContentPage()
			{
				Title = "Main Page",
				Content = new StackLayout()
				{
					Children =
					{
						new Label()
						{
							Text = "Open the Flyout and click the button. The height and width should change. Click it again and it should go back to default",
							AutomationId ="PageLoaded"
						}
					}
				}
			});

			FlyoutBackground = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Color.Blue, Offset = 0.1f },
					new GradientStop { Color = Color.BlueViolet, Offset = 1.0f },
				}
			};

			FlyoutHeader = new Label()
			{
				BackgroundColor = Color.LightBlue,
				Text = "Header",
				AutomationId = "FlyoutHeader"
			};


			FlyoutFooter = new Label()
			{
				BackgroundColor = Color.LightBlue,
				Text = "Footer",
				AutomationId = "FlyoutFooter"
			};


			var scale = 10d;

			if (Device.RuntimePlatform == Device.Android)
				scale = scale / Device.info.ScalingFactor;

			var increaseMenuItem = new MenuItem()
			{
				Text = "Increase Height and Width",
				Command = new Command(() =>
				{
					FlyoutWidth += scale;
					FlyoutHeight += scale;
				}),
				AutomationId = "IncreaseFlyoutSizes"
			};

			var descreaseMenuItem = new MenuItem()
			{
				Text = "Decrease Height and Width",
				Command = new Command(() =>
				{
					FlyoutWidth -= scale;
					FlyoutHeight -= scale;
				}),
				AutomationId = "DecreaseFlyoutSizes"
			};


			Items.Add(new MenuItem()
			{
				Text = "Change Height and Width",
				Command = new Command(() =>
				{
					FlyoutWidth = 350;
					FlyoutHeight = 350;
					Items.Add(increaseMenuItem);
					Items.Add(descreaseMenuItem);
				}),
				AutomationId = "ChangeFlyoutSizes"
			});

			Items.Add(new MenuItem()
			{
				Text = "Reset Height and Width",
				Command = new Command(() =>
				{
					FlyoutWidth = -1;
					FlyoutHeight = -1;
					Items.Remove(increaseMenuItem);
					Items.Remove(descreaseMenuItem);
				}),
				AutomationId = "ResetFlyoutSizes"
			});
		}


#if UITEST
		[Test]
		public void FlyoutHeightAndWidthResetsBackToOriginalSize()
		{
			RunningApp.WaitForElement("PageLoaded");
			this.ShowFlyout();
			var initialWidth = RunningApp.WaitForElement("FlyoutHeader")[0].Rect.Width;
			var initialHeight = RunningApp.WaitForElement("FlyoutFooter")[0].Rect.Y;
			TapInFlyout("ChangeFlyoutSizes", makeSureFlyoutStaysOpen: true);
			Assert.AreNotEqual(initialWidth, RunningApp.WaitForElement("FlyoutHeader")[0].Rect.Width);
			Assert.AreNotEqual(initialHeight, RunningApp.WaitForElement("FlyoutFooter")[0].Rect.Y);
			TapInFlyout("ResetFlyoutSizes", makeSureFlyoutStaysOpen: true);
			Assert.AreEqual(initialWidth, RunningApp.WaitForElement("FlyoutHeader")[0].Rect.Width);
			Assert.AreEqual(initialHeight, RunningApp.WaitForElement("FlyoutFooter")[0].Rect.Y);
		}

		[Test]
		public void FlyoutHeightAndWidthIncreaseAndDecreaseCorrectly()
		{
			RunningApp.WaitForElement("PageLoaded");
			this.ShowFlyout();
			TapInFlyout("ChangeFlyoutSizes", makeSureFlyoutStaysOpen: true);
			var initialWidth = RunningApp.WaitForElement("FlyoutHeader")[0].Rect.Width;
			var initialHeight = RunningApp.WaitForElement("FlyoutFooter")[0].Rect.Y;
			TapInFlyout("DecreaseFlyoutSizes", makeSureFlyoutStaysOpen: true);
			var newWidth = RunningApp.WaitForElement("FlyoutHeader")[0].Rect.Width;
			var newHeight = RunningApp.WaitForElement("FlyoutFooter")[0].Rect.Y;

			Assert.That(initialWidth - newWidth, Is.EqualTo(10).Within(1));
			Assert.That(initialHeight - newHeight, Is.EqualTo(10).Within(1));
		}
#endif
	}
}
