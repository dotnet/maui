using System.Linq;

using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TableViewUnitTests : BaseTestFixture
	{
		[Test]
		public void TestConstructor()
		{
			var table = new TableView();

			Assert.False(table.Root.Any());
			Assert.AreEqual(LayoutOptions.FillAndExpand, table.HorizontalOptions);
			Assert.AreEqual(LayoutOptions.FillAndExpand, table.VerticalOptions);
		}

		[Test]
		public void TestModelChanged()
		{
			var table = new TableView();

			bool changed = false;

			table.ModelChanged += (sender, e) => changed = true;

			table.Root = new TableRoot("NewRoot");

			Assert.True(changed);
		}

		[Test]
		public void BindingsContextChainsToModel()
		{
			const string context = "Context";
			var table = new TableView { BindingContext = context, Root = new TableRoot() };

			Assert.AreEqual(context, table.Root.BindingContext);

			// reverse assignment order
			table = new TableView { Root = new TableRoot(), BindingContext = context };
			Assert.AreEqual(context, table.Root.BindingContext);
		}

		[Test]
		public void ParentsViewCells()
		{
			ViewCell viewCell = new ViewCell { View = new Label() };
			var table = new TableView
			{
				Root = new TableRoot {
					new TableSection {
						viewCell
					}
				}
			};

			Assert.AreEqual(table, viewCell.Parent);
			Assert.AreEqual(viewCell, viewCell.View.Parent);
		}

		[Test]
		public void ParentsAddedViewCells()
		{
			var viewCell = new ViewCell { View = new Label() };
			var section = new TableSection();
			var table = new TableView
			{
				Root = new TableRoot {
					section
				}
			};

			section.Add(viewCell);

			Assert.AreEqual(table, viewCell.Parent);
			Assert.AreEqual(viewCell, viewCell.View.Parent);
		}
	}
}