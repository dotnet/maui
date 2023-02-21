using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class DataTemplateTests : BaseTestFixture
	{
		[Fact]
		public void CtorInvalid()
		{
			Assert.Throws<ArgumentNullException>(() => new DataTemplate((Func<object>)null));

			Assert.Throws<ArgumentNullException>(() => new DataTemplate((Type)null));
		}

		[Fact]
		public void CreateContent()
		{
			var template = new DataTemplate(() => new MockBindable());
			object obj = template.CreateContent();

			Assert.NotNull(obj);
			Assert.IsType<MockBindable>(obj);
		}

		[Fact]
		public void CreateContentType()
		{
			var template = new DataTemplate(typeof(MockBindable));
			object obj = template.CreateContent();

			Assert.NotNull(obj);
			Assert.IsType<MockBindable>(obj);
		}

		[Fact]
		public void CreateContentValues()
		{
			var template = new DataTemplate(typeof(MockBindable))
			{
				Values = { { MockBindable.TextProperty, "value" } }
			};

			MockBindable bindable = (MockBindable)template.CreateContent();
			Assert.Equal("value", bindable.GetValue(MockBindable.TextProperty));
		}

		[Fact]
		public void CreateContentBindings()
		{
			var template = new DataTemplate(() => new MockBindable())
			{
				Bindings = { { MockBindable.TextProperty, new Binding(".") } }
			};

			MockBindable bindable = (MockBindable)template.CreateContent();
			bindable.BindingContext = "text";
			Assert.Equal("text", bindable.GetValue(MockBindable.TextProperty));
		}

		[Fact]
		public void SetBindingInvalid()
		{
			var template = new DataTemplate(typeof(MockBindable));
			Assert.Throws<ArgumentNullException>(() => template.SetBinding(null, new Binding(".")));
			Assert.Throws<ArgumentNullException>(() => template.SetBinding(MockBindable.TextProperty, null));
		}

		[Fact]
		public void SetBindingOverridesValue()
		{
			var template = new DataTemplate(typeof(MockBindable));
			template.SetValue(MockBindable.TextProperty, "value");
			template.SetBinding(MockBindable.TextProperty, new Binding("."));

			MockBindable bindable = (MockBindable)template.CreateContent();
			Assert.Equal(bindable.GetValue(MockBindable.TextProperty), bindable.BindingContext);

			bindable.BindingContext = "binding";
			Assert.Equal("binding", bindable.GetValue(MockBindable.TextProperty));
		}

		[Fact]
		public void SetValueOverridesBinding()
		{
			var template = new DataTemplate(typeof(MockBindable));
			template.SetBinding(MockBindable.TextProperty, new Binding("."));
			template.SetValue(MockBindable.TextProperty, "value");

			MockBindable bindable = (MockBindable)template.CreateContent();
			Assert.Equal("value", bindable.GetValue(MockBindable.TextProperty));
			bindable.BindingContext = "binding";
			Assert.Equal("value", bindable.GetValue(MockBindable.TextProperty));
		}

		[Fact]
		public void SetValueInvalid()
		{
			var template = new DataTemplate(typeof(MockBindable));
			Assert.Throws<ArgumentNullException>(() => template.SetValue(null, "string"));
		}

		[Fact]
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
			Assert.Throws<InvalidOperationException>(() => template.CreateContent());
		}

		[Fact]
		public void HotReloadTransitionDoesNotCrash()
		{
			// Hot Reload may need to create a template while the content portion isn't ready yet
			// We need to make sure that a call to CreateContent during that time doesn't crash
			var template = new DataTemplate();
			template.CreateContent();
		}
	}
}
