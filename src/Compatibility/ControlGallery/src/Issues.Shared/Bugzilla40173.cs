using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Microsoft.Maui.Graphics;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
	[Category(UITestCategories.BoxView)]
	[Category(UITestCategories.InputTransparent)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.UwpIgnore)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 40173, "Android BoxView/Frame not clickthrough in ListView")]
	public class Bugzilla40173 : TestContentPage
	{
		const string CantTouchButtonId = "CantTouchButtonId";
		const string CanTouchButtonId = "CanTouchButtonId";
		const string ListTapTarget = "ListTapTarget";
		const string CantTouchFailText = "Failed";
		const string CanTouchSuccessText = "ButtonTapped";
		const string ListTapSuccessText = "ItemTapped";

#if UITEST
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiAndroid]
[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnMauiIOS]
		[Test]
		public void ButtonBlocked()
		{
			RunningApp.Tap(q => q.All().Marked(CantTouchButtonId));
			RunningApp.WaitForNoElement(q => q.All().Text(CantTouchFailText));

			RunningApp.Tap(q => q.All().Marked(CanTouchButtonId));
			RunningApp.WaitForElement(q => q.All().Text(CanTouchSuccessText));
#if !__MACOS__
			RunningApp.Tap(q => q.All().Marked(ListTapTarget));
			RunningApp.WaitForElement(q => q.All().Text(ListTapSuccessText));
#endif
		}
#endif

		protected override void Init()
		{
			var outputLabel = new Label();
			var testButton = new Button
			{
				Text = "Can't Touch This",
				AutomationId = CantTouchButtonId
			};

			testButton.Clicked += (sender, args) => outputLabel.Text = CantTouchFailText;

			var boxView = new BoxView
			{
				AutomationId = "nontransparentBoxView",
				Color = Colors.Pink.MultiplyAlpha(0.5f)
			};

			// Bump up the elevation on Android so the Button is covered (FastRenderers)
			boxView.On<Android>().SetElevation(10f);

			var testGrid = new Grid
			{
				AutomationId = "testgrid",
				Children =
				{
					testButton,
					boxView
				}
			};

			// BoxView over Button prevents Button click
			var testButtonOk = new Button
			{
				Text = "Can Touch This",
				AutomationId = CanTouchButtonId
			};

			testButtonOk.Clicked += (sender, args) =>
			{
				outputLabel.Text = CanTouchSuccessText;
			};

			var testGridOk = new Grid
			{
				AutomationId = "testgridOK",
				Children =
				{
					testButtonOk,
					new BoxView
					{
						AutomationId = "transparentBoxView",
						Color = Colors.Pink.MultiplyAlpha(0.5f),
						InputTransparent = true
					}
				}
			};

			var testListView = new ListView();
			var items = new[] { "Foo" };
			testListView.ItemsSource = items;
			testListView.ItemTemplate = new DataTemplate(() =>
			{
				var result = new ViewCell
				{
					View = new Grid
					{
						Children =
						{
							new BoxView
							{
								AutomationId = ListTapTarget,
								Color = Colors.Pink.MultiplyAlpha(0.5f)
							}
						}
					}
				};

				return result;
			});

			testListView.ItemSelected += (sender, args) => outputLabel.Text = ListTapSuccessText;

			Content = new StackLayout
			{
				AutomationId = "Container Stack Layout",
				Children = { outputLabel, testGrid, testGridOk, testListView }
			};
		}
	}
}