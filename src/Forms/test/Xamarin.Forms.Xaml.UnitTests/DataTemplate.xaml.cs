using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
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
		}

		[TestFixture]
		public class Tests
		{
			[TestCase(false)]
			[TestCase(true)]
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

			[TestCase(false)]
			[TestCase(true)]
			public void TextCell(bool useCompiledXaml)
			{
				var layout = new DataTemplate(useCompiledXaml);
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

			[TestCase(false)]
			[TestCase(true)]
			public void FromResource(bool useCompiledXaml)
			{
				var layout = new DataTemplate(useCompiledXaml);
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

			[TestCase(false)]
			[TestCase(true)]
			public void TextCellAccessResources(bool useCompiledXaml)
			{
				var layout = new DataTemplate(useCompiledXaml);
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

			[TestCase(false)]
			[TestCase(true)]
			public void ViewCellAccessResources(bool useCompiledXaml)
			{
				var layout = new DataTemplate(useCompiledXaml);
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