using NUnit.Framework;

namespace Xamarin.Forms.Markup.UnitTests
{
	[TestFixture(true)]
	[TestFixture(false)]
	public class ViewInGridExtensionsTests : MarkupBaseTestFixture<BoxView>
	{
		enum TestRow { First, Second }
		enum TestColumn { First, Second }

		public ViewInGridExtensionsTests(bool withExperimentalFlag) : base(withExperimentalFlag) { }

		[Test]
		public void Row()
			=> TestPropertiesSet(b => b.Row(1), (Grid.RowProperty, 0, 1));

		[Test]
		public void RowWithSpan()
			=> TestPropertiesSet(
					b => b.Row(1, 2),
					(Grid.RowProperty, 0, 1),
					(Grid.RowSpanProperty, 1, 2));

		[Test]
		public void RowSpan()
			=> TestPropertiesSet(b => b.RowSpan(2), (Grid.RowSpanProperty, 1, 2));

		[Test]
		public void Column()
			=> TestPropertiesSet(b => b.Column(1), (Grid.ColumnProperty, 0, 1));

		[Test]
		public void ColumnWithSpan()
			=> TestPropertiesSet(
					b => b.Column(1, 2),
					(Grid.ColumnProperty, 0, 1),
					(Grid.ColumnSpanProperty, 1, 2));

		[Test]
		public void ColumnSpan()
			=> TestPropertiesSet(b => b.ColumnSpan(2), (Grid.ColumnSpanProperty, 1, 2));

		[Test]
		public void RowEnum()
			=> TestPropertiesSet(b => b.Row(TestRow.Second), (Grid.RowProperty, (int)TestRow.First, (int)TestRow.Second));

		[Test]
		public void RowWithLastRowEnum()
			=> TestPropertiesSet(
					b => b.Row(TestRow.First, TestRow.Second),
					(Grid.RowProperty, (int)TestRow.Second, (int)TestRow.First),
					(Grid.RowSpanProperty, 1, 2));

		[Test]
		public void ColumnEnum()
			=> TestPropertiesSet(b => b.Column(TestColumn.Second), (Grid.ColumnProperty, (int)TestColumn.First, (int)TestColumn.Second));

		[Test]
		public void ColumnWithLastColumnEnum()
			=> TestPropertiesSet(
					b => b.Column(TestColumn.First, TestColumn.Second),
					(Grid.ColumnProperty, (int)TestColumn.Second, (int)TestColumn.First),
					(Grid.ColumnSpanProperty, 1, 2));
	}
}