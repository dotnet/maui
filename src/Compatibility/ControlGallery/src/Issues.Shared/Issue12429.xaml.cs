using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
	[Issue(IssueTracker.Github, 12429, "[Bug] Shell flyout items have a minimum height", PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public partial class Issue12429 : TestShell
	{
		public double SmallFlyoutItem { get; }
		public double SizeToModifyBy { get; }

		public Issue12429()
		{
			SmallFlyoutItem = 35d;
			SizeToModifyBy = 20d;

#if APP
			InitializeComponent();


			if (Device.RuntimePlatform == Device.Android)
				SmallFlyoutItem = SmallFlyoutItem / Device.info.ScalingFactor;

			if (Device.RuntimePlatform == Device.Android)
				SizeToModifyBy = SizeToModifyBy / Device.info.ScalingFactor;
#endif


			this.BindingContext = this;
		}

		protected override void Init()
		{
		}

		void ResizeFlyoutItem(System.Object sender, System.EventArgs e)
		{
			((sender as Element).Parent as VisualElement).HeightRequest += SizeToModifyBy;
		}

		void ResizeFlyoutItemDown(System.Object sender, System.EventArgs e)
		{
			((sender as Element).Parent as VisualElement).HeightRequest -= SizeToModifyBy;
		}

#if UITEST
		[Test]
		public void FlyoutItemSizesToExplicitHeight()
		{
			RunningApp.WaitForElement("PageLoaded");
			this.ShowFlyout();
			var height = RunningApp.WaitForElement("SmallFlyoutItem")[0].Rect.Height;
			Assert.That(height, Is.EqualTo(SmallFlyoutItem).Within(1));
		}


		[Test]
		public void FlyoutItemHeightAndWidthIncreaseAndDecreaseCorrectly()
		{
			RunningApp.WaitForElement("PageLoaded");
			this.ShowFlyout();
			var initialHeight = RunningApp.WaitForElement("ResizeMe")[0].Rect.Height;

			TapInFlyout("ResizeFlyoutItem", makeSureFlyoutStaysOpen: true);
			var newHeight = RunningApp.WaitForElement("ResizeMe")[0].Rect.Height;
			Assert.That(newHeight - initialHeight, Is.EqualTo(SizeToModifyBy).Within(1));

			TapInFlyout("ResizeFlyoutItemDown", makeSureFlyoutStaysOpen: true);
			newHeight = RunningApp.WaitForElement("ResizeMe")[0].Rect.Height;
			Assert.That(initialHeight, Is.EqualTo(newHeight).Within(1));

			TapInFlyout("ResizeFlyoutItemDown", makeSureFlyoutStaysOpen: true);
			newHeight = RunningApp.WaitForElement("ResizeMe")[0].Rect.Height;
			Assert.That(initialHeight - newHeight, Is.EqualTo(SizeToModifyBy).Within(1));

		}
#endif

	}


	[Preserve(AllMembers = true)]
	public class Issue12429Page : ContentPage
	{
		public Issue12429Page()
		{
			Background = SolidColorBrush.White;
			var label = new Label
			{
				Text = "Flyout Item 1: Explicit Height of 35, Flyout Item 2: will grow and shrink when you click the buttons, Flyout Item 3: doesn't exist, and Flyout Item 4: uses the default platform sizes.",
				VerticalTextAlignment = TextAlignment.Center,
				TextColor = Color.Black,
				AutomationId = "PageLoaded"
			};

			Content = new StackLayout()
			{
				Children =
				{
					new Label
					{
						Text = "Flyout Item 1: Explicit Height of 35.",
						VerticalTextAlignment = TextAlignment.Center,
						TextColor = Color.Black,
						AutomationId = "PageLoaded"
					},
					new Label
					{
						Text = "Flyout Item 2: Height sizes to the content.",
						VerticalTextAlignment = TextAlignment.Center,
						TextColor = Color.Black
					},
					new Label
					{
						Text = "Flyout Item 3: will grow and shrink when you click the buttons.",
						VerticalTextAlignment = TextAlignment.Center,
						TextColor = Color.Black
					},
					new Label
					{
						Text = "Flyout Item 4: doesn't exist. You should only see 4 Flyout Items",
						VerticalTextAlignment = TextAlignment.Center,
						TextColor = Color.Black
					},
					new Label
					{
						Text = "Flyout Item 5: uses the default height if no templates are used.",
						VerticalTextAlignment = TextAlignment.Center,
						TextColor = Color.Black
					}
				}
			};
		}
	}

}