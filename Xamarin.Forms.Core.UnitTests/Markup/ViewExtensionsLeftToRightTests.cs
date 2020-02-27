using NUnit.Framework;
using Xamarin.Forms.Markup.LeftToRight;

namespace Xamarin.Forms.Markup.UnitTests
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class ViewExtensionsLeftToRightTests : MarkupBaseTestFixture<BoxView>
	{
		public ViewExtensionsLeftToRightTests(bool withExperimentalFlag) : base(withExperimentalFlag) { }

		[Test]
		public void Left()
			=> TestPropertiesSet(v => v.Left(), (View.HorizontalOptionsProperty, LayoutOptions.End, LayoutOptions.Start));

		[Test]
		public void Right()
			=> TestPropertiesSet(v => v.Right(), (View.HorizontalOptionsProperty, LayoutOptions.Start, LayoutOptions.End));

		[Test]
		public void LeftExpand()
			=> TestPropertiesSet(v => v.LeftExpand(), (View.HorizontalOptionsProperty, LayoutOptions.End, LayoutOptions.StartAndExpand));

		[Test]
		public void RightExpand()
			=> TestPropertiesSet(v => v.RightExpand(), (View.HorizontalOptionsProperty, LayoutOptions.End, LayoutOptions.EndAndExpand));

		[Test]
		public void SupportDerivedFromView() => AssertExperimental(() =>
		{
			DerivedFromView _ =
				new DerivedFromView()
				.Left()
				.Right()
				.LeftExpand()
				.RightExpand();
		});

		class DerivedFromView : BoxView { }
	}
}