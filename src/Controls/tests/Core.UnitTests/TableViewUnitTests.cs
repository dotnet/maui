using System.Linq;

using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TableViewUnitTests : BaseTestFixture
	{
		[Fact]
		public void TestConstructor()
		{
			var table = new TableView();

			Assert.False(table.Root.Any());
			Assert.Equal(LayoutOptions.FillAndExpand, table.HorizontalOptions);
			Assert.Equal(LayoutOptions.FillAndExpand, table.VerticalOptions);
		}

		[Fact]
		public void TestModelChanged()
		{
			var table = new TableView();

			bool changed = false;

			table.ModelChanged += (sender, e) => changed = true;

			table.Root = new TableRoot("NewRoot");

			Assert.True(changed);
		}

		[Fact]
		public void BindingsContextChainsToModel()
		{
			const string context = "Context";
			var table = new TableView { BindingContext = context, Root = new TableRoot() };

			Assert.Equal(context, table.Root.BindingContext);

			// reverse assignment order
			table = new TableView { Root = new TableRoot(), BindingContext = context };
			Assert.Equal(context, table.Root.BindingContext);
		}

		[Fact]
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

			Assert.Equal(table, viewCell.Parent);
			Assert.Equal(viewCell, viewCell.View.Parent);
		}

		[Fact]
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

			Assert.Equal(table, viewCell.Parent);
			Assert.Equal(viewCell, viewCell.View.Parent);
		}
	}
}
