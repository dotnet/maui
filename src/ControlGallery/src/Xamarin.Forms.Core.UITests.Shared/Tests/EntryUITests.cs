using NUnit.Framework;
using Xamarin.Forms.Controls.Issues;
using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

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

			Assert.IsFalse(IsFocused());
			remote.TapView();
			Assert.IsTrue(IsFocused());
			App.Tap("Go"); // Won't do anything, we just need to take focus away from the Entry
			Assert.IsFalse(IsFocused());
		}

		bool IsFocused()
		{
			var focusedText = App.QueryUntilPresent(() =>
			{
				return App.Query(q => q.Marked("FocusStateLabel").All());
			})[0].ReadText();

			return System.Convert.ToBoolean(focusedText);
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

		[Test]
		[UiTest(typeof(Entry), "ClearButtonVisibility")]
		[Category(UITestCategories.ManualReview)]
		public void ClearButtonVisibility()
		{
			var remote = new StateViewContainerRemote(App, Test.Entry.ClearButtonVisibility, PlatformViewType);
			remote.GoTo();

			App.WaitForElement(q => q.Marked("Toggle ClearButtonVisibility"));
			App.Tap(q => q.Marked("Toggle ClearButtonVisibility"));
		}

		protected override void FixtureTeardown()
		{
			App.NavigateBack();
			base.FixtureTeardown();
		}

#if __ANDROID__ || __IOS__
		[Ignore("This is covered by the platform opacity tests")]
		public override void _Opacity() { }
#endif

#if __ANDROID__ || __IOS__ || __WINDOWS__
		[Ignore("This is covered by the platform tests")]
		public override void _IsEnabled() { }
#endif

#if __ANDROID__ || __IOS__ || __WINDOWS__
		[Ignore("This is covered by the platform tests")]
		public override void _Rotation() { }

		[Ignore("This is covered by the platform tests")]
		public override void _RotationX() { }

		[Ignore("This is covered by the platform tests")]
		public override void _RotationY() { }
#endif

#if __ANDROID__
		[Ignore("This is covered by the platform tests")]
		public override void _TranslationX() { }

		[Ignore("This is covered by the platform tests")]
		public override void _TranslationY() { }
#endif

#if __IOS__ || __WINDOWS__
		[Ignore("This is covered by the platform tests")]
		public override void _Scale() { }
#endif

#if __ANDROID__ || __IOS__
		[Ignore("This is covered by the platform tests")]
		public override void _IsVisible() { }
#endif
	}
}