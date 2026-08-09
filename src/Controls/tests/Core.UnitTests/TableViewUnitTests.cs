using System;
using System.Linq;
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
		static WeakReference AssignSharedRootAndDrop(TableRoot sharedRoot, bool useConstructor)
		{
			var table = useConstructor
				? new TableView(sharedRoot)
				: new TableView { Root = sharedRoot };

			return new WeakReference(table);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference ClearRootAndDrop(TableSection section)
		{
			var root = new TableRoot { section };
			root.Clear();

			return new WeakReference(root);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference AssignSharedSectionAndDrop(TableSection sharedSection)
		{
			var root = new TableRoot { sharedSection };
			return new WeakReference(root);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task RootedTableRootDoesNotLeakTableView(bool useConstructor)
		{
			// A long-lived / shared TableRoot, e.g. one reused across pages.
			var sharedRoot = new TableRoot
			{
				new TableSection("Section")
				{
					new TextCell { Text = "Cell" }
				}
			};

			var tableReference = AssignSharedRootAndDrop(sharedRoot, useConstructor);

			Assert.False(await tableReference.WaitForCollect(), "TableView should not be alive!");

			// Keep the shared root alive for the whole test so the only question is
			// whether it still roots the (dropped) TableView.
			GC.KeepAlive(sharedRoot);
		}

		[Fact]
		public async Task ClearedSectionDoesNotRetainTableRoot()
		{
			var section = new TableSection("Section");
			var rootReference = ClearRootAndDrop(section);

			Assert.False(await rootReference.WaitForCollect(), "TableRoot should not be alive!");
			GC.KeepAlive(section);
		}

		[Fact]
		public async Task RootedTableSectionDoesNotLeakTableRoot()
		{
			var sharedSection = new TableSection("Section");
			var rootReference = AssignSharedSectionAndDrop(sharedSection);

			Assert.False(await rootReference.WaitForCollect(), "TableRoot should not be alive!");
			GC.KeepAlive(sharedSection);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task RootChangesStillRaiseModelChangedAfterGc(bool useConstructor)
		{
			var section = new TableSection("Section");
			var root = new TableRoot { section };
			var table = useConstructor
				? new TableView(root)
				: new TableView { Root = root };
			int modelChangedCount = 0;
			table.ModelChanged += (sender, e) => modelChangedCount++;

			await TestHelpers.Collect();
			root.Title = "New title";
			root.Add(new TableSection("Added section"));
			section.Add(new TextCell { Text = "Added cell" });
			section.Title = "New section title";

			Assert.Equal(4, modelChangedCount);
			GC.KeepAlive(table);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void RootReplacementMovesEventSubscriptions(bool useConstructor)
		{
			var oldSection = new TableSection("Old section");
			var oldRoot = new TableRoot { oldSection };
			var newSection = new TableSection("New section");
			var newRoot = new TableRoot { newSection };
			var table = useConstructor
				? new TableView(oldRoot)
				: new TableView { Root = oldRoot };
			table.Root = newRoot;
			int modelChangedCount = 0;
			table.ModelChanged += (sender, e) => modelChangedCount++;

			oldRoot.Title = "Changed old root";
			oldRoot.Add(new TableSection("Added old section"));
			oldSection.Add(new TextCell { Text = "Added old cell" });

			Assert.Equal(0, modelChangedCount);

			newRoot.Title = "Changed new root";
			newRoot.Add(new TableSection("Added new section"));
			newSection.Add(new TextCell { Text = "Added new cell" });

			Assert.Equal(3, modelChangedCount);
		}

		[Fact]
		public void ClearMovesSectionEventSubscriptions()
		{
			var section = new TableSection("Section");
			var root = new TableRoot { section };
			var table = new TableView(root);
			int modelChangedCount = 0;
			table.ModelChanged += (sender, e) => modelChangedCount++;

			root.Clear();
			modelChangedCount = 0;

			section.Add(new TextCell { Text = "Removed section cell" });
			section.Title = "Removed section";

			Assert.Equal(0, modelChangedCount);

			root.Add(section);
			modelChangedCount = 0;

			section.Add(new TextCell { Text = "Re-added section cell" });
			section.Title = "Re-added section";

			Assert.Equal(2, modelChangedCount);
		}

		[Fact]
		public void ReplacingSectionMovesEventSubscriptions()
		{
			var oldSection = new TableSection("Old section");
			var newSection = new TableSection("New section");
			var root = new TableRoot { oldSection };
			var table = new TableView(root);
			int modelChangedCount = 0;
			table.ModelChanged += (sender, e) => modelChangedCount++;

			root[0] = newSection;
			modelChangedCount = 0;

			oldSection.Add(new TextCell { Text = "Old section cell" });
			oldSection.Title = "Changed old section";

			Assert.Equal(0, modelChangedCount);

			newSection.Add(new TextCell { Text = "New section cell" });
			newSection.Title = "Changed new section";

			Assert.Equal(2, modelChangedCount);
		}

		[Fact]
		public void DuplicateSectionsShareSubscriptionUntilLastRemoval()
		{
			var section = new TableSection("Section");
			var root = new TableRoot { section, section };
			var table = new TableView(root);
			int modelChangedCount = 0;
			table.ModelChanged += (sender, e) => modelChangedCount++;

			section.Add(new TextCell { Text = "Shared cell" });
			section.Title = "Shared section";

			Assert.Equal(2, modelChangedCount);

			root.Remove(section);
			modelChangedCount = 0;
			section.Add(new TextCell { Text = "Still shared cell" });
			section.Title = "Still shared section";

			Assert.Equal(2, modelChangedCount);

			root.Remove(section);
			modelChangedCount = 0;
			section.Add(new TextCell { Text = "Detached cell" });
			section.Title = "Detached section";

			Assert.Equal(0, modelChangedCount);
		}
	}
}
