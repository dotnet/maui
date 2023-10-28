using Maui.Controls.Sample;
#if !USE_BROWSERSTACK
using Microsoft.Maui.Controls;
#endif
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public abstract class _ViewUITests : CoreGalleryBasePageTest
	{
		public _ViewUITests(TestDevice device) : base(device) { }

		// BrowserStack currently doesn't support [Test] methods in base classes, producing this error at runtime:
		// "System.ArgumentException : You can only patch implemented methods/constructors. Patch the declared method System.Void Microsoft.Maui.AppiumTests._ViewUITests::_IsEnabled() instead.
		// Work around this for now by removing the [Test] attribute, so it's not tested by default.
#if !USE_BROWSERSTACK
		[Test]
#endif
		public virtual void _IsEnabled()
		{
			var remote = new StateViewContainerRemote(UITestContext, Test.VisualElement.IsEnabled);
			remote.GoTo();

#if !USE_BROWSERSTACK
			var enabled = remote.GetProperty<bool>(View.IsEnabledProperty);
			Assert.IsTrue(enabled);
#endif

			remote.TapStateButton();

#if !USE_BROWSERSTACK
			enabled = remote.GetProperty<bool>(View.IsEnabledProperty);
			Assert.IsFalse(enabled);
#endif

			remote.TapStateButton();

			var isEnabled = remote.GetStateLabel().GetText();
			Assert.AreEqual("True", isEnabled);

			remote.TapStateButton();

			var isDisabled = remote.GetStateLabel().GetText();
			Assert.AreEqual("False", isDisabled);
		}

		// BrowserStack currently doesn't support [Test] methods in base classes, producing this error at runtime:
		// "System.ArgumentException : You can only patch implemented methods/constructors. Patch the declared method System.Void Microsoft.Maui.AppiumTests._ViewUITests::_IsVisible() instead.
		// Work around this for now by removing the [Test] attribute, so it's not tested by default.
#if !USE_BROWSERSTACK
		[Test]
#endif
		public virtual void _IsVisible()
		{
			var remote = new StateViewContainerRemote(UITestContext, Test.VisualElement.IsVisible);
			remote.GoTo();
			App.WaitForElement($"IsVisibleStateButton");
			var viewPre = remote.GetViews();

			Assert.AreEqual(1, viewPre.Count);

			remote.TapStateButton();

			var viewPost = remote.GetViews();

			Assert.AreEqual(0, viewPost.Count);
		}
	}

}
