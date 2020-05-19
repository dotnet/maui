using NUnit.Framework;
using System.Maui.Markup.RightToLeft;

namespace System.Maui.Markup.UnitTests
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class LabelExtensionsRightToLeftTests : MarkupBaseTestFixture<Label>
	{
		public LabelExtensionsRightToLeftTests(bool withExperimentalFlag) : base(withExperimentalFlag) { }

		[Test]
		public void TextLeft()
			=> TestPropertiesSet(l => l.TextLeft(), (Label.HorizontalTextAlignmentProperty, TextAlignment.Start, TextAlignment.End));

		[Test]
		public void TextRight()
			=> TestPropertiesSet(l => l.TextRight(), (Label.HorizontalTextAlignmentProperty, TextAlignment.End, TextAlignment.Start));

		[Test]
		public void SupportDerivedFromLabel() => AssertExperimental(() =>
		{
			DerivedFromLabel _ =
				new DerivedFromLabel()
				.TextLeft()
				.TextRight();
		});

		class DerivedFromLabel : Label { }
	}
}