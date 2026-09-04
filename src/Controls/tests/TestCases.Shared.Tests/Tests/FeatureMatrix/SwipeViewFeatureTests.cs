using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.SwipeView)]
public class SwipeViewFeatureTests : _GalleryUITest
{
	public const string SwipeViewFeatureMatrix = "SwipeView Feature Matrix";
	public override string GalleryPageName => SwipeViewFeatureMatrix;

	public SwipeViewFeatureTests(TestDevice device)
		: base(device)
	{
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

	[Test, Order(7)]
	public void VerifySwipeViewOpenRequestedEventFiresOnProgrammaticOpen()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		Assert.That(App.WaitForElement("OpenRequestedLabel").GetText(), Is.EqualTo("Open Requested: "));
		App.WaitForElement("OpenLeft");
		App.Tap("OpenLeft");
		Assert.That(App.WaitForElement("OpenRequestedLabel").GetText(), Is.EqualTo("Open Requested: LeftItems"));
		App.WaitForElement("OpenRight");
		App.Tap("OpenRight");
		Assert.That(App.WaitForElement("OpenRequestedLabel").GetText(), Is.EqualTo("Open Requested: RightItems"));
		App.WaitForElement("OpenTop");
		App.Tap("OpenTop");
		Assert.That(App.WaitForElement("OpenRequestedLabel").GetText(), Is.EqualTo("Open Requested: TopItems"));
		App.WaitForElement("OpenBottom");
		App.Tap("OpenBottom");
		Assert.That(App.WaitForElement("OpenRequestedLabel").GetText(), Is.EqualTo("Open Requested: BottomItems"));
	}

	[Test, Order(8)]
	public void VerifySwipeViewCloseRequestedEventFiresOnProgrammaticClose()
	{
		App.WaitForElement("Options");
		Assert.That(App.WaitForElement("CloseRequestedLabel").GetText(), Is.EqualTo("Close Requested: "));
		App.WaitForElement("CloseSwipeViewButton");
		App.Tap("CloseSwipeViewButton");
		Assert.That(App.WaitForElement("CloseRequestedLabel").GetText(), Is.EqualTo("Close Requested: Animated=True"));
	}

	[Test, Order(9)]
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

	[Test, Order(10)]
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
	[Test, Order(11)]
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

	[Test, Order(12)]
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

	[Test, Order(13)]
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

	[Test, Order(14)]
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

	[Test, Order(15)]
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

	[Test, Order(16)]
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

	[Test, Order(17)]
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

	[Test, Order(18)]
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

	[Test, Order(19)]
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
	[Test, Order(20)]
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

	[Test, Order(21)]
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
	[Test, Order(22)]
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

	[Test, Order(23)]
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

	[Test, Order(24)]
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

	[Test, Order(25)]
	public void VerifyDisabledSwipeViewCanBeOpenedAndClosedProgrammatically()
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
		App.Tap("OpenLeft");
		Assert.That(App.WaitForElement("OpenRequestedLabel").GetText(), Is.EqualTo("Open Requested: LeftItems"));
		Assert.That(App.WaitForElement("Label"), Is.Not.Null);
		App.Tap("CloseSwipeViewButton");
		Assert.That(App.WaitForElement("CloseRequestedLabel").GetText(), Is.EqualTo("Close Requested: Animated=True"));
		App.WaitForNoElement("Label");
	}
#endif

	[Test, Order(26)]
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

	[Test, Order(27)]
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

	[Test, Order(28)]
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
	[Test, Order(29)]
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

	[Test, Order(30)]
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
		App.SwipeLeftToRight("SwipeViewImage", swipePercentage: 0.90);
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Label Invoked"));
	}

	[Test, Order(31)]
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

	[Test, Order(32)]
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

	[Test, Order(33)]
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
		bool iconDismissed = false;
		for (int i = 0; i < 3 && !iconDismissed; i++)
		{
			try
			{
				App.WaitForElement("Icon");
				App.Tap("Icon");
				App.WaitForNoElement("Icon");
				iconDismissed = true;
				break;
			}
			catch (Exception)
			{
				// retry
			}
		}
		Assert.That(iconDismissed, Is.True, "Icon did not disappear after 3 attempts.");
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/27436
	[Test, Order(34)]
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

	[Test, Order(35)]
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

	[Test, Order(36)]
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

	[Test, Order(37)]
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

	[Test, Order(38)]
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

	[Test, Order(39)]
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

	[Test, Order(40)]
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

	[Test, Order(41)]
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
	[Test, Order(42)]
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

	[Test, Order(43)]
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
	[Test, Order(44)]
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

	[Test, Order(45)]
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

	[Test, Order(46)]
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
	[Test, Order(47)]
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

	[Test, Order(48)]
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
		App.SwipeLeftToRight("SwipeViewImage", swipePercentage: 0.90);
		App.WaitForElement("Label");
	}

	[Test, Order(49)]
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
		App.SwipeLeftToRight("SwipeViewImage", swipePercentage: 0.90);
		App.WaitForElement("Icon");
	}

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/27436
	[Test, Order(50)]
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

	[Test, Order(51)]
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

	[Test, Order(52)]
	public void VerifySwipeViewWithMultipleSwipeItemsPerSide()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("UseMultipleSwipeItemsCheckBox");
		App.Tap("UseMultipleSwipeItemsCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Label");
		App.WaitForElement("Label2");
		App.Tap("Label2");
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Label2 Invoked"));
	}

	[Test, Order(53)]
	public void VerifySwipeViewWithDisabledSwipeItem()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DisableSwipeItemCheckBox");
		App.Tap("DisableSwipeItemCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Label");
		VerifySwipeViewScreenshot();
	}

	[Test, Order(54)]
	public void VerifySwipeViewWithSwipeItemCommandBinding()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("UseCommandBindingCheckBox");
		App.Tap("UseCommandBindingCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Label");
		App.Tap("Label");
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Command Invoked: Label"));
	}

	[Test, Order(55)]
	public void VerifySwipeViewWithCombinedThresholdSwipeModeAndBackground()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThresholdEntry");
		App.ClearText("ThresholdEntry");
		App.EnterText("ThresholdEntry", "30");
		App.WaitForElement("ExecuteSwipeMode");
		App.Tap("ExecuteSwipeMode");
		App.WaitForElement("SkyBlueBackground");
		App.Tap("SkyBlueBackground");
		App.WaitForElement("YellowSwipeItemBackground");
		App.Tap("YellowSwipeItemBackground");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		Assert.That(App.WaitForElement("EventInvokedLabel").GetText(), Is.EqualTo("Label Invoked"));
		VerifySwipeViewScreenshot();
	}

	[Test, Order(56)]
	public void VerifySwipeChangingEventFiresMultipleTimesDuringGesture()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		Assert.That(App.WaitForElement("SwipeChangingCountLabel").GetText(), Is.EqualTo("0"));
		App.SwipeLeftToRight("SwipeViewControl");
		var countText = App.WaitForElement("SwipeChangingCountLabel").GetText();
		Assert.That(int.TryParse(countText, out var count), Is.True,
			$"SwipeChangingCountLabel should contain an integer, was: '{countText}'");
		Assert.That(count, Is.GreaterThan(1),
			$"SwipeChanging should fire multiple times during a swipe gesture, but fired {count} times.");
	}

	[Test, Order(57)]
	public void VerifySwipeItemHiddenWhenIsVisibleFalse()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("HideSwipeItemCheckBox");
		App.Tap("HideSwipeItemCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		// The Label swipe item should not be visible/tappable when IsVisible=false.
		Assert.That(App.FindElement("Label"), Is.Null,
			"SwipeItem with IsVisible=false should not be present in the UI after swiping.");
	}

	[Test, Order(58)]
	public void VerifySwipeEndedIsOpenFalseWhenSwipeBelowThreshold()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThresholdEntry");
		App.ClearText("ThresholdEntry");
		App.EnterText("ThresholdEntry", "5000");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		var ended = App.WaitForElement("SwipeEndedLabel").GetText();
		Assert.That(ended, Does.Contain("IsOpen: Open"),
			$"When swipe distance is below Threshold, SwipeEnded.IsOpen must be true. Was: '{ended}'");
	}

	[Test, Order(59)]
	public void VerifyThresholdZero()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ThresholdEntry");
		App.ClearText("ThresholdEntry");
		App.EnterText("ThresholdEntry", "0");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");
		App.WaitForElement("Label");
		Assert.That(App.WaitForElement("SwipeStartedLabel").GetText(), Is.EqualTo("Swipe Started: Right"));
	}

	[Test, Order(60)]
	public void VerifyMultipleSwipeItemsOnAllSides()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("UseMultipleSwipeItemsCheckBox");
		App.Tap("UseMultipleSwipeItemsCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");

		App.SwipeLeftToRight("SwipeViewControl");
		VerifyMultipleSwipeItems("Right");
		CloseSwipeView();

		App.SwipeRightToLeft("SwipeViewControl");
		VerifyMultipleSwipeItems("Left");
		CloseSwipeView();

		var swipeViewRect = App.WaitForElement("SwipeViewControl").GetRect();
		App.DragCoordinates(
			swipeViewRect.CenterX(), swipeViewRect.Y + 10,
			swipeViewRect.CenterX(), swipeViewRect.Y + swipeViewRect.Height * 0.9f);
		VerifyMultipleSwipeItems("Down");
		CloseSwipeView();

		App.DragCoordinates(
			swipeViewRect.CenterX(), swipeViewRect.Y + swipeViewRect.Height - 10,
			swipeViewRect.CenterX(), swipeViewRect.Y + swipeViewRect.Height * 0.1f);
		VerifyMultipleSwipeItems("Up");
	}

	[Test, Order(61)]
	public void VerifyDisabledSwipeItemDoesNotExecuteBoundCommand()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DisableSwipeItemCheckBox");
		App.Tap("DisableSwipeItemCheckBox");
		App.WaitForElement("UseCommandBindingCheckBox");
		App.Tap("UseCommandBindingCheckBox");
		App.WaitForElement("Apply");
		App.Tap("Apply");
		App.WaitForElement("SwipeViewControl");
		App.SwipeLeftToRight("SwipeViewControl");

		var disabledItem = App.WaitForElement("Label").GetRect();
		App.TapCoordinates(disabledItem.CenterX(), disabledItem.CenterY());

		Assert.That(
			App.WaitForElement("EventInvokedLabel").GetText(),
			Is.EqualTo("Event not invoked yet"));
	}
#endif

	private void VerifyMultipleSwipeItems(string expectedDirection)
	{
		App.WaitForElement("Label");
		App.WaitForElement("Label2");
		Assert.That(
			App.WaitForTextToBePresentInElement(
				"SwipeEndedLabel",
				$"Swipe Ended: {expectedDirection}",
				timeout: TimeSpan.FromSeconds(3)),
			Is.True,
			$"The {expectedDirection} swipe did not reveal its two SwipeItems.");
	}

	private void CloseSwipeView()
	{
		App.Tap("CloseSwipeViewButton");
		App.WaitForNoElement("Label2");
	}

	private void VerifySwipeViewScreenshot()
	{
#if WINDOWS
		VerifyScreenshot(cropTop: 100);
#else
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#endif
	}
}
