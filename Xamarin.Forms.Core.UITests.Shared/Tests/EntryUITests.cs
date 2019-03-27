using NUnit.Framework;
using Xamarin.Forms.Controls.Issues;
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

		[Test]
		[UiTest(typeof(Entry), "Focus")]
		public override void _Focus()
		{
			var remote = new StateViewContainerRemote(App, Test.VisualElement.Focus, PlatformViewType);
			remote.GoTo();
			bool isFocused = System.Convert.ToBoolean( App.Query("FocusStateLabel")[0].ReadText());
			Assert.IsFalse(isFocused);
			remote.TapView();
			isFocused = System.Convert.ToBoolean(App.Query("FocusStateLabel")[0].ReadText());
			Assert.IsTrue(isFocused);
			App.Tap("FocusStateLabel");
			isFocused = System.Convert.ToBoolean(App.Query("FocusStateLabel")[0].ReadText());
			Assert.IsFalse(isFocused);
		}

		[UiTestExempt(ExemptReason.CannotTest, "Invalid interaction")]
		public override void _GestureRecognizers()
		{
		}

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
		[Category(UITestCategories.UwpIgnore)]
		public virtual void Completed()
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