using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class DataTemplate : ContentPage
	{
		public DataTemplate()
		{
			InitializeComponent();
		}

		public DataTemplate(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		public class Tests
		{
			[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void EmptyTextCell(bool useCompiledXaml)
			{
				var layout = new DataTemplate(useCompiledXaml);

				var cell0 = layout.emptyTextCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.That(cell0, Is.TypeOf<TextCell>());

				var cell1 = layout.emptyTextCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.That(cell1, Is.TypeOf<TextCell>());

				Assert.AreNotSame(cell0, cell1);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void TextCell(bool useCompiledXaml)
			{
				var layout = new DataTemplate(useCompiledXaml);
				var cell0 = layout.textCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.That(cell0, Is.TypeOf<TextCell>());
				((TextCell)cell0).BindingContext = "Foo";
				Assert.Equal("Foo", ((TextCell)cell0).Text);

				var cell1 = layout.textCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.That(cell1, Is.TypeOf<TextCell>());
				((TextCell)cell1).BindingContext = "Bar";
				Assert.Equal("Bar", ((TextCell)cell1).Text);

				Assert.AreNotSame(cell0, cell1);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void FromResource(bool useCompiledXaml)
			{
				var layout = new DataTemplate(useCompiledXaml);
				var cell0 = layout.fromResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.That(cell0, Is.TypeOf<TextCell>());
				((TextCell)cell0).BindingContext = "Foo";
				Assert.Equal("Foo", ((TextCell)cell0).Text);

				var cell1 = layout.fromResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.That(cell1, Is.TypeOf<TextCell>());
				((TextCell)cell1).BindingContext = "Bar";
				Assert.Equal("Bar", ((TextCell)cell1).Text);

				Assert.AreNotSame(cell0, cell1);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void TextCellAccessResources(bool useCompiledXaml)
			{
				var layout = new DataTemplate(useCompiledXaml);
				var cell0 = layout.textCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.That(cell0, Is.TypeOf<TextCell>());
				((TextCell)cell0).BindingContext = "Foo";
				Assert.Equal("ooF", ((TextCell)cell0).Text);

				var cell1 = layout.textCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.That(cell1, Is.TypeOf<TextCell>());
				((TextCell)cell1).BindingContext = "Bar";
				Assert.Equal("raB", ((TextCell)cell1).Text);

				Assert.AreNotSame(cell0, cell1);
			}

			[Theory]
			[InlineData(false)]
			[Theory]
			[InlineData(true)]
			public void ViewCellAccessResources(bool useCompiledXaml)
			{
				var layout = new DataTemplate(useCompiledXaml);
				var cell0 = layout.viewCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.That(cell0, Is.TypeOf<ViewCell>());
				((ViewCell)cell0).BindingContext = "Foo";
				Assert.Equal("ooF", ((Label)((ViewCell)cell0).View).Text);

				var cell1 = layout.viewCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.That(cell1, Is.TypeOf<ViewCell>());
				((ViewCell)cell1).BindingContext = "Bar";
				Assert.Equal("raB", ((Label)((ViewCell)cell1).View).Text);

				Assert.AreNotSame(cell0, cell1);
			}
		}
	}
}