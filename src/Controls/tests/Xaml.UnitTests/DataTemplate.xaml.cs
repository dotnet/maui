using System;
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


		public class Tests
		{
			// TODO: Convert to IDisposable or constructor - [MemberData(nameof(InitializeTest))] // TODO: Convert to IDisposable or constructor public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			[Theory]
			[Values]
			public void EmptyTextCell(XamlInflator inflator)
			{
				var layout = new DataTemplate(inflator);

				var cell0 = layout.emptyTextCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.IsType<TextCell>(cell0);

				var cell1 = layout.emptyTextCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.IsType<TextCell>(cell1);

				Assert.NotSame(cell0, cell1);
			}

			[Theory]
			[Values]
			public void TextCell(XamlInflator inflator)
			{
				var layout = new DataTemplate(inflator);
				var cell0 = layout.textCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.IsType<TextCell>(cell0);
				((TextCell)cell0).BindingContext = "Foo";
				Assert.Equal("Foo", ((TextCell)cell0).Text);

				var cell1 = layout.textCell.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.IsType<TextCell>(cell1);
				((TextCell)cell1).BindingContext = "Bar";
				Assert.Equal("Bar", ((TextCell)cell1).Text);

				Assert.NotSame(cell0, cell1);
			}

			[Theory]
			[Values]
			public void FromResource(XamlInflator inflator)
			{
				var layout = new DataTemplate(inflator);
				var cell0 = layout.fromResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.IsType<TextCell>(cell0);
				((TextCell)cell0).BindingContext = "Foo";
				Assert.Equal("Foo", ((TextCell)cell0).Text);

				var cell1 = layout.fromResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.IsType<TextCell>(cell1);
				((TextCell)cell1).BindingContext = "Bar";
				Assert.Equal("Bar", ((TextCell)cell1).Text);

				Assert.NotSame(cell0, cell1);
			}

			[Theory]
			[Values]
			public void TextCellAccessResources(XamlInflator inflator)
			{
				var layout = new DataTemplate(inflator);
				var cell0 = layout.textCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.IsType<TextCell>(cell0);
				((TextCell)cell0).BindingContext = "Foo";
				Assert.Equal("ooF", ((TextCell)cell0).Text);

				var cell1 = layout.textCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.IsType<TextCell>(cell1);
				((TextCell)cell1).BindingContext = "Bar";
				Assert.Equal("raB", ((TextCell)cell1).Text);

				Assert.NotSame(cell0, cell1);
			}

			[Theory]
			[Values]
			public void ViewCellAccessResources(XamlInflator inflator)
			{
				var layout = new DataTemplate(inflator);
				var cell0 = layout.viewCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell0);
				Assert.IsType<ViewCell>(cell0);
				((ViewCell)cell0).BindingContext = "Foo";
				Assert.Equal("ooF", ((Label)((ViewCell)cell0).View).Text);

				var cell1 = layout.viewCellAccessResource.ItemTemplate.CreateContent();
				Assert.NotNull(cell1);
				Assert.IsType<ViewCell>(cell1);
				((ViewCell)cell1).BindingContext = "Bar";
				Assert.Equal("raB", ((Label)((ViewCell)cell1).View).Text);

				Assert.NotSame(cell0, cell1);
			}
		}
	}
}