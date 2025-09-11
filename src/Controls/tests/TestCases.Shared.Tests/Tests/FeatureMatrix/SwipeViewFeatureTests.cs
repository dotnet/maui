using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.SwipeView)]
public class SwipeViewFeatureTests : UITest
{
	public const string SwipeViewFeatureMatrix = "SwipeView Feature Matrix";

	public SwipeViewFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(SwipeViewFeatureMatrix);
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/30949 && https://github.com/dotnet/maui/issues/30947
	[Test, Order(1)]
	public void VerifySwipeViewWhenLabelSwipeItemAndEvents()
	{
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Label");
		App.Tap("Label");
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Label Invoked"));
		Assert.That(App.WaitForElement("SwipeStartedLabel").GetText(), Is.EqualTo("Swipe Started: Right"));
		Assert.That(App.WaitForElement("SwipeChangingLabel").GetText(), Is.EqualTo("Swipe Changing: Right"));
		Assert.That(App.WaitForElement("SwipeEndedLabel").GetText(), Is.EqualTo("Swipe Ended: Right, IsOpen: Open"));
	}

	[Test, Order(2)]
	public void VerifySwipeViewWhenImageSwipeItemAndEvents()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IconImageSourceSwipeItem");
		App.Tap("IconImageSourceSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Icon");
		App.Tap("Icon");
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Icon Invoked"));
		Assert.That(App.WaitForElement("SwipeStartedLabel").GetText(), Is.EqualTo("Swipe Started: Right"));
		Assert.That(App.WaitForElement("SwipeChangingLabel").GetText(), Is.EqualTo("Swipe Changing: Right"));
		Assert.That(App.WaitForElement("SwipeEndedLabel").GetText(), Is.EqualTo("Swipe Ended: Right, IsOpen: Open"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS //In Android, Buttton SwipeItem is not Invoked & On Windows, related issue link: https://github.com/dotnet/maui/issues/27436
	[Test, Order(3)]
	public void VerifySwipeViewWhenButtonSwipeItemAndEvents()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ButtonSwipeItem");
		App.Tap("ButtonSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Click Me");
		App.Tap("Click Me");
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Button Clicked"));
		Assert.That(App.WaitForElement("SwipeStartedLabel").GetText(), Is.EqualTo("Swipe Started: Right"));
		Assert.That(App.WaitForElement("SwipeChangingLabel").GetText(), Is.EqualTo("Swipe Changing: Right"));
		Assert.That(App.WaitForElement("SwipeEndedLabel").GetText(), Is.EqualTo("Swipe Ended: Right, IsOpen: Open"));
	}
#endif
#endif

#if TEST_FAILS_ON_WINDOWS //related issue link:  https://github.com/dotnet/maui/issues/14777
	[Test, Order(4)]
	public void VerifySwipeViewWhenLabelContentAndProgrammaticActions()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.WaitForElement("OpenLeft");
		App.Tap("OpenLeft");
		App.WaitForElement("Label");
		App.Tap("OpenRight");
		App.WaitForElement("Label");
		App.Tap("OpenTop");
		App.WaitForElement("Label");
		App.Tap("OpenBottom");
		App.WaitForElement("Label");
		App.Tap("CloseSwipeViewButton");
		App.WaitForNoElement("Label");
	}

	[Test, Order(5)]
	public void VerifySwipeViewWithImageContentAndProgrammaticActions()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewImage");
		App.WaitForElement("OpenLeft");
		App.Tap("OpenLeft");
		App.WaitForElement("Label");
		App.Tap("OpenRight");
		App.WaitForElement("Label");
		App.Tap("OpenTop");
		App.WaitForElement("Label");
		App.Tap("OpenBottom");
		App.WaitForElement("Label");
		App.WaitForElement("CloseSwipeViewButton");
		App.Tap("CloseSwipeViewButton");
		App.WaitForNoElement("Label");
	}

	[Test, Order(6)]
	public void VerifySwipeViewWithCollectionViewContentAndProgrammaticActions()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewCollectionItem");
		App.WaitForElement("OpenLeft");
		App.Tap("OpenLeft");
		App.WaitForNoElement("Label");
		App.Tap("OpenRight");
		App.WaitForNoElement("Label");
		App.SwipeLeftToRight("Item 4");
		App.WaitForElement("Label");
		App.WaitForElement("CloseSwipeViewButton");
		App.Tap("CloseSwipeViewButton");
		App.WaitForElement("Label");
	}
#endif

	[Test]
	public void VerifySwipeViewWithImageContentChanged()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControlLabel");
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeViewWithCollectionViewContentChanged()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControlLabel");
		VerifySwipeViewScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/30947
	[Test]
	public void VerifySwipeViewWithLabelContentAndThreshold()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThresholdEntry");
		App.ClearText("ThresholdEntry");
		App.EnterText("ThresholdEntry", "30");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewLabel");
		App.SwipeLeftToRight("SwipeViewLabel");
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeViewWithImageContentAndThreshold()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("ThresholdEntry");
		App.ClearText("ThresholdEntry");
		App.EnterText("ThresholdEntry", "30");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewImage");
		App.SwipeLeftToRight("SwipeViewImage");
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeViewWithCollectionViewContentAndThreshold()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("ThresholdEntry");
		App.ClearText("ThresholdEntry");
		App.EnterText("ThresholdEntry", "30");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControlLabel");
		App.SwipeLeftToRight("Item 4");
		VerifySwipeViewScreenshot();
	}
#endif

	[Test]
	public void VerifySwipeViewWithLabelContentAndBackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("LightGreenBackground");
		App.Tap("LightGreenBackground");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControlLabel");
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeViewWithImageContentAndBackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("LightGreenBackground");
		App.Tap("LightGreenBackground");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControlLabel");
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeViewWithCollectionViewContentAndBackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("LightPinkBackground");
		App.Tap("LightPinkBackground");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControlLabel");
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeViewWithLabelContentAndFlowDirection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControlLabel");
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeViewWithImageContentAndFlowDirection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControlLabel");
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeViewWithCollectionViewContentAndFlowDirection()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("FlowDirectionRightToLeft");
		App.Tap("FlowDirectionRightToLeft");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControlLabel");
		VerifySwipeViewScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/29812
	[Test]
	public void VerifySwipeViewWithLabelContentAndShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControlLabel");
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeViewWithImageContentAndShadow()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("ShadowCheckBox");
		App.Tap("ShadowCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControlLabel");
		VerifySwipeViewScreenshot();
	}
#endif

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/30947
	[Test]
	public void VerifySwipeViewWithLabelContentAndIsEnabledFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalse");
		App.Tap("IsEnabledFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForNoElement("Label");
	}

	[Test]
	public void VerifySwipeViewWithImageContentAndIsEnabledFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("IsEnabledFalse");
		App.Tap("IsEnabledFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewImage");
		App.SwipeLeftToRight("SwipeViewImage");
		App.WaitForNoElement("Label");
	}

	[Test]
	public void VerifySwipeViewWithCollectionViewContentAndIsEnabledFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("IsEnabledFalse");
		App.Tap("IsEnabledFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewCollectionItem");
		App.SwipeLeftToRight("SwipeViewCollectionItem");
		App.WaitForNoElement("Label");
	}
#endif

	[Test]
	public void VerifySwipeViewWithLabelContentAndIsVisibleFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsVisibleFalse");
		App.Tap("IsVisibleFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("SwipeViewControl");
	}

	[Test]
	public void VerifySwipeViewWithImageContentAndIsVisibleFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("IsVisibleFalse");
		App.Tap("IsVisibleFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("SwipeViewImage");
	}

	[Test]
	public void VerifySwipeViewWithCollectionViewContentAndIsVisibleFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("IsVisibleFalse");
		App.Tap("IsVisibleFalse");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForNoElement("SwipeViewCollectionItem");
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/30947
	[Test]
	public void VerifySwipeViewWithLabelContentSwipeMode()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ExecuteSwipeMode");
		App.Tap("ExecuteSwipeMode");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Label Invoked"));
	}

	[Test]
	public void VerifySwipeViewWithImageContentSwipeMode()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("ExecuteSwipeMode");
		App.Tap("ExecuteSwipeMode");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewImage");
		App.SwipeLeftToRight("SwipeViewImage");
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Label Invoked"));
	}

	[Test]
	public void VerifySwipeViewWithCollectionViewContentSwipeMode()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("ExecuteSwipeMode");
		App.Tap("ExecuteSwipeMode");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewCollectionItem");
		App.SwipeLeftToRight("SwipeViewCollectionItem");
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Label Invoked"));
	}

	[Test]
	public void VerifyLabelWithSwipeRevealAndSwipeBehaviorOnInvokedAuto()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Label");
		App.Tap("Label");
		App.WaitForNoElement("Label");
	}

	[Test]
	public void VerifyImageWithSwipeRevealAndSwipeBehaviorOnInvokedAuto()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("IconImageSourceSwipeItem");
		App.Tap("IconImageSourceSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewImage");
		App.SwipeLeftToRight("SwipeViewImage");
		bool iconDismissed = true;
        for (int i = 0; i < 3 && !iconDismissed; i++)
        {
            try
            {
                App.WaitForElement("Icon");
                App.Tap("Icon");
                App.WaitForNoElement("Icon");
                iconDismissed = false;
                break;
            }
            catch (Exception)
            {
                // retry
            }
 
    Assert.That(iconDismissed,Is.True,"Icon did not disappear after 3 attempts.");
    }
 
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/27436
	[Test]
	public void VerifyCollectionViewWithSwipeRevealAndSwipeBehaviorOnInvokedAuto()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("ButtonSwipeItem");
		App.Tap("ButtonSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewCollectionItem");
		App.SwipeLeftToRight("Item 3");
		App.WaitForElement("Click Me");
		App.Tap("Click Me");
		App.WaitForNoElement("Click Me");
	}
#endif

	[Test]
	public void VerifySwipeModeRevealWithSwipeBehaviorOnInvokedRemainOpen()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("RemainOpenSwipeBehavior");
		App.Tap("RemainOpenSwipeBehavior");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Label");
		App.Tap("Label");
		App.WaitForElement("Label");
	}

	[Test]
	public void VerifySwipeModeRevealWithSwipeBehaviorOnInvokedCloseSwipeViewButton()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CloseSwipeBehavior");
		App.Tap("CloseSwipeBehavior");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Label");
		App.Tap("Label");
		App.WaitForNoElement("Label");
	}

	[Test]
	public void VerifySwipeModeExecuteWithSwipeBehaviorOnInvokedAuto()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ExecuteSwipeMode");
		App.Tap("ExecuteSwipeMode");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Label Invoked"));
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeModeExecuteWithSwipeBehaviorOnInvokedRemainOpen()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ExecuteSwipeMode");
		App.Tap("ExecuteSwipeMode");
		App.WaitForElement("RemainOpenSwipeBehavior");
		App.Tap("RemainOpenSwipeBehavior");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Label");
		App.Tap("Label");
		App.WaitForElement("Label");
	}

	[Test]
	public void VerifySwipeModeExecuteWithSwipeBehaviorOnInvokedCloseSwipeViewButton()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ExecuteSwipeMode");
		App.Tap("ExecuteSwipeMode");
		App.WaitForElement("CloseSwipeBehavior");
		App.Tap("CloseSwipeBehavior");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Label Invoked"));
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeViewWithLabelSwipeItemsBackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("YellowSwipeItemBackground");
		App.Tap("YellowSwipeItemBackground");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifySwipeViewWithIconImageSwipeItemsBackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IconImageSourceSwipeItem");
		App.Tap("IconImageSourceSwipeItem");
		App.WaitForElement("YellowSwipeItemBackground");
		App.Tap("YellowSwipeItemBackground");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		VerifySwipeViewScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/27436
	[Test]
	public void VerifySwipeViewWithButtonSwipeItemsBackgroundColor()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ButtonSwipeItem");
		App.Tap("ButtonSwipeItem");
		App.WaitForElement("YellowSwipeItemBackground");
		App.Tap("YellowSwipeItemBackground");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		VerifySwipeViewScreenshot();
	}
#endif

	[Test]
	public void VerifySwipeViewWithIconImageSwipeItemChanged()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IconImageSourceSwipeItem");
		App.Tap("IconImageSourceSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Icon");
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/27436
	[Test]
	public void VerifySwipeViewWithButtonSwipeItemChanged()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ButtonSwipeItem");
		App.Tap("ButtonSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Click Me");
	}
#endif

	[Test]
	public void VerifyCollectionViewContentWithLabelSwipeItem()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("LabelSwipeItem");
		App.Tap("LabelSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewCollectionItem");
		App.SwipeLeftToRight("Item 3");
		App.SwipeLeftToRight("Item 6");
		App.WaitForElement("Label");
		VerifySwipeViewScreenshot();
	}

	[Test]
	public void VerifyCollectionViewContentWithIconImageSwipeItem()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("IconImageSourceSwipeItem");
		App.Tap("IconImageSourceSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewCollectionItem");
		App.SwipeLeftToRight("Item 2");
		App.SwipeLeftToRight("Item 4");
		App.WaitForElement("Icon");
		VerifySwipeViewScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/27436
	[Test]
	public void VerifyCollectionViewContentWithButtonSwipeItem()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("CollectionViewContent");
		App.Tap("CollectionViewContent");
		App.WaitForElement("ButtonSwipeItem");
		App.Tap("ButtonSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewCollectionItem");
		App.SwipeLeftToRight("Item 1");
		App.SwipeLeftToRight("Item 5");
		App.WaitForElement("Click Me");
		VerifySwipeViewScreenshot();
	}
#endif

	[Test]
	public void VerifyImageContentWithLabelSwipeItem()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("LabelSwipeItem");
		App.Tap("LabelSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewImage");
		App.SwipeLeftToRight("SwipeViewImage");
		App.WaitForElement("Label");
	}

	[Test]
	public void VerifyImageContentWithIconImageSwipeItem()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("IconImageSourceSwipeItem");
		App.Tap("IconImageSourceSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewImage");
		App.SwipeLeftToRight("SwipeViewImage");
		App.WaitForElement("Icon");
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/27436
	[Test]
	public void VerifyImageContentWithButtonSwipeItem()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ImageContent");
		App.Tap("ImageContent");
		App.WaitForElement("ButtonSwipeItem");
		App.Tap("ButtonSwipeItem");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewImage");
		App.SwipeLeftToRight("SwipeViewImage");
		App.WaitForElement("Click Me");
	}
#endif

	[Test]
	public void VerifyThresholdWithSwipeMode()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThresholdEntry");
		App.ClearText("ThresholdEntry");
		App.EnterText("ThresholdEntry", "20");
		App.WaitForElement("ExecuteSwipeMode");
		App.Tap("ExecuteSwipeMode");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		Assert.That(App.WaitForElement("SwipeStartedLabel").GetText(), Is.EqualTo("Swipe Started: Right"));
	}
#endif

	private void VerifySwipeViewScreenshot()
	{
#if WINDOWS
		VerifyScreenshot(cropTop: 100);
#else
		VerifyScreenshot();
#endif
	}
}