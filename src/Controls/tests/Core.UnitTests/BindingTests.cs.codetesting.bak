using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class BindingSystemTests : BaseTestFixture
	{
		class BindableViewCell : ViewCell
		{
			public static readonly BindableProperty NameProperty =
				BindableProperty.Create("Name", typeof(string), typeof(BindableViewCell));

			public Label NameLabel { get; set; }

			public string Name
			{
				get { return (string)GetValue(NameProperty); }
				set { SetValue(NameProperty, value); }
			}

			public BindableViewCell()
			{
				NameLabel = new Label { BindingContext = this };
				NameLabel.SetBinding(Label.TextProperty, new Binding("Name"));
				View = NameLabel;
			}
		}

		[Fact]
		public void RecursiveSettingInSystem()
		{
			var tempObjects = new[] {
				new {Name = "Test1"},
				new {Name = "Test2"}
			};

			var template = new DataTemplate(typeof(BindableViewCell))
			{
				Bindings = { { BindableViewCell.NameProperty, new Binding("Name") } }
			};

			var cell1 = (Cell)template.CreateContent();
			cell1.BindingContext = tempObjects[0];
			cell1.Parent = new ListView();

			var cell2 = (Cell)template.CreateContent();
			cell2.BindingContext = tempObjects[1];
			cell2.Parent = new ListView();

			var viewCell1 = (BindableViewCell)cell1;
			var viewCell2 = (BindableViewCell)cell2;

			Assert.Equal("Test1", viewCell1.Name);
			Assert.Equal("Test2", viewCell2.Name);

			Assert.Equal("Test1", viewCell1.NameLabel.Text);
			Assert.Equal("Test2", viewCell2.NameLabel.Text);
		}
	}
}