using System;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class SwitchCellTemplateTests : BaseTestFixture
	{
		[Test]
		public void Create()
		{
			var template = new DataTemplate(typeof(SwitchCell));
			var content = template.CreateContent();

			Assert.That(content, Is.InstanceOf<SwitchCell>());
		}

		[Test]
		public void Text()
		{
			var template = new DataTemplate(typeof(SwitchCell));
			template.SetValue(SwitchCell.TextProperty, "text");

			SwitchCell cell = (SwitchCell)template.CreateContent();
			Assert.That(cell.Text, Is.EqualTo("text"));
		}

		[Test]
		public void On()
		{
			var template = new DataTemplate(typeof(SwitchCell));
			template.SetValue(SwitchCell.OnProperty, true);

			SwitchCell cell = (SwitchCell)template.CreateContent();
			Assert.That(cell.On, Is.EqualTo(true));
		}

		[TestCase(false, true)]
		[TestCase(true, false)]
		public void SwitchCellSwitchChangedArgs(bool initialValue, bool finalValue)
		{
			var template = new DataTemplate(typeof(SwitchCell));
			SwitchCell cell = (SwitchCell)template.CreateContent();

			SwitchCell switchCellFromSender = null;
			bool newSwitchValue = false;

			cell.On = initialValue;

			cell.OnChanged += (s, e) =>
			{
				switchCellFromSender = (SwitchCell)s;
				newSwitchValue = e.Value;
			};

			cell.On = finalValue;

			Assert.AreEqual(cell, switchCellFromSender);
			Assert.AreEqual(finalValue, newSwitchValue);
		}
	}
}