using NUnit.Framework;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category(UITestCategories.Entry)]
	internal class EntryUITests : _ViewUITests
	{
		public EntryUITests()
		{
			PlatformViewType = Views.Entry;
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.EntryGallery);
		}

		// TODO
		public override void _Focus()
		{
		}

		[UiTestExempt(ExemptReason.CannotTest, "Invalid interaction")]
		public override void _GestureRecognizers()
		{
		}

		// TODO
		public override void _IsFocused()
		{
		}

		// TODO
		public override void _UnFocus()
		{
		}

		// TODO
		// Implement control specific ui tests
		[Test]
		[UiTest(typeof(Entry), "Completed")]
		public void Completed()
		{
			var remote = new EventViewContainerRemote(App, Test.Entry.Completed, PlatformViewType);
			remote.GoTo();

			App.EnterText(q => q.Raw(remote.ViewQuery), "Test");

			App.PressEnter();

			var eventLabelText = remote.GetEventLabel().Text;
			Assert.AreEqual(eventLabelText, "Event: Completed (fired 1)");
		}

		protected override void FixtureTeardown()
		{
			App.NavigateBack();
			base.FixtureTeardown();
		}
	}
}