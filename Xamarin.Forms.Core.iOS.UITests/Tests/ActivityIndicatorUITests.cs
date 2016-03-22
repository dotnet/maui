using System;
using System.IO;
using System.Linq;

using NUnit.Framework;

using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("ActivityIndicator")]
	internal class ActivityIndicatorUITests : _ViewUITests
	{
		public ActivityIndicatorUITests ()
		{
			PlatformViewType = Views.ActivityIndicator;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ActivityIndicatorGallery);
		}

		// View tests
		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _Focus () {}

		public override void _GestureRecognizers ()
		{
			// TODO Can implement this
			var remote = new ViewContainerRemote (App, Test.View.GestureRecognizers, PlatformViewType);
			remote.GoTo ();
		}
		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _IsEnabled () {}

		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _IsFocused () {}

		[UiTestExempt (ExemptReason.CannotTest, "Invalid interaction")]
		public override void _UnFocus () {}

		//[UiTest (typeof(ActivityIndicator), "Color")]
		public void Color ()
		{
			//TODO: this was failing and is changing in next version of calabash (UI-Test-pre nuget) to a json rgb

//			var remote = RemoteFactory.CreateRemote<ViewContainerRemote> (App, "Color", PlatformViewType);
//			remote.GoTo ();
//
//			var color = remote.GetProperty<Color> (ActivityIndicator.ColorProperty);
//			Assert.AreEqual (Forms.Color.Lime, color);
		}

		// ActivityIndicator tests
		[Test]
		[UiTest (typeof(ActivityIndicator), "IsRunning")]
		public void IsRunning ()
		{
			var remote = new ViewContainerRemote (App, Test.ActivityIndicator.IsRunning, PlatformViewType);
			remote.GoTo ();

			var isRunning = remote.GetProperty<bool> (ActivityIndicator.IsRunningProperty);
			Assert.IsTrue (isRunning);
		}
		
		protected override void FixtureTeardown ()
		{
			App.NavigateBack ();
			base.FixtureTeardown ();
		}

	}
}