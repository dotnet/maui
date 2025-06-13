using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public abstract class _ViewUITests : CoreGalleryBasePageTest
	{
		public _ViewUITests(TestDevice device) : base(device) { }

		[Fact]
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
			Assert.Equal("True", isEnabled);

			remote.TapStateButton();

			var isDisabled = remote.GetStateLabel().GetText();
			Assert.Equal("False", isDisabled);
		}

		[Fact]
		[Category(UITestCategories.IsVisible)]
		public virtual void IsVisible()
		{
			var remote = GoToStateRemote();

			App.WaitForElement($"IsVisibleStateButton");
			var viewPre = remote.GetViews();

			Assert.Equal(1, viewPre.Count);

			remote.TapStateButton();

			var viewPost = remote.GetViews();

			Assert.Equal(0, viewPost.Count);
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
