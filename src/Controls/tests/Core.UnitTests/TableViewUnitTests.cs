using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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

		[MethodImpl(MethodImplOptions.NoInlining)]
		static (WeakReference Table, WeakReference CollectionChangedProxy, WeakReference SectionCollectionChangedProxy, WeakReference PropertyChangedProxy)
			AssignSharedRootAndDrop(TableRoot sharedRoot, bool useConstructor)
		{
			var table = useConstructor
				? new TableView(sharedRoot)
				: new TableView { Root = sharedRoot };

			var flags = BindingFlags.NonPublic | BindingFlags.Instance;
			var model = table.Model;
			var modelType = model.GetType();

			return (
				new WeakReference(table),
				new WeakReference(modelType.GetField("_collectionChangedProxy", flags).GetValue(model)),
				new WeakReference(modelType.GetField("_sectionCollectionChangedProxy", flags).GetValue(model)),
				new WeakReference(modelType.GetField("_propertyChangedProxy", flags).GetValue(model)));
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task RootedTableRootDoesNotLeakTableViewOrProxies(bool useConstructor)
		{
			// A long-lived / shared TableRoot, e.g. one reused across pages.
			var sharedRoot = new TableRoot
			{
				new TableSection("Section")
				{
					new TextCell { Text = "Cell" }
				}
			};

			var weakReferences = AssignSharedRootAndDrop(sharedRoot, useConstructor);

			Assert.False(await weakReferences.Table.WaitForCollect(), "TableView should not be alive!");
			Assert.False(await weakReferences.CollectionChangedProxy.WaitForCollect(), "WeakNotifyCollectionChangedProxy should not be alive!");
			Assert.False(await weakReferences.SectionCollectionChangedProxy.WaitForCollect(), "WeakSectionCollectionChangedProxy should not be alive!");
			Assert.False(await weakReferences.PropertyChangedProxy.WaitForCollect(), "WeakNotifyPropertyChangedProxy should not be alive!");

			// Keep the shared root alive for the whole test so the only question is
			// whether it still roots the (dropped) TableView.
			GC.KeepAlive(sharedRoot);
		}

		[Fact]
		public void ConstructorRootPropertyChangeRaisesModelChanged()
		{
			var root = new TableRoot();
			var table = new TableView(root);
			bool changed = false;
			table.ModelChanged += (sender, e) => changed = true;

			root.Title = "New title";

			Assert.True(changed);
		}
	}
}
