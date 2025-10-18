using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class TableSectionTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var section = new TableSection("Title");
			Assert.Equal("Title", section.Title);
			Assert.Empty(section);
		}

		[Fact]
		public void IsReadOnly()
		{
			var section = new TableSection() as ICollection<Cell>;
			Assert.False(section.IsReadOnly);
		}

		[Fact]
		public void Add()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			Assert.Contains(first, section);
			Assert.Contains(second, section);
		}

		[Fact]
		public void Remove()
		{
			var section = new TableSection();
			TextCell first;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(new TextCell { Text = "Text" });

			var result = section.Remove(first);
			Assert.True(result);
			Assert.DoesNotContain(first, section);
		}

		[Fact]
		public void Clear()
		{
			var section = new TableSection { new TextCell { Text = "Text" }, new TextCell { Text = "Text" } };
			section.Clear();
			Assert.Empty(section);
		}

		[Fact]
		public void Contains()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			Assert.Contains(first, section);
			Assert.Contains(second, section);
		}

		[Fact]
		public void IndexOf()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			Assert.Equal(0, section.IndexOf(first));
			Assert.Equal(1, section.IndexOf(second));
		}

		[Fact]
		public void Insert()
		{
			var section = new TableSection();
			section.Add(new TextCell { Text = "Text" });
			section.Add(new TextCell { Text = "Text" });

			var third = new TextCell { Text = "Text" };
			section.Insert(1, third);
			Assert.Equal(third, section[1]);
		}

		[Fact]
		public void RemoveAt()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			section.RemoveAt(0);
			Assert.DoesNotContain(first, section);
		}

		[Fact]
		public void Overwrite()
		{
			var section = new TableSection();
			TextCell second;
			section.Add(new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			var third = new TextCell { Text = "Text" };
			section[1] = third;

			Assert.Equal(third, section[1]);
			Assert.DoesNotContain(second, section);
		}

		[Fact]
		public void CopyTo()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			Cell[] cells = new Cell[2];
			section.CopyTo(cells, 0);

			Assert.Equal(first, cells[0]);
			Assert.Equal(second, cells[1]);
		}

		[Fact]
		public void ChainsBindingContextOnSet()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			var bindingContext = "bindingContext";

			section.BindingContext = bindingContext;

			Assert.Equal(bindingContext, first.BindingContext);
			Assert.Equal(bindingContext, second.BindingContext);
		}

		[Fact]
		public void ChainsBindingContextWithExistingContext()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			var bindingContext = "bindingContext";
			section.BindingContext = bindingContext;

			bindingContext = "newContext";
			section.BindingContext = bindingContext;

			Assert.Equal(bindingContext, first.BindingContext);
			Assert.Equal(bindingContext, second.BindingContext);
		}

		[Fact]
		public void ChainsBindingContextToNewlyAdded()
		{
			var section = new TableSection();
			var bindingContext = "bindingContext";
			section.BindingContext = bindingContext;

			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			Assert.Equal(bindingContext, first.BindingContext);
			Assert.Equal(bindingContext, second.BindingContext);
		}

		[Fact]
		public void TestBindingTitleSectionChange()
		{
			var vm = new MockViewModel { Text = "FooBar" };
			var section = new TableSection();

			section.BindingContext = vm;
			section.SetBinding(TableSectionBase.TitleProperty, "Text");

			Assert.Equal("FooBar", section.Title);

			vm.Text = "Baz";

			Assert.Equal("Baz", section.Title);
		}

		[Fact]
		public void TestBindingTitle()
		{
			var section = new TableSection();
			var mock = new MockViewModel();
			section.BindingContext = mock;
			section.SetBinding(TableSection.TitleProperty, new Binding("Text"));

			Assert.Equal(mock.Text, section.Title);
		}
	}
}
