using System.Runtime.CompilerServices;
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
		[Category(UITestCategories.IsEnabled)]
		public virtual void IsEnabled()
		{
			var remote = GoToStateRemote();

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
		[Category(UITestCategories.IsVisible)]
		public virtual void IsVisible()
		{
			var remote = GoToStateRemote();

			App.WaitForElement($"IsVisibleStateButton");
			var viewPre = remote.GetViews();

			ClassicAssert.AreEqual(1, viewPre.Count);

			remote.TapStateButton();

			var viewPost = remote.GetViews();

			ClassicAssert.AreEqual(0, viewPost.Count);
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
