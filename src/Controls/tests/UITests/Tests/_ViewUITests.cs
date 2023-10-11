using Maui.Controls.Sample;
using Microsoft.Maui.Controls;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public abstract class _ViewUITests : CoreGalleryBasePageTest
	{
		public _ViewUITests(TestDevice device) : base(device) { }

		[Test]
		public virtual void _IsEnabled()
		{
			var remote = new StateViewContainerRemote(UITestContext, Test.VisualElement.IsEnabled);
			remote.GoTo();

			var enabled = remote.GetProperty<bool>(View.IsEnabledProperty);
			Assert.IsTrue(enabled);

			remote.TapStateButton();

			enabled = remote.GetProperty<bool>(View.IsEnabledProperty);
			Assert.IsFalse(enabled);

			remote.TapStateButton();

			var isEnabled = remote.GetStateLabel().GetText();
			Assert.AreEqual("True", isEnabled);

			remote.TapStateButton();

			var isDisabled = remote.GetStateLabel().GetText();
			Assert.AreEqual("False", isDisabled);
		}

		[Test]
		public virtual void _IsVisible()
		{
			var remote = new StateViewContainerRemote(UITestContext, Test.VisualElement.IsVisible);
			remote.GoTo();

			var viewPre = remote.GetViews();

			Assert.AreEqual(1, viewPre.Count);

			remote.TapStateButton();

			var viewPost = remote.GetViews();

			Assert.AreEqual(0, viewPost.Count);
		}
	}

}
