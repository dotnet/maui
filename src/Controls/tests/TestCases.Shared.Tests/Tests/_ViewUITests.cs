using Maui.Controls.Sample;
using Microsoft.Maui.Controls;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
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
			ClassicAssert.IsTrue(enabled);

			remote.TapStateButton();

			enabled = remote.GetProperty<bool>(View.IsEnabledProperty);
			ClassicAssert.IsFalse(enabled);

			remote.TapStateButton();

			var isEnabled = remote.GetStateLabel().GetText();
			ClassicAssert.AreEqual("True", isEnabled);

			remote.TapStateButton();

			var isDisabled = remote.GetStateLabel().GetText();
			ClassicAssert.AreEqual("False", isDisabled);
		}

		[Test]
		public virtual void _IsVisible()
		{
			var remote = new StateViewContainerRemote(UITestContext, Test.VisualElement.IsVisible);
			remote.GoTo();
			App.WaitForElement($"IsVisibleStateButton");
			var viewPre = remote.GetViews();

			ClassicAssert.AreEqual(1, viewPre.Count);

			remote.TapStateButton();

			var viewPost = remote.GetViews();

			ClassicAssert.AreEqual(0, viewPost.Count);
		}
	}

}
