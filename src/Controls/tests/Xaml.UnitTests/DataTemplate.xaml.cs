using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class DataTemplate : ContentPage
	{
		public DataTemplate()
		{
			InitializeComponent();
		}

		[TestFixture]
		class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Test]
			public void EmptyTextCell([Values] XamlInflator inflator)
			{
				var layout = new DataTemplate(inflator);

				var cell0 = layout.emptyTextCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.That(cell0, Is.TypeOf<TextCell>());

				var cell1 = layout.emptyTextCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.That(cell1, Is.TypeOf<TextCell>());

				Assert.AreNotSame(cell0, cell1);
			}

			[Test]
			public void TextCell([Values] XamlInflator inflator)
			{
				var layout = new DataTemplate(inflator);
				var cell0 = layout.textCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.That(cell0, Is.TypeOf<TextCell>());
				((TextCell)cell0).BindingContext = "Foo";
				Assert.AreEqual("Foo", ((TextCell)cell0).Text);

				var cell1 = layout.textCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.That(cell1, Is.TypeOf<TextCell>());
				((TextCell)cell1).BindingContext = "Bar";
				Assert.AreEqual("Bar", ((TextCell)cell1).Text);

				Assert.AreNotSame(cell0, cell1);
			}

			[Test]
			public void FromResource([Values] XamlInflator inflator)
			{
				var layout = new DataTemplate(inflator);
				var cell0 = layout.fromResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.That(cell0, Is.TypeOf<TextCell>());
				((TextCell)cell0).BindingContext = "Foo";
				Assert.AreEqual("Foo", ((TextCell)cell0).Text);

				var cell1 = layout.fromResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.That(cell1, Is.TypeOf<TextCell>());
				((TextCell)cell1).BindingContext = "Bar";
				Assert.AreEqual("Bar", ((TextCell)cell1).Text);

				Assert.AreNotSame(cell0, cell1);
			}

			[Test]
			public void TextCellAccessResources([Values] XamlInflator inflator)
			{
				var layout = new DataTemplate(inflator);
				var cell0 = layout.textCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.That(cell0, Is.TypeOf<TextCell>());
				((TextCell)cell0).BindingContext = "Foo";
				Assert.AreEqual("ooF", ((TextCell)cell0).Text);

				var cell1 = layout.textCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.That(cell1, Is.TypeOf<TextCell>());
				((TextCell)cell1).BindingContext = "Bar";
				Assert.AreEqual("raB", ((TextCell)cell1).Text);

				Assert.AreNotSame(cell0, cell1);
			}

			[Test]
			public void ViewCellAccessResources([Values] XamlInflator inflator)
			{
				var layout = new DataTemplate(inflator);
				var cell0 = layout.viewCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.That(cell0, Is.TypeOf<ViewCell>());
				((ViewCell)cell0).BindingContext = "Foo";
				Assert.AreEqual("ooF", ((Label)((ViewCell)cell0).View).Text);

				var cell1 = layout.viewCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.That(cell1, Is.TypeOf<ViewCell>());
				((ViewCell)cell1).BindingContext = "Bar";
				Assert.AreEqual("raB", ((Label)((ViewCell)cell1).View).Text);

				Assert.AreNotSame(cell0, cell1);
			}
		}
	}
}