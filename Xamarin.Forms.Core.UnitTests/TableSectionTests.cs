using System.Collections.Generic;
using NUnit.Framework;

using NContains = NUnit.Framework.Contains;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class TableSectionTests : BaseTestFixture
	{
		[SetUp]
		public void Setup()
		{
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public void TearDown()
		{
			Device.PlatformServices = null;
		}

		[Test]
		public void Constructor()
		{
			var section = new TableSection("Title");
			Assert.AreEqual("Title", section.Title);
			Assert.That(section, Is.Empty);
		}

		[Test]
		public void IsReadOnly()
		{
			var section = new TableSection() as ICollection<Cell>;
			Assert.False(section.IsReadOnly);
		}

		[Test]
		public void Add()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			Assert.That(section, NContains.Item(first));
			Assert.That(section, NContains.Item(second));
		}

		[Test]
		public void Remove()
		{
			var section = new TableSection();
			TextCell first;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(new TextCell { Text = "Text" });

			var result = section.Remove(first);
			Assert.True(result);
			Assert.That(section, Has.No.Contains(first));
		}

		[Test]
		public void Clear()
		{
			var section = new TableSection { new TextCell { Text = "Text" }, new TextCell { Text = "Text" } };
			section.Clear();
			Assert.That(section, Is.Empty);
		}

		[Test]
		public void Contains()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			Assert.True(section.Contains(first));
			Assert.True(section.Contains(second));
		}

		[Test]
		public void IndexOf()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			Assert.AreEqual(0, section.IndexOf(first));
			Assert.AreEqual(1, section.IndexOf(second));
		}

		[Test]
		public void Insert()
		{
			var section = new TableSection();
			section.Add(new TextCell { Text = "Text" });
			section.Add(new TextCell { Text = "Text" });

			var third = new TextCell { Text = "Text" };
			section.Insert(1, third);
			Assert.AreEqual(third, section[1]);
		}

		[Test]
		public void RemoveAt()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			section.RemoveAt(0);
			Assert.That(section, Has.No.Contains(first));
		}

		[Test]
		public void Overwrite()
		{
			var section = new TableSection();
			TextCell second;
			section.Add(new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			var third = new TextCell { Text = "Text" };
			section[1] = third;

			Assert.AreEqual(third, section[1]);
			Assert.That(section, Has.No.Contains(second));
		}

		[Test]
		public void CopyTo()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			Cell[] cells = new Cell[2];
			section.CopyTo(cells, 0);

			Assert.AreEqual(first, cells[0]);
			Assert.AreEqual(second, cells[1]);
		}

		[Test]
		public void ChainsBindingContextOnSet()
		{
			var section = new TableSection();
			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			var bindingContext = "bindingContext";

			section.BindingContext = bindingContext;

			Assert.AreEqual(bindingContext, first.BindingContext);
			Assert.AreEqual(bindingContext, second.BindingContext);
		}

		[Test]
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

			Assert.AreEqual(bindingContext, first.BindingContext);
			Assert.AreEqual(bindingContext, second.BindingContext);
		}

		[Test]
		public void ChainsBindingContextToNewlyAdded()
		{
			var section = new TableSection();
			var bindingContext = "bindingContext";
			section.BindingContext = bindingContext;

			TextCell first, second;
			section.Add(first = new TextCell { Text = "Text" });
			section.Add(second = new TextCell { Text = "Text" });

			Assert.AreEqual(bindingContext, first.BindingContext);
			Assert.AreEqual(bindingContext, second.BindingContext);
		}

		[Test]
		public void TestBindingTitleSectionChange()
		{
			var vm = new MockViewModel { Text = "FooBar" };
			var section = new TableSection();

			section.BindingContext = vm;
			section.SetBinding(TableSectionBase.TitleProperty, "Text");

			Assert.AreEqual("FooBar", section.Title);

			vm.Text = "Baz";

			Assert.AreEqual("Baz", section.Title);
		}

		[Test]
		public void TestBindingTitle()
		{
			var section = new TableSection();
			var mock = new MockViewModel();
			section.BindingContext = mock;
			section.SetBinding(TableSection.TitleProperty, new Binding("Text"));

			Assert.AreEqual(mock.Text, section.Title);
		}
	}
}