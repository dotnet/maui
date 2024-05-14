using System.Runtime.CompilerServices;
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
		public virtual void IsEnabled()
		{
			var remote = GoToStateRemote();

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
		public virtual void IsVisible()
		{
			var remote = GoToStateRemote();

			App.WaitForElement($"IsVisibleStateButton");
			var viewPre = remote.GetViews();

			Assert.AreEqual(1, viewPre.Count);

			remote.TapStateButton();

			var viewPost = remote.GetViews();

			Assert.AreEqual(0, viewPost.Count);
		}

		internal StateViewContainerRemote GoToStateRemote([CallerMemberName] string? testName = null)
		{
			_ = testName ?? throw new ArgumentNullException(nameof(testName));

			var remote = new StateViewContainerRemote(UITestContext, testName);
			remote.GoTo(testName);
			return remote;
		}

		internal EventViewContainerRemote GoToEventRemote([CallerMemberName] string? testName = null)
		{
			_ = testName ?? throw new ArgumentNullException(nameof(testName));

			var remote = new EventViewContainerRemote(UITestContext, testName);
			remote.GoTo(testName);
			return remote;
		}

		internal ViewContainerRemote GoToRemote([CallerMemberName] string? testName = null)
		{
			_ = testName ?? throw new ArgumentNullException(nameof(testName));

			var remote = new ViewContainerRemote(UITestContext, testName);
			remote.GoTo(testName);
			return remote;
		}
	}
}
