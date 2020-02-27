using NUnit.Framework;
using Xamarin.Forms.Markup.RightToLeft;

namespace Xamarin.Forms.Markup.UnitTests
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class ViewExtensionsRightToLeftTests : MarkupBaseTestFixture<BoxView>
	{
		public ViewExtensionsRightToLeftTests(bool withExperimentalFlag) : base(withExperimentalFlag) { }

		[Test]
		public void Left()
			=> TestPropertiesSet(v => v.Left(), (View.HorizontalOptionsProperty, LayoutOptions.Start, LayoutOptions.End));

		[Test]
		public void Right()
			=> TestPropertiesSet(v => v.Right(), (View.HorizontalOptionsProperty, LayoutOptions.End, LayoutOptions.Start));

		[Test]
		public void LeftExpand()
			=> TestPropertiesSet(v => v.LeftExpand(), (View.HorizontalOptionsProperty, LayoutOptions.Start, LayoutOptions.EndAndExpand));

		[Test]
		public void RightExpand()
			=> TestPropertiesSet(v => v.RightExpand(), (View.HorizontalOptionsProperty, LayoutOptions.End, LayoutOptions.StartAndExpand));

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