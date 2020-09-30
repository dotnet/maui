using NUnit.Framework;

namespace Xamarin.Forms.Markup.UnitTests
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class ViewInFlexLayoutExtensionsTests : MarkupBaseTestFixture<BoxView>
	{
		public ViewInFlexLayoutExtensionsTests(bool withExperimentalFlag) : base(withExperimentalFlag) { }

		[Test]
		public void AlignSelf() => AssertExperimental(() =>
		{
			FlexLayout.SetAlignSelf(Bindable, FlexAlignSelf.End);
			Bindable.AlignSelf(FlexAlignSelf.Start);
			Assert.That(FlexLayout.GetAlignSelf(Bindable), Is.EqualTo(FlexAlignSelf.Start));
		});

		[Test]
		public void Basis() => AssertExperimental(() =>
		{
			FlexLayout.SetBasis(Bindable, FlexBasis.Auto);
			Bindable.Basis(50);
			Assert.That(FlexLayout.GetBasis(Bindable), Is.EqualTo(new FlexBasis(50)));
		});

		[Test]
		public void Grow() => AssertExperimental(() =>
		{
			FlexLayout.SetGrow(Bindable, 0f);
			Bindable.Grow(1f);
			Assert.That(FlexLayout.GetGrow(Bindable), Is.EqualTo(1f));
		});

		[Test]
		public void Order() => AssertExperimental(() =>
		{
			FlexLayout.SetOrder(Bindable, 0);
			Bindable.Order(1);
			Assert.That(FlexLayout.GetOrder(Bindable), Is.EqualTo(1));
		});

		[Test]
		public void Shrink() => AssertExperimental(() =>
		{
			FlexLayout.SetShrink(Bindable, 1f);
			Bindable.Shrink(0f);
			Assert.That(FlexLayout.GetShrink(Bindable), Is.EqualTo(0f));
		});
	}
}