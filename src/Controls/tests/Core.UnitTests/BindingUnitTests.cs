using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;


	public class BindingUnitTests
		: BindingBaseUnitTests
	{
		protected override BindingBase CreateBinding(BindingMode mode = BindingMode.Default, string stringFormat = null)
		{
			return new Binding("Text", mode, stringFormat: stringFormat);
		}

		static void AssertNoErrorsLogged()
		{
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
					"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[Fact]
		public void Ctor()
		{
			const string path = "Foo";

			var binding = new Binding(path, BindingMode.OneWayToSource);
			Assert.Equal(path, binding.Path);
			Assert.Equal(BindingMode.OneWayToSource, binding.Mode);
		}

		[Fact]
		public void InvalidCtor()
		{
			Assert.Throws<ArgumentNullException>(() => new Binding(null));

			Assert.Throws<ArgumentException>(() => new Binding(String.Empty));

			Assert.Throws<ArgumentException>(() => new Binding("   "));

			Assert.Throws<ArgumentException>(() => new Binding("Path", (BindingMode)Int32.MaxValue));
		}

		[Fact("You should get an exception when trying to change a binding after it's been applied")]
		public void ChangeBindingAfterApply()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var binding = (Binding)CreateBinding(BindingMode.Default, "Foo {0}");

			var vm = new MockViewModel { Text = "Bar" };
			var bo = new MockBindable { BindingContext = vm };
			bo.SetBinding(property, binding);

			Assert.Throws<InvalidOperationException>(() => binding.Path = "path");
			Assert.Throws<InvalidOperationException>(() => binding.Converter = null);
			Assert.Throws<InvalidOperationException>(() => binding.ConverterParameter = new object());
		}

		[Fact]
		public void NullPathIsSelf()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var binding = new Binding();

			var bo = new MockBindable { BindingContext = "Foo" };
			bo.SetBinding(property, binding);

			Assert.Equal("Foo", bo.GetValue(property));
		}

		class ComplexPropertyNamesViewModel
			: MockViewModel
		{
			public string Foo_Bar
			{
				get;
				set;
			}

			public string @if
			{
				get;
				set;
			}

			/*public string P̀ः०‿
			{
				get;
				set;
			}*/

			public string _UnderscoreStart
			{
				get;
				set;
			}
		}

		[Theory, Category("[Binding] Complex paths")]
		[InlineData("Foo_Bar")]
		[InlineData("if")]
		//TODO FIXME [TestCase ("P̀ः०‿")]
		[InlineData("_UnderscoreStart")]
		public void ComplexPropertyNames(string propertyName)
		{
			var vm = new ComplexPropertyNamesViewModel();
			vm.GetType().GetProperty(propertyName).SetValue(vm, "Value");

			var binding = new Binding(propertyName);

			var bindable = new MockBindable { BindingContext = vm };
			bindable.SetBinding(MockBindable.TextProperty, binding);

			Assert.Equal("Value", bindable.Text);
		}

		[Theory]
		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Category("[Binding] Complex paths")]
		public void ValueSetOnOneWayWithComplexPathBinding(
			 bool setContextFirst,
			 bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Text = value
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), null, propertyDefault);
			var binding = new Binding("Model.Model.Text", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.Equal(value, viewmodel.Model.Model.Text);
			Assert.Equal(value, bindable.GetValue(property));
			Assert.True(MockApplication.MockLogger.Messages.Count == 0,
				"An error was logged: " + MockApplication.MockLogger.Messages.FirstOrDefault());
		}

		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Theory, Category("[Binding] Complex paths")]
		public void ValueSetOnOneWayToSourceWithComplexPathBinding(
			 bool setContextFirst,
			 bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), value, propertyDefault);
			var binding = new Binding("Model.Model.Text", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.Equal(value, bindable.GetValue(property));
			Assert.Equal(value, viewmodel.Model.Model.Text);

			AssertNoErrorsLogged();
		}

		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Theory, Category("[Binding] Complex paths")]
		public void ValueSetOnTwoWayWithComplexPathBinding(
			 bool setContextFirst,
			 bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Text = value
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new Binding("Model.Model.Text", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			AssertSourceAndTarget(value, viewmodel.Model.Model.Text, bindable, property);
			AssertNoErrorsLogged();
		}

		static void AssertSourceAndTarget(string value, string vmValue, BindableObject bindable, BindableProperty property)
		{
			Assert.True(value == vmValue,
				"BindingContext property changed");
			Assert.True(value == (string)bindable.GetValue(property),
				"Target property did not change");
		}

		class Outer
		{
			public Outer(Inner inner)
			{
				PropertyWithPublicSetter = inner;
				PropertyWithPrivateSetter = inner;
			}

			public Inner PropertyWithPublicSetter { get; set; }
			public Inner PropertyWithPrivateSetter { get; private set; }
		}

		class Inner
		{
			public Inner(string property)
			{
				GetSetProperty = property;
			}

			public string GetSetProperty { get; set; }
		}

		[Theory, Category("[Binding] Complex paths")]
		[InlineData(true, BindingMode.OneWay, true)]
		[InlineData(true, BindingMode.OneWay, false)]
		[InlineData(true, BindingMode.TwoWay, true)]
		[InlineData(true, BindingMode.TwoWay, false)]
		[InlineData(false, BindingMode.OneWay, true)]
		[InlineData(false, BindingMode.OneWay, false)]
		[InlineData(false, BindingMode.TwoWay, true)]
		[InlineData(false, BindingMode.TwoWay, false)]
		public void BindingWithNoPublicSetterOnParent(
			bool setContextFirst,
			BindingMode bindingmode,
			bool usePrivateSetter)
		{
			var value = "FooBar";
			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", BindingMode.Default);
			var binding = new Binding(usePrivateSetter ? "PropertyWithPrivateSetter.GetSetProperty" : "PropertyWithPublicSetter.GetSetProperty", bindingmode);
			var viewmodel = new Outer(new Inner(value));
			var bindable = new MockBindable();

			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.Equal(value, viewmodel.PropertyWithPublicSetter.GetSetProperty);
			Assert.Equal(value, bindable.GetValue(property));

			if (bindingmode == BindingMode.TwoWay)
			{
				var updatedValue = "Qux";
				bindable.SetValue(property, updatedValue);
				Assert.Equal(updatedValue, bindable.GetValue(property));
				Assert.Equal(updatedValue, viewmodel.PropertyWithPublicSetter.GetSetProperty);
			}

			AssertNoErrorsLogged();
		}

		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Theory, Category("[Binding] Indexed paths")]
		public void ValueSetOnOneWayWithIndexedPathBinding(
			 bool setContextFirst,
			 bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = value;

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), null, propertyDefault);

			var binding = new Binding("Model.Model[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			AssertSourceAndTarget(value, viewmodel.Model.Model[1], bindable, property);
			AssertNoErrorsLogged();
		}

		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Theory, Category("[Binding] Indexed paths")]
		public void ValueSetOnOneWayWithSelfIndexedPathBinding(
			 bool setContextFirst,
			 bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel();
			viewmodel[1] = value;

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), null, propertyDefault);

			var binding = new Binding(".[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			AssertSourceAndTarget(value, viewmodel[1], bindable, property);
			AssertNoErrorsLogged();
		}

		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Theory, Category("[Binding] Indexed paths")]
		public void ValueSetOnOneWayWithIndexedPathArrayBinding(
			 bool setContextFirst,
			 bool isDefault)
		{
			const string value = "Bar";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Array = new[] { "Foo", "Bar" }
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), null, propertyDefault);

			var binding = new Binding("Model.Array[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			AssertSourceAndTarget(value, viewmodel.Model.Array[1], bindable, property);
			AssertNoErrorsLogged();
		}

		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Theory, Category("[Binding] Indexed paths")]
		public void ValueSetOnOneWayWithIndexedSelfPathArrayBinding(
			 bool setContextFirst,
			 bool isDefault)
		{
			const string value = "bar";
			string[] context = new[] { "foo", value };

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), null, propertyDefault);

			var binding = new Binding(".[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = context;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = context;
			}

			AssertSourceAndTarget(value, context[1], bindable, property);
			AssertNoErrorsLogged();
		}

		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Theory, Category("[Binding] Indexed paths")]
		public void ValueSetOnOneWayToSourceWithIndexedPathBinding(
			 bool setContextFirst,
			 bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel()
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), defaultValue: value, defaultBindingMode: propertyDefault);

			var binding = new Binding("Model.Model[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			AssertSourceAndTarget(value, viewmodel.Model.Model[1], bindable, property);
			AssertNoErrorsLogged();
		}

		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Theory, Category("[Binding] Indexed paths")]
		public void ValueSetOnTwoWayWithIndexedPathBinding(
			 bool setContextFirst,
			 bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = value;

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var binding = new Binding("Model.Model[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			AssertSourceAndTarget(value, viewmodel.Model.Model[1], bindable, property);
			AssertNoErrorsLogged();
		}

		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Theory, Category("[Binding] Indexed paths")]
		public void ValueSetOnTwoWayWithIndexedArrayPathBinding(
			bool setContextFirst,
			bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Array = new string[2]
				}
			};
			viewmodel.Model.Array[1] = value;

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var binding = new Binding("Model.Array[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.Equal(value, viewmodel.Model.Array[1]);
			Assert.Equal(value, bindable.GetValue(property));

			AssertNoErrorsLogged();
		}

		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Theory, Category("[Binding] Indexed paths")]
		public void ValueSetOnTwoWayWithIndexedArraySelfPathBinding(
			bool setContextFirst,
			bool isDefault)
		{
			const string value = "Foo";
			string[] viewmodel = new[] { "bar", value };

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var binding = new Binding(".[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			AssertSourceAndTarget(value, viewmodel[1], bindable, property);
			AssertNoErrorsLogged();
		}

		[MemberData(nameof(TestDataHelpers.TrueFalse), MemberType = typeof(TestDataHelpers))]
		[Theory, Category("[Binding] Self paths")]
		public void ValueSetOnOneWayWithSelfPathBinding(
			bool setContextFirst,
			bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), null, propertyDefault);

			var binding = new Binding(".", bindingMode);

			const string value = "value";

			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = value;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = value;
			}

			AssertSourceAndTarget(value, (string)bindable.BindingContext, bindable, property);
			AssertNoErrorsLogged();
		}

		[InlineData(true)]
		[InlineData(false)]
		[Theory, Category("[Binding] Self paths")]
		public void ValueNotSetOnOneWayToSourceWithSelfPathBinding(
			bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			const string value = "Foo";
			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), defaultValue: value, defaultBindingMode: propertyDefault);

			var binding = new Binding(".", bindingMode);

			var bindable = new MockBindable();
			Assert.Null(bindable.BindingContext);

			bindable.SetBinding(property, binding);

			Assert.True(value == (string)bindable.GetValue(property),
				"Target property changed");
			Assert.True(bindable.BindingContext == null,
				"BindingContext changed with self-path binding");

			AssertNoErrorsLogged();
		}

		[InlineData(true, true)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(false, false)]
		[Theory, Category("[Binding] Self paths")]
		public void ValueSetOnTwoWayWithSelfPathBinding(
			 bool setContextFirst,
			 bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var binding = new Binding(".", bindingMode);

			const string value = "Foo";
			var bindable = new MockBindable();
			if (setContextFirst)
			{
				bindable.BindingContext = value;
				bindable.SetBinding(property, binding);
			}
			else
			{
				bindable.SetBinding(property, binding);
				bindable.BindingContext = value;
			}

			AssertSourceAndTarget(value, (string)bindable.BindingContext, bindable, property);
			AssertNoErrorsLogged();
		}

		[Theory, Category("[Binding] Complex paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithComplexPathOnOneWayBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Text = "Foo"
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, new Binding("Model.Model.Text", bindingMode));

			viewmodel.Model.Model.Text = newvalue;

			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.True(newvalue == viewmodel.Model.Model.Text,
				"Source property changed when it shouldn't");

			AssertNoErrorsLogged();
		}

		[Theory, Category("[Binding] Complex paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithComplexPathOnOneWayToSourceBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Text = "Foo"
					}
				}
			};
			;

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, new Binding("Model.Model.Text", bindingMode));

			string original = (string)bindable.GetValue(property);
			const string value = "value";
			viewmodel.Model.Model.Text = value;
			Assert.True(original == (string)bindable.GetValue(property),
				"Target updated from Source on OneWayToSource");

			bindable.SetValue(property, newvalue);
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.True(newvalue == viewmodel.Model.Model.Text,
				"Source property changed when it shouldn't");

			AssertNoErrorsLogged();
		}

		[Theory, Category("[Binding] Complex paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithComplexPathOnTwoWayBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Text = "Foo"
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, new Binding("Model.Model.Text", bindingMode));

			viewmodel.Model.Model.Text = newvalue;

			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Target property did not update change");
			Assert.True(newvalue == viewmodel.Model.Model.Text,
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.True(newvalue2 == viewmodel.Model.Model.Text,
				"Source property did not update with Target's change");
			Assert.True(newvalue2 == (string)bindable.GetValue(property),
				"Target property changed from what it was set to");

			AssertNoErrorsLogged();
		}

		[Theory, Category("[Binding] Indexed paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithIndexedPathOnOneWayBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, new Binding("Model.Model[1]", bindingMode));

			viewmodel.Model.Model[1] = newvalue;
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.True(newvalue == viewmodel.Model.Model[1],
				"Source property changed when it shouldn't");

			AssertNoErrorsLogged();
		}

		[Theory, Category("[Binding] Indexed paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithIndexedPathOnOneWayToSourceBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, new Binding("Model.Model[1]", bindingMode));

			string original = (string)bindable.GetValue(property);
			const string value = "value";
			viewmodel.Model.Model[1] = value;
			Assert.True(original == (string)bindable.GetValue(property),
				"Target updated from Source on OneWayToSource");

			bindable.SetValue(property, newvalue);
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.True(newvalue == viewmodel.Model.Model[1],
				"Source property changed when it shouldn't");

			AssertNoErrorsLogged();
		}

		[Theory, Category("[Binding] Indexed paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithIndexedPathOnTwoWayBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, new Binding("Model.Model[1]", bindingMode));

			viewmodel.Model.Model[1] = newvalue;
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Target property did not update change");
			Assert.True(newvalue == viewmodel.Model.Model[1],
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.True(newvalue2 == viewmodel.Model.Model[1],
				"Source property did not update with Target's change");
			Assert.True(newvalue2 == (string)bindable.GetValue(property),
				"Target property changed from what it was set to");

			AssertNoErrorsLogged();
		}

		[Theory, Category("[Binding] Indexed paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithIndexedArrayPathOnTwoWayBinding(bool isDefault)
		{
			var viewmodel = new ComplexMockViewModel
			{
				Array = new string[2]
			};
			viewmodel.Array[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, new Binding("Array[1]", bindingMode));

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.True(newvalue2 == viewmodel.Array[1],
				"Source property did not update with Target's change");
			Assert.True(newvalue2 == (string)bindable.GetValue(property),
				"Target property changed from what it was set to");

			AssertNoErrorsLogged();
		}

		[Theory, Category("[Binding] Self paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithSelfPathOnOneWayBinding(bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			const string value = "foo";

			var bindable = new MockBindable();
			bindable.BindingContext = value;
			bindable.SetBinding(property, new Binding(".", bindingMode));

			const string newvalue = "value";
			bindable.SetValue(property, newvalue);
			Assert.True(value == (string)bindable.BindingContext,
				"Source was updated from Target on OneWay binding");

			bindable.BindingContext = newvalue;
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.True(newvalue == (string)bindable.BindingContext,
				"Source property changed when it shouldn't");

			AssertNoErrorsLogged();
		}

		[Theory, Category("[Binding] Self paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueDoesNotUpdateWithSelfPathOnOneWayToSourceBinding(bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault)
			{
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var binding = new Binding(".", bindingMode);

			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);

			const string newvalue = "new value";

			string original = (string)bindable.GetValue(property);
			bindable.BindingContext = newvalue;
			Assert.True(original == (string)bindable.GetValue(property),
				"Target updated from Source on OneWayToSource with self path");

			const string newvalue2 = "new value 2";
			bindable.SetValue(property, newvalue2);
			Assert.True(newvalue2 == (string)bindable.GetValue(property),
				"Target property changed on OneWayToSource with self path");
			Assert.True(newvalue == (string)bindable.BindingContext,
				"Source property changed on OneWayToSource with self path");

			AssertNoErrorsLogged();
		}

		[Theory, Category("[Binding] Self paths")]
		[InlineData(true)]
		[InlineData(false)]
		public void ValueUpdatedWithSelfPathOnTwoWayBinding(bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault)
			{
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var binding = new Binding(".", bindingMode);

			var bindable = new MockBindable();
			bindable.BindingContext = "value";
			bindable.SetBinding(property, binding);

			const string newvalue = "New Value";
			bindable.BindingContext = newvalue;
			Assert.True(newvalue == (string)bindable.GetValue(property),
				"Target property did not update change");
			Assert.True(newvalue == (string)bindable.BindingContext,
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.True(newvalue == (string)bindable.BindingContext,
				"Self-path Source changed with Target's change");
			Assert.True(newvalue2 == (string)bindable.GetValue(property),
				"Target property changed from what it was set to");

			AssertNoErrorsLogged();
		}


		[Theory, Category("[Binding] Complex paths")]
		[InlineData(BindingMode.OneWay)]
		[InlineData(BindingMode.OneWayToSource)]
		[InlineData(BindingMode.TwoWay)]
		public async Task WeakPropertyChangedProxyDoesNotLeak(BindingMode mode)
		{
			var proxies = new List<WeakReference>();
			WeakReference weakViewModel = null, weakBindable = null;

			int i = 0;
			void create()
			{
				if (i++ < 1024)
				{
					create();
					return;
				}

				var binding = new Binding("Model.Model[1]");
				var bindable = new MockBindable();
				weakBindable = new WeakReference(bindable);

				var viewmodel = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel
					{
						Model = new ComplexMockViewModel()
					}
				};

				weakViewModel = new WeakReference(viewmodel);

				bindable.BindingContext = viewmodel;
				bindable.SetBinding(MockBindable.TextProperty, binding);

				// Access private members:
				// WeakPropertyChangedProxy proxy = binding._expression._parts[i]._listener;
				var flags = BindingFlags.NonPublic | BindingFlags.Instance;
				var expression = binding.GetType().GetField("_expression", flags).GetValue(binding);
				Assert.NotNull(expression);
				var parts = expression.GetType().GetField("_parts", flags).GetValue(expression) as IEnumerable;
				Assert.NotNull(parts);
				foreach (var part in parts)
				{
					var listener = part.GetType().GetField("_listener", flags).GetValue(part);
					if (listener == null)
						continue;
					proxies.Add(new WeakReference(listener));
				}
				Assert.NotEmpty(proxies); // Should be at least 1
			};
			create();

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			if (mode == BindingMode.TwoWay || mode == BindingMode.OneWay)
				Assert.False(weakViewModel.IsAlive, "ViewModel wasn't collected");

			if (mode == BindingMode.TwoWay || mode == BindingMode.OneWayToSource)
				Assert.False(weakBindable.IsAlive, "Bindable wasn't collected");

			// WeakPropertyChangedProxy won't go away until the second GC, BindingExpressionPart unsubscribes in its finalizer
			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			foreach (var proxy in proxies)
			{
				Assert.False(proxy.IsAlive, "WeakPropertyChangedProxy wasn't collected");
			}
		}

		[Theory, Category("[Binding] Complex paths")]
		[InlineData(BindingMode.OneWay)]
		[InlineData(BindingMode.OneWayToSource)]
		[InlineData(BindingMode.TwoWay)]
		public void SourceAndTargetAreWeakComplexPath(BindingMode mode)
		{
			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "default value");

			var binding = new Binding("Model.Model[1]");

			WeakReference weakViewModel = null, weakBindable = null;

			HackAroundMonoSucking(0, property, binding, out weakViewModel, out weakBindable);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			if (mode == BindingMode.TwoWay || mode == BindingMode.OneWay)
				Assert.False(weakViewModel.IsAlive, "ViewModel wasn't collected");

			if (mode == BindingMode.TwoWay || mode == BindingMode.OneWayToSource)
				Assert.False(weakBindable.IsAlive, "Bindable wasn't collected");
		}

		// Mono doesn't handle the GC properly until the stack frame where the object is created is popped.
		// This means calling another method and not just using lambda as works in real .NET
		void HackAroundMonoSucking(int i, BindableProperty property, Binding binding, out WeakReference weakViewModel, out WeakReference weakBindable)
		{
			if (i++ < 1024)
			{
				HackAroundMonoSucking(i, property, binding, out weakViewModel, out weakBindable);
				return;
			}

			MockBindable bindable = new MockBindable();

			weakBindable = new WeakReference(bindable);

			ComplexMockViewModel viewmodel = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel()
				}
			};

			weakViewModel = new WeakReference(viewmodel);

			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			bindable.BindingContext = null;
		}

		class TestConverter<TSource, TTarget>
			: IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				Assert.Equal(typeof(TTarget), targetType);
				return System.Convert.ChangeType(value, targetType, CultureInfo.CurrentUICulture);
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				Assert.Equal(typeof(TSource), targetType);
				return System.Convert.ChangeType(value, targetType, CultureInfo.CurrentUICulture);
			}
		}

		[Fact]
		public void ValueConverter()
		{
			var converter = new TestConverter<string, int>();

			var vm = new MockViewModel("1");
			var property = BindableProperty.Create("TargetInt", typeof(int), typeof(MockBindable), 0);
			var binding = CreateBinding();
			((Binding)binding).Converter = converter;

			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.Equal(1, bindable.GetValue(property));

			AssertNoErrorsLogged();
		}

		[Fact]
		public void ValueConverterBack()
		{
			var converter = new TestConverter<string, int>();

			var vm = new MockViewModel();
			var property = BindableProperty.Create(nameof(MockBindable.TargetInt), typeof(int), typeof(MockBindable), 1, BindingMode.OneWayToSource);
			var bindable = new MockBindable();
			bindable.SetBinding(property, new Binding("Text", converter: converter));
			bindable.BindingContext = vm;

			Assert.Equal("1", vm.Text);

			AssertNoErrorsLogged();
		}


		class TestConverterParameter : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return parameter;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return parameter;
			}
		}

		[Fact]
		public void ValueConverterParameter()
		{
			var converter = new TestConverterParameter();

			var vm = new MockViewModel();
			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "Bar", BindingMode.OneWayToSource);
			var bindable = new MockBindable();
			bindable.SetBinding(property, new Binding("Text", converter: converter, converterParameter: "Foo"));
			bindable.BindingContext = vm;

			Assert.Equal("Foo", vm.Text);

			AssertNoErrorsLogged();
		}

		class TestConverterCulture : IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return culture.ToString();
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return culture.ToString();
			}
		}

#if !WINDOWS_PHONE
		[Theory, InlineData("en-US"), InlineData("pt-PT"), InlineData("tr-TR")]
		public void ValueConverterCulture(string culture)
		{
			System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

			var converter = new TestConverterCulture();
			var vm = new MockViewModel();
			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "Bar", BindingMode.OneWayToSource);
			var bindable = new MockBindable();
			bindable.SetBinding(property, new Binding("Text", converter: converter));
			bindable.BindingContext = vm;

			Assert.Equal(culture, vm.Text);
		}
#endif

		[Fact]
		public void SelfBindingConverter()
		{
			var converter = new TestConverter<int, string>();

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "0");
			var bindable = new MockBindable();
			bindable.BindingContext = 1;
			bindable.SetBinding(property, new Binding(Binding.SelfPath, converter: converter));
			Assert.Equal("1", bindable.GetValue(property));

			AssertNoErrorsLogged();
		}

		internal class MultiplePropertyViewModel
			: INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			int done;
			public int Done
			{
				get { return done; }
				set
				{
					done = value;
					OnPropertyChanged();
					OnPropertyChanged("Progress");
				}
			}

			int total = 100;
			public int Total
			{
				get { return total; }
				set
				{
					if (total == value)
						return;

					total = value;
					OnPropertyChanged();
					OnPropertyChanged("Progress");
				}
			}

			public float Progress
			{
				get { return (float)done / total; }
			}

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChangedEventHandler handler = PropertyChanged;
				if (handler != null)
					handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		internal class MultiplePropertyBindable
			: BindableObject
		{
			public static readonly BindableProperty ValueProperty =
				BindableProperty.Create(nameof(MultiplePropertyBindable.Value), typeof(float), typeof(MultiplePropertyBindable), 0f);

			public float Value
			{
				get { return (float)GetValue(ValueProperty); }
				set { SetValue(ValueProperty, value); }
			}

			public static readonly BindableProperty DoneProperty =
				BindableProperty.Create(nameof(MultiplePropertyBindable.Done), typeof(int), typeof(MultiplePropertyBindable), 0);

			public int Done
			{
				get { return (int)GetValue(DoneProperty); }
				set { SetValue(DoneProperty, value); }
			}
		}

		[Fact]
		public void MultiplePropertyUpdates()
		{
			var mpvm = new MultiplePropertyViewModel();

			var bindable = new MultiplePropertyBindable();
			bindable.SetBinding(MultiplePropertyBindable.ValueProperty, new Binding("Progress", BindingMode.OneWay));
			bindable.SetBinding(MultiplePropertyBindable.DoneProperty, new Binding("Done", BindingMode.OneWayToSource));
			bindable.BindingContext = mpvm;

			bindable.Done = 5;

			Assert.Equal(5, mpvm.Done);
			Assert.Equal(0.05f, mpvm.Progress);
			Assert.Equal(5, bindable.Done);
			Assert.Equal(0.05f, bindable.Value);


			AssertNoErrorsLogged();
		}

		[Fact, Category("[Binding] Complex paths")]
		[Description("When part of a complex path can not be evaluated during an update, bindables should return to their default value.")]
		public void NullInPathUsesDefaultValue()
		{
			var vm = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel()
			};

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "foo bar");

			var bindable = new MockBindable();
			bindable.SetBinding(property, new Binding("Model.Text", BindingMode.OneWay));
			bindable.BindingContext = vm;

			vm.Model = null;

			Assert.Equal(property.DefaultValue, bindable.GetValue(property));

			AssertNoErrorsLogged();
		}

		[Fact, Category("[Binding] Complex paths")]
		[Description("When part of a complex path can not be evaluated during an update, bindables should return to their default value.")]
		public void NullContextUsesDefaultValue()
		{
			var vm = new ComplexMockViewModel
			{
				Model = new ComplexMockViewModel
				{
					Text = "vm value"
				}
			};

			var property = BindableProperty.Create(nameof(MockBindable.Text), typeof(string), typeof(MockBindable), "foo bar");

			var bindable = new MockBindable();
			bindable.SetBinding(property, new Binding("Model.Text", BindingMode.OneWay));
			bindable.BindingContext = vm;

			Assert.Equal(bindable.GetValue(property), vm.Model.Text);

			bindable.BindingContext = null;

			Assert.Equal(property.DefaultValue, bindable.GetValue(property));

			AssertNoErrorsLogged();
		}

		[Fact("OneWay bindings should not double apply on source updates.")]
		public void OneWayBindingsDontDoubleApplyOnSourceUpdates()
		{
			var vm = new ComplexMockViewModel();

			var bindable = new MockBindable();
			bindable.SetBinding(MultiplePropertyBindable.DoneProperty, new Binding("QueryCount", BindingMode.OneWay));
			bindable.BindingContext = vm;

			Assert.Equal(1, vm.count);

			bindable.BindingContext = null;

			Assert.Equal(1, vm.count);

			bindable.BindingContext = vm;

			Assert.Equal(2, vm.count);
		}

		[Fact("When there are multiple bindings, an update in one should not cause the other to udpate.")]
		public void BindingsShouldNotTriggerOtherBindings()
		{
			var vm = new ComplexMockViewModel();

			var bindable = new MockBindable();
			bindable.SetBinding(MultiplePropertyBindable.DoneProperty, new Binding("QueryCount", BindingMode.OneWay));
			bindable.SetBinding(MockBindable.TextProperty, new Binding("Text", BindingMode.OneWay));
			bindable.BindingContext = vm;

			Assert.Equal(1, vm.count);

			vm.Text = "update";

			Assert.True(1 == vm.count, "Source property was queried due to a different binding update.");
		}

		internal class DerivedViewModel
			: MockViewModel
		{
			public override string Text
			{
				get { return base.Text + "2"; }
				set { base.Text = value; }
			}
		}

		[Fact("The most derived version of a property should always be called.")]
		public void MostDerviedPropertyOnContextSwitchOfSimilarType()
		{
			var vm = new MockViewModel { Text = "text" };

			var bindable = new MockBindable();
			bindable.BindingContext = vm;
			bindable.SetBinding(MockBindable.TextProperty, new Binding("Text"));

			Assert.Equal(vm.Text, bindable.GetValue(MockBindable.TextProperty));

			bindable.BindingContext = vm = new DerivedViewModel { Text = "text" };

			Assert.Equal(vm.Text, bindable.GetValue(MockBindable.TextProperty));
		}

		internal class EmptyViewModel
		{
		}

		internal class DifferentViewModel
			: INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			string text = "foo";

			public string Text
			{
				get { return text; }
			}

			public string Text2
			{
				set { text = value; }
			}

			public string PrivateSetter
			{
				get;
				private set;
			}
		}

		[Fact("Paths should not distinguish types, a context change to a completely different type should work.")]
		public void DifferentContextTypeAccessedCorrectlyWithSamePath()
		{
			var vm = new MockViewModel { Text = "text" };

			var bindable = new MockBindable();
			bindable.BindingContext = vm;
			bindable.SetBinding(MockBindable.TextProperty, new Binding("Text"));

			Assert.Equal(vm.Text, bindable.GetValue(MockBindable.TextProperty));

			var dvm = new DifferentViewModel();
			bindable.BindingContext = dvm;

			Assert.Equal(dvm.Text, bindable.GetValue(MockBindable.TextProperty));
		}

		[Fact]
		public void Clone()
		{
			object param = new object();
			var binding = new Binding(".", converter: new TestConverter<string, int>(), converterParameter: param, stringFormat: "{0}");
			var clone = (Binding)binding.Clone();

			Assert.Same(binding.Converter, clone.Converter);
			Assert.Same(binding.ConverterParameter, clone.ConverterParameter);
			Assert.Equal(binding.Mode, clone.Mode);
			Assert.Equal(binding.Path, clone.Path);
			Assert.Equal(binding.StringFormat, clone.StringFormat);
		}

		[Fact]
		public void PropertyMissingOneWay()
		{
			var bindable = new MockBindable { BindingContext = new MockViewModel() };
			bindable.Text = "foo";

			bindable.SetBinding(MockBindable.TextProperty, new Binding("Monkeys", BindingMode.OneWay));
			Assert.True(MockApplication.MockLogger.Messages.Count == 1, "An error was not logged");
		}

		[Fact]
		public void PropertyMissingOneWayToSource()
		{
			var bindable = new MockBindable { BindingContext = new MockViewModel() };
			bindable.Text = "foo";

			bindable.SetBinding(MockBindable.TextProperty, new Binding("Monkeys", BindingMode.OneWayToSource));
			Assert.True(MockApplication.MockLogger.Messages.Count == 1, "An error was not logged");
			Assert.Equal(bindable.Text, bindable.Text);
		}

		[Fact]
		public void PropertyMissingTwoWay()
		{
			var bindable = new MockBindable { BindingContext = new MockViewModel() };
			bindable.Text = "foo";

			bindable.SetBinding(MockBindable.TextProperty, new Binding("Monkeys"));
			// The first error is for the initial binding, the second is for reflecting the update back to the default value
			Assert.True(MockApplication.MockLogger.Messages.Count >= 1, "An error was not logged");
		}

		[Fact]
		public void GetterMissingTwoWay()
		{
			var bindable = new MockBindable { BindingContext = new DifferentViewModel() };

			bindable.SetBinding(MockBindable.TextProperty, new Binding("Text2"));
			Assert.Equal(bindable.Text, MockBindable.TextProperty.DefaultValue);
			Assert.True(MockApplication.MockLogger.Messages.Count == 1, "An error was not logged");
			Assert.Contains(MockApplication.MockLogger.Messages[0], String.Format(BindingExpression.PropertyNotFoundErrorMessage,
				"Text2",
				"Microsoft.Maui.Controls.Core.UnitTests.BindingUnitTests+DifferentViewModel",
				"Microsoft.Maui.Controls.Core.UnitTests.MockBindable",
				"Text"), StringComparison.InvariantCulture);

		}

		[Fact]
		public void BindingAppliesAfterGetterPreviouslyMissing()
		{
			var bindable = new MockBindable { BindingContext = new EmptyViewModel() };
			bindable.SetBinding(MockBindable.TextProperty, new Binding("Text"));

			bindable.BindingContext = new MockViewModel { Text = "Foo" };
			Assert.Equal("Foo", bindable.Text);

			Assert.True(MockApplication.MockLogger.Messages.Count <= 1, "Too many errors were logged");

			Assert.Contains(MockApplication.MockLogger.Messages[0], String.Format(BindingExpression.PropertyNotFoundErrorMessage,
				"Text",
				"Microsoft.Maui.Controls.Core.UnitTests.BindingUnitTests+EmptyViewModel",
				"Microsoft.Maui.Controls.Core.UnitTests.MockBindable",
				"Text"), StringComparison.InvariantCulture);
		}

		[Fact]
		public void SetterMissingTwoWay()
		{
			var bindable = new MockBindable { BindingContext = new DifferentViewModel() };
			bindable.SetBinding(MockBindable.TextProperty, new Binding("Text"));

			Assert.True(MockApplication.MockLogger.Messages.Count == 1, "An error was not logged");
			Assert.Contains(MockApplication.MockLogger.Messages[0], String.Format(BindingExpression.PropertyNotFoundErrorMessage,
				"Text",
				"Microsoft.Maui.Controls.Core.UnitTests.BindingUnitTests+DifferentViewModel",
				"Microsoft.Maui.Controls.Core.UnitTests.MockBindable",
				"Text"), StringComparison.InvariantCulture);

			bindable.SetValueCore(MockBindable.TextProperty, "foo");
		}

		[Fact]
		public void PrivateSetterTwoWay()
		{
			var bindable = new MockBindable { BindingContext = new DifferentViewModel() };
			bindable.SetBinding(MockBindable.TextProperty, new Binding("PrivateSetter"));

			Assert.True(MockApplication.MockLogger.Messages.Count == 1, "An error was not logged");
			Assert.Contains(MockApplication.MockLogger.Messages[0], String.Format(BindingExpression.PropertyNotFoundErrorMessage,
				"PrivateSetter",
				"Microsoft.Maui.Controls.Core.UnitTests.BindingUnitTests+DifferentViewModel",
				"Microsoft.Maui.Controls.Core.UnitTests.MockBindable",
				"Text"), StringComparison.InvariantCulture);

			bindable.SetValueCore(MockBindable.TextProperty, "foo");

			//test fails sometimes (race condition)
			//Assert.True(MockApplication.MockLogger.Messages.Count == (2), "An error was not logged");
			//Assert.Contains(MockApplication.MockLogger.Messages[1], String.Format(BindingExpression.PropertyNotFoundErrorMessage,
			//	"PrivateSetter",
			//	"Microsoft.Maui.Controls.Core.UnitTests.BindingUnitTests+DifferentViewModel",
			//	"Microsoft.Maui.Controls.Core.UnitTests.MockBindable",
			//	"Text"), StringComparison.InvariantCulture);
		}

		[Fact]
		public void PropertyNotFound()
		{
			var bindable = new MockBindable { BindingContext = new MockViewModel() };
			bindable.SetBinding(MockBindable.TextProperty, new Binding("MissingProperty"));

			Assert.True(MockApplication.MockLogger.Messages.Count == 1, "An error was not logged");
			Assert.Contains(MockApplication.MockLogger.Messages[0], String.Format(BindingExpression.PropertyNotFoundErrorMessage,
				"MissingProperty",
				"Microsoft.Maui.Controls.Core.UnitTests.MockViewModel",
				"Microsoft.Maui.Controls.Core.UnitTests.MockBindable",
				"Text"), StringComparison.InvariantCulture);
		}

		[Fact("When binding with a multi-part path and part is null, no error should be thrown or logged")]
		public void ChainedPartNull()
		{
			var bindable = new MockBindable { BindingContext = new ComplexMockViewModel() };
			bindable.SetBinding(MockBindable.TextProperty, new Binding("Model.Text"));
			AssertNoErrorsLogged();
		}

		[Fact]
		public void PropertyNotFoundChained()
		{
			var bindable = new MockBindable
			{
				BindingContext = new ComplexMockViewModel
				{
					Model = new ComplexMockViewModel()
				}

			};
			bindable.SetBinding(MockBindable.TextProperty, new Binding("Model.MissingProperty"));

			Assert.True(MockApplication.MockLogger.Messages.Count == 1, "An error was not logged");
			Assert.Contains(MockApplication.MockLogger.Messages[0], String.Format(BindingExpression.PropertyNotFoundErrorMessage,
				"MissingProperty",
				"Microsoft.Maui.Controls.Core.UnitTests.BindingBaseUnitTests+ComplexMockViewModel",
				"Microsoft.Maui.Controls.Core.UnitTests.MockBindable",
				"Text"), StringComparison.InvariantCulture);

			Assert.Equal(bindable.Text, MockBindable.TextProperty.DefaultValue);
		}

		[Fact]
		public void SetBindingContextBeforeContextBindingAndInnerBindings()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };

			view.BindingContext = new { item0 = "Foo", item1 = "Bar" };
			label.SetBinding(BindableObject.BindingContextProperty, "item0");
			label.SetBinding(Label.TextProperty, Binding.SelfPath);

			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void SetBindingContextAndInnerBindingBeforeContextBinding()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };

			view.BindingContext = new { item0 = "Foo", item1 = "Bar" };
			label.SetBinding(Label.TextProperty, Binding.SelfPath);
			label.SetBinding(BindableObject.BindingContextProperty, "item0");

			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void SetBindingContextAfterContextBindingAndInnerBindings()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };

			label.SetBinding(BindableObject.BindingContextProperty, "item0");
			label.SetBinding(Label.TextProperty, Binding.SelfPath);
			view.BindingContext = new { item0 = "Foo", item1 = "Bar" };

			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void SetBindingContextAfterInnerBindingsAndContextBinding()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };

			label.SetBinding(Label.TextProperty, Binding.SelfPath);
			label.SetBinding(BindableObject.BindingContextProperty, "item0");
			view.BindingContext = new { item0 = "Foo", item1 = "Bar" };

			Assert.Equal("Foo", label.Text);
		}

		[Fact]
		public void Convert()
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

			var slider = new Slider();
			var vm = new MockViewModel { Text = "0.5" };
			slider.BindingContext = vm;
			slider.SetBinding(Slider.ValueProperty, "Text", BindingMode.TwoWay);

			Assert.Equal(0.5, slider.Value);

			slider.Value = 0.9;

			Assert.Equal("0.9", vm.Text);
		}

#if !WINDOWS_PHONE
		[Theory, InlineData("en-US", "0.5", 0.5, 0.9, "0.9")]
		[InlineData("pt-PT", "0,5", 0.5, 0.9, "0,9")]
		public void ConvertIsCultureAware(string culture, string sliderSetStringValue, double sliderExpectedDoubleValue, double sliderSetDoubleValue, string sliderExpectedStringValue)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = System.Threading.Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);

			var slider = new Slider();
			var vm = new MockViewModel { Text = sliderSetStringValue };
			slider.BindingContext = vm;
			slider.SetBinding(Slider.ValueProperty, "Text", BindingMode.TwoWay);

			Assert.Equal(slider.Value, sliderExpectedDoubleValue);

			slider.Value = sliderSetDoubleValue;

			Assert.Equal(vm.Text, sliderExpectedStringValue);
		}
#endif

		[Fact]
		public void FailToConvert()
		{
			var slider = new Slider();
			slider.BindingContext = new ComplexMockViewModel { Model = new ComplexMockViewModel() };

			slider.SetBinding(Slider.ValueProperty, "Model");

			Assert.Equal(slider.Value, Slider.ValueProperty.DefaultValue);
		}

		class NullViewModel : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public string Foo
			{
				get;
				set;
			}

			public string Bar
			{
				get;
				set;
			}

			public void SignalAllPropertiesChanged(bool useNull)
			{
				var changed = PropertyChanged;
				if (changed != null)
					changed(this, new PropertyChangedEventArgs((useNull) ? null : String.Empty));
			}
		}

		class MockBindable2 : MockBindable
		{
			public static readonly BindableProperty Text2Property = BindableProperty.Create(nameof(MockBindable2.Text2), typeof(string), typeof(MockBindable2), "default", BindingMode.TwoWay);

			public string Text2
			{
				get { return (string)GetValue(Text2Property); }
				set { SetValue(Text2Property, value); }
			}
		}

		[Theory, InlineData(true)]
		[InlineData(false)]
		public void NullPropertyUpdatesAllBindings(bool useStringEmpty)
		{
			var vm = new NullViewModel();
			var bindable = new MockBindable2();
			bindable.BindingContext = vm;
			bindable.SetBinding(MockBindable.TextProperty, "Foo");
			bindable.SetBinding(MockBindable2.Text2Property, "Bar");

			vm.Foo = "Foo";
			vm.Bar = "Bar";
			vm.SignalAllPropertiesChanged(useNull: !useStringEmpty);

			Assert.Equal("Foo", bindable.Text);
			Assert.Equal("Bar", bindable.Text2);
		}

		[Fact]
		public void BindingSourceOverContext()
		{
			var label = new Label();
			label.BindingContext = "bindingcontext";
			label.SetBinding(Label.TextProperty, Binding.SelfPath);
			Assert.Equal("bindingcontext", label.Text);

			label.SetBinding(Label.TextProperty, new Binding(Binding.SelfPath, source: "bindingsource"));
			Assert.Equal("bindingsource", label.Text);
		}

		class TestViewModel : INotifyPropertyChanged
		{
			event PropertyChangedEventHandler PropertyChanged;

			event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
			{
				add { PropertyChanged += value; }
				remove { PropertyChanged -= value; }
			}

			public string Foo { get; set; }

			public int InvocationListSize()
			{
				if (PropertyChanged == null)
					return 0;
				return PropertyChanged.GetInvocationList().Length;
			}

			public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[Fact]
		public async Task BindingUnsubscribesForDeadTarget()
		{
			var viewmodel = new TestViewModel();

			{
				var button = new Button();
				button.SetBinding(Button.TextProperty, "Foo");
				button.BindingContext = viewmodel;
			}

			Assert.Equal(1, viewmodel.InvocationListSize());

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			viewmodel.OnPropertyChanged("Foo");

			Assert.Equal(0, viewmodel.InvocationListSize());
		}

		[Fact]
		public async Task BindingDoesNotStayAliveForDeadTarget()
		{
			var viewModel = new TestViewModel();
			WeakReference bindingRef;

			{
				var binding = new Binding("Foo");

				var button = new Button();
				button.SetBinding(Button.TextProperty, binding);
				button.BindingContext = viewModel;

				bindingRef = new WeakReference(binding);
			}

			Assert.Equal(1, viewModel.InvocationListSize());

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.False(bindingRef.IsAlive, "Binding should not be alive!");
		}

		[Fact]
		public void BindingCreatesSingleSubscription()
		{
			TestViewModel viewmodel = new TestViewModel();

			var button = new Button();
			button.SetBinding(Button.TextProperty, "Foo");
			button.BindingContext = viewmodel;

			Assert.Equal(1, viewmodel.InvocationListSize());
		}

		public class IndexedViewModel : INotifyPropertyChanged
		{
			Dictionary<string, object> dict = new Dictionary<string, object>();

			public object this[string index]
			{
				get { return dict[index]; }
				set
				{
					dict[index] = value;
					OnPropertyChanged("Item[" + index + "]");

				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		[Fact]
		public void IndexedViewModelPropertyChanged()
		{
			var label = new Label();
			var viewModel = new IndexedViewModel();
			//viewModel["Foo"] = "Bar";

			label.BindingContext = new
			{
				Data = viewModel
			};
			label.SetBinding(Label.TextProperty, "Data[Foo]");


			Assert.Null(label.Text);

			viewModel["Foo"] = "Baz";

			Assert.Equal("Baz", label.Text);
		}

		class VM57081
		{
			string _foo;
			public string Foo
			{
				get
				{
					Count++;
					return _foo;
				}
				set { _foo = value; }
			}

			public int Count { get; set; }
		}

		[Fact]
		// https://bugzilla.xamarin.com/show_bug.cgi?id=57081
		public void BindingWithSourceNotReappliedWhenBindingContextIsChanged()
		{
			var bindable = new MockBindable();
			var model = new VM57081();
			var bp = BindableProperty.Create("foo", typeof(string), typeof(MockBindable), null);
			Assert.Equal(0, model.Count);
			bindable.SetBinding(bp, new Binding { Path = "Foo", Source = model });
			Assert.Equal(1, model.Count);
			bindable.BindingContext = new object();
			Assert.Equal(1, model.Count);
		}

		[Fact]
		// https://bugzilla.xamarin.com/show_bug.cgi?id=57081
		public void BindingWithSourceNotReappliedWhenParented()
		{
			var view = new ContentView();
			var model = new VM57081();
			Assert.Equal(0, model.Count);
			view.SetBinding(BindableObject.BindingContextProperty, new Binding { Path = "Foo", Source = model });
			Assert.Equal(1, model.Count);
			var parent = new ContentView { BindingContext = new object() };
			parent.Content = view;
			Assert.Equal(1, model.Count);
		}

		class MockValueTupleContext
		{
			public (string Foo, string Bar) Tuple { get; set; }
		}

		[Fact]
		public void ValueTupleAsBindingContext()
		{
			var label = new Label
			{
				BindingContext = new MockValueTupleContext { Tuple = (Foo: "FOO", Bar: "BAR") },
			};

			label.SetBinding(Label.TextProperty, "Tuple.Foo");
			Assert.Equal("FOO", label.Text);
			label.SetBinding(Label.TextProperty, "Tuple[1]");
			Assert.Equal("BAR", label.Text);
		}

		[Fact]
		public void OneTimeBindingDoesntUpdateOnPropertyChanged()
		{
			var view = new VisualElement();
			var bp1t = BindableProperty.Create("Foo", typeof(string), typeof(VisualElement));
			var bp1w = BindableProperty.Create("Foo", typeof(string), typeof(VisualElement));
			var vm = new MockViewModel("foobar");
			view.BindingContext = vm;
			view.SetBinding(bp1t, "Text", mode: BindingMode.OneTime);
			view.SetBinding(bp1w, "Text", mode: BindingMode.OneWay);
			Assert.Equal("foobar", view.GetValue(bp1w));
			Assert.Equal("foobar", view.GetValue(bp1t));

			vm.Text = "qux";
			Assert.Equal("qux", view.GetValue(bp1w));
			Assert.Equal("foobar", view.GetValue(bp1t));
		}

		[Fact]
		public void OneTimeBindingUpdatesOnBindingContextChanged()
		{
			var view = new VisualElement();
			var bp1t = BindableProperty.Create("Foo", typeof(string), typeof(VisualElement));
			var bp1w = BindableProperty.Create("Foo", typeof(string), typeof(VisualElement));
			view.BindingContext = new MockViewModel("foobar");
			view.SetBinding(bp1t, "Text", mode: BindingMode.OneTime);
			view.SetBinding(bp1w, "Text", mode: BindingMode.OneWay);
			Assert.Equal("foobar", view.GetValue(bp1w));
			Assert.Equal("foobar", view.GetValue(bp1t));

			view.BindingContext = new MockViewModel("qux");
			Assert.Equal("qux", view.GetValue(bp1w));
			Assert.Equal("qux", view.GetValue(bp1t));
		}

		[Fact]
		public void FallbackValueWhenSourceIsNull()
		{
			var bindable = new MockBindable();
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), "default");
			bindable.SetBinding(property, new Binding("Foo.Bar") { FallbackValue = "fallback" });
			Assert.Equal("fallback", bindable.GetValue(property));
		}

		[Fact]
		//https://github.com/xamarin/Xamarin.Forms/issues/3467
		public void TargetNullValueIgnoredWhenBindingIsResolved()
		{
			var bindable = new MockBindable();
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), "default");
			bindable.SetBinding(property, new Binding("Text") { TargetNullValue = "fallback" });
			Assert.Equal("default", bindable.GetValue(property));
			bindable.BindingContext = new MockViewModel { Text = "Foo" };
			Assert.Equal("Foo", bindable.GetValue(property));
		}

		[Fact]
		public void TargetNullValueFallback()
		{
			var bindable = new MockBindable();
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), "default");
			bindable.SetBinding(property, new Binding("Text") { TargetNullValue = "fallback" });
			Assert.Equal("default", bindable.GetValue(property));
			bindable.BindingContext = new MockViewModel();
			Assert.Equal("fallback", bindable.GetValue(property));
		}

		[Fact]
		//https://github.com/xamarin/Xamarin.Forms/issues/3994
		public void INPCOnBindingWithSource()
		{
			var page = new ContentPage { Title = "Foo" };
			page.BindingContext = page;
			var label = new Label();
			page.Content = label;

			label.SetBinding(Label.TextProperty, new Binding("BindingContext.Title", source: page));
			Assert.Equal("Foo", label.Text);

			page.Title = "Bar";
			Assert.Equal("Bar", label.Text);
		}

		[Fact]
		//https://github.com/xamarin/Xamarin.Forms/issues/10405
		public void TypeConversionExceptionIsCaughtAndLogged()
		{
			var label = new Label();
			label.SetBinding(Label.TextColorProperty, "color");

			_ = label.BindingContext = new { color = "" };
			Assert.True(MockApplication.MockLogger.Messages.Count == 1, "No error logged");
		}

		[Fact]
		//https://github.com/dotnet/maui/issues/7977
		public void NullRefWithDefaultCtor()
		{
			var label = new Label();
			label.SetBinding(Label.TextColorProperty, new Binding());
		}
	}
}