using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class DataTemplateTests : BaseTestFixture
	{
		[Test]
		public void CtorInvalid()
		{
			Assert.Throws<ArgumentNullException>(() => new DataTemplate((Func<object>)null),
				"Allowed null creator delegate");

			Assert.Throws<ArgumentNullException>(() => new DataTemplate((Type)null),
				"Allowed null type");
		}

		[Test]
		public void CreateContent()
		{
			var template = new DataTemplate(() => new MockBindable());
			object obj = template.CreateContent();

			Assert.IsNotNull(obj);
			Assert.That(obj, Is.InstanceOf<MockBindable>());
		}

		[Test]
		public void CreateContentType()
		{
			var template = new DataTemplate(typeof(MockBindable));
			object obj = template.CreateContent();

			Assert.IsNotNull(obj);
			Assert.That(obj, Is.InstanceOf<MockBindable>());
		}

		[Test]
		public void CreateContentValues()
		{
			var template = new DataTemplate(typeof(MockBindable))
			{
				Values = { { MockBindable.TextProperty, "value" } }
			};

			MockBindable bindable = (MockBindable)template.CreateContent();
			Assert.That(bindable.GetValue(MockBindable.TextProperty), Is.EqualTo("value"));
		}

		[Test]
		public void CreateContentBindings()
		{
			var template = new DataTemplate(() => new MockBindable())
			{
				Bindings = { { MockBindable.TextProperty, new Binding(".") } }
			};

			MockBindable bindable = (MockBindable)template.CreateContent();
			bindable.BindingContext = "text";
			Assert.That(bindable.GetValue(MockBindable.TextProperty), Is.EqualTo("text"));
		}

		[Test]
		public void SetBindingInvalid()
		{
			var template = new DataTemplate(typeof(MockBindable));
			Assert.That(() => template.SetBinding(null, new Binding(".")), Throws.InstanceOf<ArgumentNullException>());
			Assert.That(() => template.SetBinding(MockBindable.TextProperty, null), Throws.InstanceOf<ArgumentNullException>());
		}

		[Test]
		public void SetBindingOverridesValue()
		{
			var template = new DataTemplate(typeof(MockBindable));
			template.SetValue(MockBindable.TextProperty, "value");
			template.SetBinding(MockBindable.TextProperty, new Binding("."));

			MockBindable bindable = (MockBindable)template.CreateContent();
			Assume.That(bindable.GetValue(MockBindable.TextProperty), Is.EqualTo(bindable.BindingContext));

			bindable.BindingContext = "binding";
			Assert.That(bindable.GetValue(MockBindable.TextProperty), Is.EqualTo("binding"));
		}

		[Test]
		public void SetValueOverridesBinding()
		{
			var template = new DataTemplate(typeof(MockBindable));
			template.SetBinding(MockBindable.TextProperty, new Binding("."));
			template.SetValue(MockBindable.TextProperty, "value");

			MockBindable bindable = (MockBindable)template.CreateContent();
			Assert.That(bindable.GetValue(MockBindable.TextProperty), Is.EqualTo("value"));
			bindable.BindingContext = "binding";
			Assert.That(bindable.GetValue(MockBindable.TextProperty), Is.EqualTo("value"));
		}

		[Test]
		public void SetValueInvalid()
		{
			var template = new DataTemplate(typeof(MockBindable));
			Assert.That(() => template.SetValue(null, "string"), Throws.InstanceOf<ArgumentNullException>());
		}

		[Test]
		public void SetValueAndBinding()
		{
			var template = new DataTemplate(typeof(TextCell))
			{
				Bindings = {
					{TextCell.TextProperty, new Binding ("Text")}
				},
				Values = {
					{TextCell.TextProperty, "Text"}
				}
			};
			Assert.That(() => template.CreateContent(), Throws.InstanceOf<InvalidOperationException>());
		}
	}
}