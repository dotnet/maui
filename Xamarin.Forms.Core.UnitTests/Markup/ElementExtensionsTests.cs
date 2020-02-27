using NUnit.Framework;

namespace Xamarin.Forms.Markup.UnitTests
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class ElementExtensionsTests : MarkupBaseTestFixture<Label>
	{
		public ElementExtensionsTests(bool withExperimentalFlag) : base(withExperimentalFlag) { }

		Label Label => Bindable;

		[Test]
		public void EffectSingle() => AssertExperimental(() =>
		{
			Label.Effects?.Clear();
			Assume.That((Label.Effects?.Count ?? 0), Is.EqualTo(0));

			NullEffect effect1 = new NullEffect();
			Label.Effects(effect1);

			Assert.That((Label.Effects?.Count ?? 0), Is.EqualTo(1));
			Assert.That(Label.Effects.Contains(effect1));
		});

		[Test]
		public void EffectsMultiple() => AssertExperimental(() =>
		{
			Label.Effects?.Clear();
			Assume.That((Label.Effects?.Count ?? 0), Is.EqualTo(0));

			NullEffect effect1 = new NullEffect(), effect2 = new NullEffect();
			Label.Effects(effect1, effect2);

			Assert.That((Label.Effects?.Count ?? 0), Is.EqualTo(2));
			Assert.That(Label.Effects.Contains(effect1));
			Assert.That(Label.Effects.Contains(effect2));
		});

		[Test]
		public void FontSize()
			=> TestPropertiesSet(l => l.FontSize(8), (FontElement.FontSizeProperty, 6.0, 8.0));

		[Test]
		public void Bold()
			=> TestPropertiesSet(l => l.Bold(), (FontElement.FontAttributesProperty, FontAttributes.None, FontAttributes.Bold));

		[Test]
		public void Italic()
			=> TestPropertiesSet(l => l.Italic(), (FontElement.FontAttributesProperty, FontAttributes.None, FontAttributes.Italic));

		[Test]
		public void FontWithPositionalParameters()
			=> TestPropertiesSet(
					l => l.Font(8, true, true, "AFontName"),
					(FontElement.FontSizeProperty, 6.0, 8.0),
					(FontElement.FontAttributesProperty, FontAttributes.None, FontAttributes.Bold | FontAttributes.Italic),
					(FontElement.FontFamilyProperty, "", "AFontName"));

		[Test]
		public void FontWithSizeNamedParameter()
			=> TestPropertiesSet(l => l.Font(size: 8), (FontElement.FontSizeProperty, 6.0, 8.0));

		[Test]
		public void FontWithBoldNamedParameter()
			=> TestPropertiesSet(l => l.Font(bold: true), (FontElement.FontAttributesProperty, FontAttributes.None, FontAttributes.Bold));

		[Test]
		public void FontWithItalicNamedParameter()
			=> TestPropertiesSet(l => l.Font(italic: true), (FontElement.FontAttributesProperty, FontAttributes.None, FontAttributes.Italic));

		[Test]
		public void FontWithFamilyNamedParameter()
			=> TestPropertiesSet(l => l.Font(family: "AFontName"), (FontElement.FontFamilyProperty, "", "AFontName"));

		[Test]
		public void SupportDerivedFromLabel() => AssertExperimental(() =>
		{
			DerivedFromLabel _ =
				new DerivedFromLabel()
				.Effects(new NullEffect())
				.FontSize(8)
				.Bold()
				.Italic()
				.Font(8, true, true, "AFontName");
		});

		class DerivedFromLabel : Label { }
	}
}