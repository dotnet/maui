using Maui.Controls.Sample;
using Microsoft.Maui.Appium;
using Microsoft.Maui.Controls;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests
{
	public abstract class _ViewUITests : UITestBase
	{
		public string? PlatformViewType { get; protected set; }

		public _ViewUITests(TestDevice device) : base(device) { }

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			NavigateToGallery();
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			App.NavigateBack();
		}

		protected abstract void NavigateToGallery();

		[Test]
		public virtual void _IsEnabled()
		{
			var remote = new StateViewContainerRemote(UITestContext, Test.VisualElement.IsEnabled, PlatformViewType);
			remote.GoTo();

			var enabled = remote.GetProperty<bool>(View.IsEnabledProperty);
			Assert.IsTrue(enabled);

			remote.TapStateButton();

			enabled = remote.GetProperty<bool>(View.IsEnabledProperty);
			Assert.IsFalse(enabled);

			remote.TapStateButton();

			var isEnabled = remote.GetStateLabel().ReadText();
			Assert.AreEqual("True", isEnabled);

			remote.TapStateButton();

			var isDisabled = remote.GetStateLabel().ReadText();
			Assert.AreEqual("False", isDisabled);
		}

		[Test]
		public virtual void _IsVisible()
		{
			var remote = new StateViewContainerRemote(UITestContext, Test.VisualElement.IsVisible, PlatformViewType);
			remote.GoTo();

			var viewPre = remote.GetViews();

			Assert.AreEqual(1, viewPre.Length);

			remote.TapStateButton();

			var viewPost = remote.GetViews();

			Assert.AreEqual(0, viewPost.Length);
		}
	}

}