using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.BoxView)]
#endif

    [Preserve(AllMembers = true)]
    [Issue(IssueTracker.Bugzilla, 40173, "Android BoxView/Frame not clickthrough in ListView")]
	public class Bugzilla40173 : TestContentPage // or TestMasterDetailPage, etc ...
    {
        const string CantTouchButtonId = "CantTouchButtonId";
        const string CanTouchButtonId = "CanTouchButtonId";
        const string ListTapTarget = "ListTapTarget";
        const string CantTouchFailText = "Failed";
        const string CanTouchSuccessText = "ButtonTapped";
        const string ListTapSuccessText = "ItemTapped";

#if UITEST
        [Test]
        public void ButtonBlocked()
        {
            RunningApp.Tap(q => q.All().Marked(CantTouchButtonId));
            RunningApp.WaitForNoElement(q => q.All().Text(CantTouchFailText));

            RunningApp.Tap(q => q.All().Marked(CanTouchButtonId));
            RunningApp.WaitForElement(q => q.All().Text(CanTouchSuccessText));

            RunningApp.Tap(q => q.All().Marked(ListTapTarget));
            RunningApp.WaitForElement(q => q.All().Text(ListTapSuccessText));
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

            var testGrid = new Grid
            {
                Children =
                {
                    testButton,
                    new BoxView
                    {
                        Color = Color.Pink.MultiplyAlpha(0.5)
                    }
                }
            };

            // BoxView over Button prevents Button click
            var testButtonOk = new Button
            {
                Text = "Can Touch This",
                AutomationId = CanTouchButtonId
            };

            testButtonOk.Clicked += (sender, args) => outputLabel.Text = CanTouchSuccessText;

            var testGridOk = new Grid
            {
                Children =
                {
                    testButtonOk,
                    new BoxView
                    {
                        Color = Color.Pink.MultiplyAlpha(0.5),
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
                                Color = Color.Pink.MultiplyAlpha(0.5)
                            }
                        }
                    }
                };

                return result;
            });

            testListView.ItemSelected += (sender, args) => outputLabel.Text = ListTapSuccessText;

            Content = new StackLayout
            {
                Children = { outputLabel, testGrid, testGridOk, testListView }
            };
        }
    }
}