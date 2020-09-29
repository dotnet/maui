using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	internal class BindingSystemTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		class BindableViewCell : ViewCell
		{
			public static readonly BindableProperty NameProperty =
				BindableProperty.Create<BindableViewCell, string>(w => w.Name, "");

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

		[Test]
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

			Assert.AreEqual("Test1", viewCell1.Name);
			Assert.AreEqual("Test2", viewCell2.Name);

			Assert.AreEqual("Test1", viewCell1.NameLabel.Text);
			Assert.AreEqual("Test2", viewCell2.NameLabel.Text);
		}
	}
}