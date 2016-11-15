using System;
using System.Collections.Generic;
using System.Globalization;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NUnit.Framework;
using CategoryAttribute=NUnit.Framework.CategoryAttribute;
using DescriptionAttribute=NUnit.Framework.DescriptionAttribute;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class BindingUnitTests
		: BindingBaseUnitTests
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup ();
			log = new Logger();

			Device.PlatformServices = new MockPlatformServices ();
			Log.Listeners.Add (log);
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown ();
			Device.PlatformServices = null;
			Log.Listeners.Remove (log);
		}

		protected override BindingBase CreateBinding(BindingMode mode = BindingMode.Default, string stringFormat = null)
		{
			return new Binding ("Text", mode, stringFormat: stringFormat);
		}

		[Test]
		public void Ctor()
		{
			const string path = "Foo";

			var binding = new Binding (path, BindingMode.OneWayToSource);
			Assert.AreEqual (path, binding.Path);
			Assert.AreEqual (BindingMode.OneWayToSource, binding.Mode);
		}

		[Test]
		public void InvalidCtor()
		{
			Assert.Throws<ArgumentNullException> (() => new Binding (null),
				"Allowed null Path");

			Assert.Throws<ArgumentException> (() => new Binding (String.Empty),
				"Allowed empty path");

			Assert.Throws<ArgumentException> (() => new Binding ("   "),
				"Allowed whitespace path");

			Assert.Throws<ArgumentException> (() => new Binding ("Path", (BindingMode)Int32.MaxValue),
				"Allowed invalid value for BindingMode");
		}

		[Test]
		[Description ("You should get an exception when trying to change a binding after it's been applied")]
		public void ChangeBindingAfterApply()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var binding = (Binding)CreateBinding(BindingMode.Default, "Foo {0}");

			var vm = new MockViewModel { Text = "Bar" };
			var bo = new MockBindable { BindingContext = vm };
			bo.SetBinding (property, binding);

			Assert.That (() => binding.Path = "path", Throws.InvalidOperationException);
			Assert.That (() => binding.Converter = null, Throws.InvalidOperationException);
			Assert.That (() => binding.ConverterParameter = new object(), Throws.InvalidOperationException);
		}

		[Test]
		public void NullPathIsSelf()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var binding = new Binding();

			var bo = new MockBindable { BindingContext = "Foo" };
			bo.SetBinding(property, binding);

			Assert.That(bo.GetValue(property), Is.EqualTo("Foo"));
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

		[Category ("[Binding] Complex paths")]
		[TestCase ("Foo_Bar")]
		[TestCase ("if")]
		//TODO FIXME [TestCase ("P̀ः०‿")]
		[TestCase ("_UnderscoreStart")]
		public void ComplexPropertyNames (string propertyName)
		{
			var vm = new ComplexPropertyNamesViewModel();
			vm.GetType().GetProperty (propertyName).SetValue (vm, "Value");

			var binding = new Binding (propertyName);

			var bindable = new MockBindable { BindingContext = vm };
			bindable.SetBinding (MockBindable.TextProperty, binding);

			Assert.That (bindable.Text, Is.EqualTo ("Value"));
		}

		[Test]
		[Category ("[Binding] Complex paths")]
		public void ValueSetOnOneWayWithComplexPathBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel {
						Text = value
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), null, propertyDefault);
			var binding = new Binding("Model.Model.Text", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual (value, viewmodel.Model.Model.Text,
				"BindingContext property changed");
			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Complex paths")]
		public void ValueSetOnOneWayToSourceWithComplexPathBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel {
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault) {
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), value, propertyDefault);
			var binding = new Binding("Model.Model.Text", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property changed");
			Assert.AreEqual (value, viewmodel.Model.Model.Text,
				"BindingContext property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Complex paths")]
		public void ValueSetOnTwoWayWithComplexPathBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel {
						Text = value
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = new Binding("Model.Model.Text", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual (value, viewmodel.Model.Model.Text,
				"BindingContext property changed");
			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		class Outer {
			public Outer (Inner inner)
			{
				PropertyWithPublicSetter = inner;
				PropertyWithPrivateSetter = inner;
			}

			public Inner PropertyWithPublicSetter { get; set; }
			public Inner PropertyWithPrivateSetter { get; private set; }
		}

		class Inner {
			public Inner (string property)
			{
				GetSetProperty = property;
			}

			public string GetSetProperty { get; set; }
		}

		[Test, Category ("[Binding] Complex paths")]
		public void BindingWithNoPublicSetterOnParent (
			[Values (true, false)] bool setContextFirst,
			[Values (BindingMode.OneWay, BindingMode.TwoWay)] BindingMode bindingmode,
			[Values (true, false)] bool usePrivateSetter)
		{
			var value = "FooBar";
			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", BindingMode.Default);
			var binding = new Binding(usePrivateSetter ? "PropertyWithPrivateSetter.GetSetProperty" : "PropertyWithPublicSetter.GetSetProperty", bindingmode);
			var viewmodel = new Outer(new Inner(value));
			var bindable = new MockBindable();

			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual (value, viewmodel.PropertyWithPublicSetter.GetSetProperty);
			Assert.AreEqual (value, bindable.GetValue (property));

			if (bindingmode == BindingMode.TwoWay) {
				var updatedValue = "Qux";
				bindable.SetValue (property, updatedValue);
				Assert.AreEqual (updatedValue, bindable.GetValue (property));
				Assert.AreEqual (updatedValue, viewmodel.PropertyWithPublicSetter.GetSetProperty);
			}
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Indexed paths")]
		public void ValueSetOnOneWayWithIndexedPathBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = value;

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, null, propertyDefault);

			var binding = new Binding ("Model.Model[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual (value, viewmodel.Model.Model[1],
				"BindingContext property changed");
			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Indexed paths")]
		public void ValueSetOnOneWayWithSelfIndexedPathBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel();
			viewmodel[1] = value;

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, null, propertyDefault);

			var binding = new Binding (".[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual (value, viewmodel[1],
				"BindingContext property changed");
			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Indexed paths")]
		public void ValueSetOnOneWayWithIndexedPathArrayBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			const string value = "Bar";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Array = new [] { "Foo", "Bar" }
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, null, propertyDefault);

			var binding = new Binding ("Model.Array[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual (value, viewmodel.Model.Array[1],
				"BindingContext property changed");
			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Indexed paths")]
		public void ValueSetOnOneWayWithIndexedSelfPathArrayBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			const string value = "bar";
			string[] context = new[] { "foo", value };

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, null, propertyDefault);

			var binding = new Binding (".[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = context;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = context;
			}

			Assert.AreEqual (value, context[1],
				"BindingContext property changed");
			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Indexed paths")]
		public void ValueSetOnOneWayToSourceWithIndexedPathBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel()
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault) {
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, defaultValue: value, defaultBindingMode: propertyDefault);

			var binding = new Binding ("Model.Model[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property changed");
			Assert.AreEqual (value, viewmodel.Model.Model[1],
				"BindingContext property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Indexed paths")]
		public void ValueSetOnTwoWayWithIndexedPathBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = value;

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			var binding = new Binding ("Model.Model[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual (value, viewmodel.Model.Model[1],
				"BindingContext property changed");
			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Indexed paths")]
		public void ValueSetOnTwoWayWithIndexedArrayPathBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Array = new string[2]
				}
			};
			viewmodel.Model.Array[1] = value;

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			var binding = new Binding ("Model.Array[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual (value, viewmodel.Model.Array[1],
				"BindingContext property changed");
			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Indexed paths")]
		public void ValueSetOnTwoWayWithIndexedArraySelfPathBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			const string value = "Foo";
			string[] viewmodel = new [] { "bar", value };

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			var binding = new Binding (".[1]", bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual (value, viewmodel[1],
				"BindingContext property changed");
			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Self paths")]
		public void ValueSetOnOneWayWithSelfPathBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, null, propertyDefault);

			var binding = new Binding (".", bindingMode);

			const string value = "value";

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = value;
				bindable.SetBinding (property, binding);
			} else {
				bindable.SetBinding (property, binding);
				bindable.BindingContext = value;
			}

			Assert.AreEqual (value, bindable.BindingContext,
				"BindingContext property changed");
			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property did not change");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Self paths")]
		public void ValueNotSetOnOneWayToSourceWithSelfPathBinding (
			[Values (true, false)] bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault) {
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			const string value = "Foo";
			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, defaultValue: value, defaultBindingMode: propertyDefault);

			var binding = new Binding (".", bindingMode);

			var bindable = new MockBindable();
			Assert.IsNull (bindable.BindingContext);

			bindable.SetBinding (property, binding);

			Assert.AreEqual (value, bindable.GetValue (property),
				"Target property changed");
			Assert.IsNull (bindable.BindingContext,
				"BindingContext changed with self-path binding");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Self paths")]
		public void ValueSetOnTwoWayWithSelfPathBinding (
			[Values (true, false)] bool setContextFirst,
			[Values (true, false)] bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);

			var binding = new Binding(".", bindingMode);

			const string value = "Foo";
			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = value;
				bindable.SetBinding(property, binding);
			} else {
				bindable.SetBinding(property, binding);
				bindable.BindingContext = value;
			}

			Assert.AreEqual(value, bindable.BindingContext,
				"BindingContext property changed");
			Assert.AreEqual(value, bindable.GetValue(property),
				"Target property did not change");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Complex paths")]
		[TestCase(true)]
		[TestCase(false)]
		public void ValueUpdatedWithComplexPathOnOneWayBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel {
						Text = "Foo"
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding (property, new Binding ("Model.Model.Text", bindingMode));

			viewmodel.Model.Model.Text = newvalue;
			Assert.AreEqual (newvalue, bindable.GetValue (property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual (newvalue, viewmodel.Model.Model.Text,
				"Source property changed when it shouldn't");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category ("[Binding] Complex paths")]
		[TestCase (true)]
		[TestCase (false)]
		public void ValueUpdatedWithComplexPathOnOneWayToSourceBinding (bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel {
						Text = "Foo"
					}
				}
			};;

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault) {
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding (property, new Binding ("Model.Model.Text", bindingMode));

			string original = (string)bindable.GetValue (property);
			const string value = "value";
			viewmodel.Model.Model.Text = value;
			Assert.AreEqual (original, bindable.GetValue (property),
				"Target updated from Source on OneWayToSource");

			bindable.SetValue (property, newvalue);
			Assert.AreEqual (newvalue, bindable.GetValue (property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual (newvalue, viewmodel.Model.Model.Text,
				"Source property changed when it shouldn't");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category ("[Binding] Complex paths")]
		[TestCase (true)]
		[TestCase (false)]
		public void ValueUpdatedWithComplexPathOnTwoWayBinding (bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel {
						Text = "Foo"
					}
				}
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding (property, new Binding ("Model.Model.Text", bindingMode));

			viewmodel.Model.Model.Text = newvalue;
			Assert.AreEqual (newvalue, bindable.GetValue (property),
				"Target property did not update change");
			Assert.AreEqual (newvalue, viewmodel.Model.Model.Text,
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue (property, newvalue2);
			Assert.AreEqual (newvalue2, viewmodel.Model.Model.Text,
				"Source property did not update with Target's change");
			Assert.AreEqual (newvalue2, bindable.GetValue (property),
				"Target property changed from what it was set to");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category ("[Binding] Indexed paths")]
		[TestCase (true)]
		[TestCase (false)]
		public void ValueUpdatedWithIndexedPathOnOneWayBinding (bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding (property, new Binding ("Model.Model[1]", bindingMode));

			viewmodel.Model.Model[1] = newvalue;
			Assert.AreEqual (newvalue, bindable.GetValue (property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual (newvalue, viewmodel.Model.Model[1],
				"Source property changed when it shouldn't");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category ("[Binding] Indexed paths")]
		[TestCase (true)]
		[TestCase (false)]
		public void ValueUpdatedWithIndexedPathOnOneWayToSourceBinding (bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault) {
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding (property, new Binding ("Model.Model[1]", bindingMode));

			string original = (string)bindable.GetValue (property);
			const string value = "value";
			viewmodel.Model.Model[1] = value;
			Assert.AreEqual (original, bindable.GetValue (property),
				"Target updated from Source on OneWayToSource");

			bindable.SetValue (property, newvalue);
			Assert.AreEqual (newvalue, bindable.GetValue (property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual (newvalue, viewmodel.Model.Model[1],
				"Source property changed when it shouldn't");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category ("[Binding] Indexed paths")]
		[TestCase (true)]
		[TestCase (false)]
		public void ValueUpdatedWithIndexedPathOnTwoWayBinding (bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel()
				}
			};
			viewmodel.Model.Model[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding (property, new Binding ("Model.Model[1]", bindingMode));

			viewmodel.Model.Model[1] = newvalue;
			Assert.AreEqual (newvalue, bindable.GetValue (property),
				"Target property did not update change");
			Assert.AreEqual (newvalue, viewmodel.Model.Model[1],
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue (property, newvalue2);
			Assert.AreEqual (newvalue2, viewmodel.Model.Model[1],
				"Source property did not update with Target's change");
			Assert.AreEqual (newvalue2, bindable.GetValue (property),
				"Target property changed from what it was set to");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category ("[Binding] Indexed paths")]
		[TestCase (true)]
		[TestCase (false)]
		public void ValueUpdatedWithIndexedArrayPathOnTwoWayBinding (bool isDefault)
		{
			var viewmodel = new ComplexMockViewModel {
				Array = new string[2]
			};
			viewmodel.Array[1] = "Foo";

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding (property, new Binding ("Array[1]", bindingMode));

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue (property, newvalue2);
			Assert.AreEqual (newvalue2, viewmodel.Array[1],
				"Source property did not update with Target's change");
			Assert.AreEqual (newvalue2, bindable.GetValue (property),
				"Target property changed from what it was set to");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category ("[Binding] Self paths")]
		[TestCase (true)]
		[TestCase (false)]
		public void ValueUpdatedWithSelfPathOnOneWayBinding (bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			const string value = "foo";

			var bindable = new MockBindable();
			bindable.BindingContext = value;
			bindable.SetBinding (property, new Binding (".", bindingMode));

			const string newvalue = "value";
			bindable.SetValue (property, newvalue);
			Assert.AreEqual (value, bindable.BindingContext,
				"Source was updated from Target on OneWay binding");

			bindable.BindingContext = newvalue;
			Assert.AreEqual (newvalue, bindable.GetValue (property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual (newvalue, bindable.BindingContext,
				"Source property changed when it shouldn't");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category ("[Binding] Self paths")]
		[TestCase (true)]
		[TestCase (false)]
		public void ValueDoesNotUpdateWithSelfPathOnOneWayToSourceBinding (bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault) {
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "default value", propertyDefault);

			var binding = new Binding (".", bindingMode);

			var bindable = new MockBindable();
			bindable.SetBinding (property, binding);

			const string newvalue = "new value";

			string original = (string)bindable.GetValue (property);
			bindable.BindingContext = newvalue;
			Assert.AreEqual (original, bindable.GetValue (property),
				"Target updated from Source on OneWayToSource with self path");

			const string newvalue2 = "new value 2";
			bindable.SetValue (property, newvalue2);
			Assert.AreEqual (newvalue2, bindable.GetValue (property),
				"Target property changed on OneWayToSource with self path");
			Assert.AreEqual (newvalue, bindable.BindingContext,
				"Source property changed on OneWayToSource with self path");
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category ("[Binding] Self paths")]
		[TestCase (true)]
		[TestCase (false)]
		public void ValueUpdatedWithSelfPathOnTwoWayBinding (bool isDefault)
		{
			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create<MockBindable, string>(w => w.Text, "default value", propertyDefault);

			var binding = new Binding(".", bindingMode);

			var bindable = new MockBindable();
			bindable.BindingContext = "value";
			bindable.SetBinding(property, binding);

			const string newvalue = "New Value";
			bindable.BindingContext = newvalue;
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Target property did not update change");
			Assert.AreEqual(newvalue, bindable.BindingContext,
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.AreEqual(newvalue, bindable.BindingContext,
				"Self-path Source changed with Target's change");
			Assert.AreEqual(newvalue2, bindable.GetValue(property),
				"Target property changed from what it was set to");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Category("[Binding] Complex paths")]
		[TestCase(BindingMode.OneWay)]
		[TestCase(BindingMode.OneWayToSource)]
		[TestCase(BindingMode.TwoWay)]
		public void SourceAndTargetAreWeakComplexPath(BindingMode mode)
		{
			var property = BindableProperty.Create<MockBindable, string>(w => w.Text, "default value");

			var binding = new Binding("Model.Model[1]");

			WeakReference weakViewModel = null, weakBindable = null;

			HackAroundMonoSucking(0, property, binding, out weakViewModel, out weakBindable);

			GC.Collect();
			GC.WaitForPendingFinalizers();

			if (mode == BindingMode.TwoWay || mode == BindingMode.OneWay)
				Assert.IsFalse(weakViewModel.IsAlive, "ViewModel wasn't collected");

			if (mode == BindingMode.TwoWay || mode == BindingMode.OneWayToSource)
				Assert.IsFalse(weakBindable.IsAlive, "Bindable wasn't collected");
		}

		// Mono doesn't handle the GC properly until the stack frame where the object is created is popped.
		// This means calling another method and not just using lambda as works in real .NET
		void HackAroundMonoSucking(int i, BindableProperty property, Binding binding, out WeakReference weakViewModel, out WeakReference weakBindable)
		{
			if (i++ < 1024) {
				HackAroundMonoSucking(i, property, binding, out weakViewModel, out weakBindable);
				return;
			}

			MockBindable bindable = new MockBindable();

			weakBindable = new WeakReference(bindable);

			ComplexMockViewModel viewmodel = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Model = new ComplexMockViewModel()
				}
			};

			weakViewModel = new WeakReference (viewmodel);

			bindable.BindingContext = viewmodel;
			bindable.SetBinding (property, binding);

			bindable.BindingContext = null;
		}

		class TestConverter<TSource,TTarget>
			: IValueConverter
		{
			public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
			{
				Assert.AreEqual (typeof (TTarget), targetType);
				return System.Convert.ChangeType (value, targetType, CultureInfo.CurrentUICulture);
			}

			public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			{
				Assert.AreEqual (typeof (TSource), targetType);
				return System.Convert.ChangeType (value, targetType, CultureInfo.CurrentUICulture);
			}
		}

		[Test]
		public void ValueConverter()
		{
			var converter = new TestConverter<string, int>();

			var vm = new MockViewModel ("1");
			var property = BindableProperty.Create("TargetInt", typeof(int), typeof(MockBindable), 0);
			var binding = CreateBinding();
			((Binding)binding).Converter = converter;

			var bindable = new MockBindable();
			bindable.SetBinding(property, binding);
			bindable.BindingContext = vm;

			Assert.AreEqual(1, bindable.GetValue(property));

			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test]
		public void ValueConverterBack()
		{
			var converter = new TestConverter<string, int>();

			var vm = new MockViewModel();
			var property = BindableProperty.Create<MockBindable, int> (w=>w.TargetInt, 1, BindingMode.OneWayToSource);
			var bindable = new MockBindable();
			bindable.SetBinding (property, new Binding ("Text", converter: converter));
			bindable.BindingContext = vm;

			Assert.AreEqual ("1", vm.Text);

			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}


		class TestConverterParameter : IValueConverter
		{
			public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
			{
				return parameter;
			}

			public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			{
				return parameter;
			}
		}

		[Test]
		public void ValueConverterParameter ()
		{
			var converter = new TestConverterParameter ();

			var vm = new MockViewModel ();
			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "Bar", BindingMode.OneWayToSource);
			var bindable = new MockBindable();
			bindable.SetBinding (property, new Binding ("Text", converter: converter, converterParameter: "Foo"));
			bindable.BindingContext = vm;

			Assert.AreEqual ("Foo", vm.Text);

			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		class TestConverterCulture : IValueConverter
		{
			public object Convert (object value, Type targetType, object parameter, CultureInfo culture)
			{
				return culture.ToString ();
			}

			public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture)
			{
				return culture.ToString ();
			}
		}

		#if !WINDOWS_PHONE
		[Test]
		[SetUICulture ("pt-PT")]
		public void ValueConverterCulture ()
		{
			var converter = new TestConverterCulture ();
			var vm = new MockViewModel ();
			var property = BindableProperty.Create<MockBindable, string> (w=>w.Text, "Bar", BindingMode.OneWayToSource);
			var bindable = new MockBindable();
			bindable.SetBinding (property, new Binding ("Text", converter: converter));
			bindable.BindingContext = vm;

			Assert.AreEqual ("pt-PT", vm.Text);
		}
		#endif

		[Test]
		public void SelfBindingConverter()
		{
			var converter = new TestConverter<int, string> ();

			var property = BindableProperty.Create<MockBindable, string> (w => w.Text, "0");
			var bindable = new MockBindable ();
			bindable.BindingContext = 1;
			bindable.SetBinding (property, new Binding (Binding.SelfPath, converter:converter));
			Assert.AreEqual ("1", bindable.GetValue (property));

			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
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
					OnPropertyChanged ("Progress");
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
					OnPropertyChanged ("Progress");
				}
			}

			public float Progress
			{
				get { return (float)done / total; }
			}

			protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
			{
				PropertyChangedEventHandler handler = PropertyChanged;
				if (handler != null)
					handler (this, new PropertyChangedEventArgs (propertyName));
			}
		}

		internal class MultiplePropertyBindable
			: BindableObject
		{
			public static readonly BindableProperty ValueProperty =
				BindableProperty.Create<MultiplePropertyBindable, float> (b => b.Value, 0f);

			public float Value
			{
				get { return (float)GetValue (ValueProperty); }
				set { SetValue (ValueProperty, value); }
			}

			public static readonly BindableProperty DoneProperty =
				BindableProperty.Create<MultiplePropertyBindable, int> (b => b.Done, 0);

			public int Done
			{
				get { return (int)GetValue (DoneProperty); }
				set { SetValue (DoneProperty, value); }
			}
		}

		[Test]
		public void MultiplePropertyUpdates()
		{
			var mpvm = new MultiplePropertyViewModel();

			var bindable = new MultiplePropertyBindable();
			bindable.SetBinding (MultiplePropertyBindable.ValueProperty, new Binding ("Progress", BindingMode.OneWay));
			bindable.SetBinding (MultiplePropertyBindable.DoneProperty, new Binding ("Done", BindingMode.OneWayToSource));
			bindable.BindingContext = mpvm;

			bindable.Done = 5;

			Assert.AreEqual (5, mpvm.Done);
			Assert.AreEqual (0.05f, mpvm.Progress);
			Assert.AreEqual (5, bindable.Done);
			Assert.AreEqual (0.05f, bindable.Value);

			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Complex paths")]
		[Description ("When part of a complex path can not be evaluated during an update, bindables should return to their default value.")]
		public void NullInPathUsesDefaultValue()
		{
			var vm = new ComplexMockViewModel {
				Model = new ComplexMockViewModel()
			};

			var property = BindableProperty.Create<MockBindable, string> (w => w.Text, "foo bar");

			var bindable = new MockBindable();
			bindable.SetBinding (property, new Binding ("Model.Text", BindingMode.OneWay));
			bindable.BindingContext = vm;

			vm.Model = null;

			Assert.AreEqual (property.DefaultValue, bindable.GetValue (property));
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category ("[Binding] Complex paths")]
		[Description ("When part of a complex path can not be evaluated during an update, bindables should return to their default value.")]
		public void NullContextUsesDefaultValue()
		{
			var vm = new ComplexMockViewModel {
				Model = new ComplexMockViewModel {
					Text = "vm value"
				}
			};

			var property = BindableProperty.Create<MockBindable, string> (w => w.Text, "foo bar");

			var bindable = new MockBindable();
			bindable.SetBinding (property, new Binding ("Model.Text", BindingMode.OneWay));
			bindable.BindingContext = vm;

			Assume.That (bindable.GetValue (property), Is.EqualTo (vm.Model.Text));

			bindable.BindingContext = null;

			Assert.AreEqual (property.DefaultValue, bindable.GetValue (property));
			Assert.That (log.Messages.Count, Is.EqualTo (0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test]
		[Description ("OneWay bindings should not double apply on source updates.")]
		public void OneWayBindingsDontDoubleApplyOnSourceUpdates()
		{
			var vm = new ComplexMockViewModel();

			var bindable = new MockBindable();
			bindable.SetBinding (MultiplePropertyBindable.DoneProperty, new Binding ("QueryCount", BindingMode.OneWay));
			bindable.BindingContext = vm;

			Assert.AreEqual (1, vm.count);

			bindable.BindingContext = null;

			Assert.AreEqual (1, vm.count, "Source property was queried on an unset");

			bindable.BindingContext = vm;

			Assert.AreEqual (2, vm.count, "Source property was queried multiple times on a reapply");
		}

		[Test]
		[Description ("When there are multiple bindings, an update in one should not cause the other to udpate.")]
		public void BindingsShouldNotTriggerOtherBindings()
		{
			var vm = new ComplexMockViewModel();

			var bindable = new MockBindable();
			bindable.SetBinding (MultiplePropertyBindable.DoneProperty, new Binding ("QueryCount", BindingMode.OneWay));
			bindable.SetBinding (MockBindable.TextProperty, new Binding ("Text", BindingMode.OneWay));
			bindable.BindingContext = vm;

			Assert.AreEqual (1, vm.count);

			vm.Text = "update";

			Assert.AreEqual (1, vm.count, "Source property was queried due to a different binding update.");
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

		[Test]
		[Description ("The most derived version of a property should always be called.")]
		public void MostDerviedPropertyOnContextSwitchOfSimilarType()
		{
			var vm = new MockViewModel { Text = "text" };

			var bindable = new MockBindable();
			bindable.BindingContext = vm;
			bindable.SetBinding (MockBindable.TextProperty, new Binding ("Text"));

			Assert.AreEqual (vm.Text, bindable.GetValue (MockBindable.TextProperty));

			bindable.BindingContext = vm = new DerivedViewModel { Text = "text" };

			Assert.AreEqual (vm.Text, bindable.GetValue (MockBindable.TextProperty));
		}

		internal class EmptyViewModel
		{
		}

		internal class DifferentViewModel
			: INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			string text = "foo";

			public string Text {
				get { return text; }
			}

			public string Text2 {
				set { text = value; }
			}

			public string PrivateSetter {
				get;
				private set;
			}
		}

		[Test]
		[Description("Paths should not distinguish types, a context change to a completely different type should work.")]
		public void DifferentContextTypeAccessedCorrectlyWithSamePath()
		{
			var vm = new MockViewModel { Text = "text" };

			var bindable = new MockBindable();
			bindable.BindingContext = vm;
			bindable.SetBinding(MockBindable.TextProperty, new Binding("Text"));

			Assert.AreEqual(vm.Text, bindable.GetValue(MockBindable.TextProperty));

			var dvm = new DifferentViewModel();
			bindable.BindingContext = dvm;

			Assert.AreEqual(dvm.Text, bindable.GetValue(MockBindable.TextProperty));
		}

		[Test]
		public void Clone()
		{
			object param = new object();
			var binding = new Binding(".", converter: new TestConverter<string, int>(), converterParameter: param, stringFormat: "{0}");
			var clone = (Binding)binding.Clone();

			Assert.AreSame(binding.Converter, clone.Converter);
			Assert.AreSame(binding.ConverterParameter, clone.ConverterParameter);
			Assert.AreEqual(binding.Mode, clone.Mode);
			Assert.AreEqual(binding.Path, clone.Path);
			Assert.AreEqual(binding.StringFormat, clone.StringFormat);
		}

		[Test]
		public void PropertyMissingOneWay()
		{
			var bindable = new MockBindable { BindingContext = new MockViewModel() };
			bindable.Text = "foo";

			Assert.That (() => bindable.SetBinding (MockBindable.TextProperty, new Binding ("Monkeys", BindingMode.OneWay)), Throws.Nothing);
			Assert.That (log.Messages.Count, Is.EqualTo (1), "An error was not logged");
			Assert.That (bindable.Text, Is.EqualTo (MockBindable.TextProperty.DefaultValue));
		}

		[Test]
		public void PropertyMissingOneWayToSource()
		{
			var bindable = new MockBindable { BindingContext = new MockViewModel() };
			bindable.Text = "foo";

			Assert.That (() => bindable.SetBinding (MockBindable.TextProperty, new Binding ("Monkeys", BindingMode.OneWayToSource)), Throws.Nothing);
			Assert.That (log.Messages.Count, Is.EqualTo (1), "An error was not logged");
			Assert.That (bindable.Text, Is.EqualTo (bindable.Text));
		}

		[Test]
		public void PropertyMissingTwoWay()
		{
			var bindable = new MockBindable { BindingContext = new MockViewModel() };
			bindable.Text = "foo";

			Assert.That (() => bindable.SetBinding (MockBindable.TextProperty, new Binding ("Monkeys")), Throws.Nothing);
			// The first error is for the initial binding, the second is for reflecting the update back to the default value
			Assert.That (log.Messages.Count, Is.EqualTo (2), "An error was not logged");
			Assert.That (bindable.Text, Is.EqualTo (MockBindable.TextProperty.DefaultValue));
		}

		[Test]
		public void GetterMissingTwoWay()
		{
			var bindable = new MockBindable { BindingContext = new DifferentViewModel() };
			bindable.Text = "foo";

			Assert.That (() => bindable.SetBinding (MockBindable.TextProperty, new Binding ("Text2")), Throws.Nothing);
			Assert.That (bindable.Text, Is.EqualTo (MockBindable.TextProperty.DefaultValue));
			Assert.That (log.Messages.Count, Is.EqualTo (1), "An error was not logged");
			Assert.That (log.Messages[0], Is.StringContaining (String.Format (BindingExpression.PropertyNotFoundErrorMessage,
				"Text2",
				"Xamarin.Forms.Core.UnitTests.BindingUnitTests+DifferentViewModel",
				"Xamarin.Forms.Core.UnitTests.MockBindable",
				"Text")));

			Assert.That (((DifferentViewModel) bindable.BindingContext).Text, Is.EqualTo (MockBindable.TextProperty.DefaultValue));
		}

		[Test]
		public void BindingAppliesAfterGetterPreviouslyMissing()
		{
			var bindable = new MockBindable { BindingContext = new EmptyViewModel() };
			bindable.SetBinding (MockBindable.TextProperty, new Binding ("Text"));

			bindable.BindingContext = new MockViewModel { Text = "Foo" };
			Assert.That (bindable.Text, Is.EqualTo ("Foo"));

			Assert.That (log.Messages.Count, Is.Not.GreaterThan (1), "Too many errors were logged");
			Assert.That (log.Messages[0], Is.StringContaining (String.Format (BindingExpression.PropertyNotFoundErrorMessage,
				"Text",
				"Xamarin.Forms.Core.UnitTests.BindingUnitTests+EmptyViewModel",
				"Xamarin.Forms.Core.UnitTests.MockBindable",
				"Text")));
		}

		[Test]
		public void SetterMissingTwoWay()
		{
			var bindable = new MockBindable { BindingContext = new DifferentViewModel() };
			Assert.That (() => bindable.SetBinding (MockBindable.TextProperty, new Binding ("Text")), Throws.Nothing);

			Assert.That (log.Messages.Count, Is.EqualTo (1), "An error was not logged");
			Assert.That (log.Messages[0], Is.StringContaining (String.Format (BindingExpression.PropertyNotFoundErrorMessage,
				"Text",
				"Xamarin.Forms.Core.UnitTests.BindingUnitTests+DifferentViewModel",
				"Xamarin.Forms.Core.UnitTests.MockBindable",
				"Text")));

			Assert.That (() => bindable.SetValueCore (MockBindable.TextProperty, "foo"), Throws.Nothing);
		}

		[Test]
		public void PrivateSetterTwoWay()
		{
			var bindable = new MockBindable { BindingContext = new DifferentViewModel() };
			Assert.That (() => bindable.SetBinding (MockBindable.TextProperty, new Binding ("PrivateSetter")), Throws.Nothing);

			Assert.That (log.Messages.Count, Is.EqualTo (1), "An error was not logged");
			Assert.That (log.Messages[0], Is.StringContaining (String.Format (BindingExpression.PropertyNotFoundErrorMessage,
				"PrivateSetter",
				"Xamarin.Forms.Core.UnitTests.BindingUnitTests+DifferentViewModel",
				"Xamarin.Forms.Core.UnitTests.MockBindable",
				"Text")));

			Assert.That (() => bindable.SetValueCore (MockBindable.TextProperty, "foo"), Throws.Nothing);

			Assert.That (log.Messages.Count, Is.EqualTo (2), "An error was not logged");
			Assert.That (log.Messages[1], Is.StringContaining (String.Format (BindingExpression.PropertyNotFoundErrorMessage,
				"PrivateSetter",
				"Xamarin.Forms.Core.UnitTests.BindingUnitTests+DifferentViewModel",
				"Xamarin.Forms.Core.UnitTests.MockBindable",
				"Text")));
		}

		[Test]
		public void PropertyNotFound()
		{
			var bindable = new MockBindable { BindingContext = new MockViewModel() };
			Assert.That (() => bindable.SetBinding (MockBindable.TextProperty, new Binding ("MissingProperty")), Throws.Nothing);

			Assert.That (log.Messages.Count, Is.EqualTo (1), "An error was not logged");
			Assert.That (log.Messages[0], Is.StringContaining (String.Format (BindingExpression.PropertyNotFoundErrorMessage,
				"MissingProperty",
				"Xamarin.Forms.Core.UnitTests.MockViewModel",
				"Xamarin.Forms.Core.UnitTests.MockBindable",
				"Text")));
		}

		[Test]
		[Description ("When binding with a multi-part path and part is null, no error should be thrown or logged")]
		public void ChainedPartNull()
		{
			var bindable = new MockBindable { BindingContext = new ComplexMockViewModel() };
			Assert.That (() => bindable.SetBinding (MockBindable.TextProperty, new Binding ("Model.Text")), Throws.Nothing);
			Assert.That (log.Messages.Count, Is.EqualTo (0), "An error was logged");
		}

		[Test]
		public void PropertyNotFoundChained()
		{
			var bindable = new MockBindable {
				BindingContext = new ComplexMockViewModel {
					Model = new ComplexMockViewModel()
				}
			
			};
			Assert.That (() => bindable.SetBinding (MockBindable.TextProperty, new Binding ("Model.MissingProperty")), Throws.Nothing);

			Assert.That (log.Messages.Count, Is.EqualTo (1), "An error was not logged");
			Assert.That (log.Messages[0], Is.StringContaining (String.Format (BindingExpression.PropertyNotFoundErrorMessage,
				"MissingProperty",
				"Xamarin.Forms.Core.UnitTests.BindingBaseUnitTests+ComplexMockViewModel",
				"Xamarin.Forms.Core.UnitTests.MockBindable",
				"Text")));

			Assert.That (bindable.Text, Is.EqualTo (MockBindable.TextProperty.DefaultValue));
		}

		[Test]
		public void CreateBindingNull()
		{
			Assert.That (() => Binding.Create<MockViewModel> (null), Throws.InstanceOf<ArgumentNullException>());
		}

		[Test]
		public void CreateBindingSimple()
		{
			Binding binding = Binding.Create<MockViewModel> (mvm => mvm.Text);
			Assert.IsNotNull (binding);
			Assert.AreEqual ("Text", binding.Path);
		}

		[Test]
		public void CreateBindingComplex()
		{
			Binding binding = Binding.Create<ComplexMockViewModel> (vm => vm.Model.Model.Text);
			Assert.IsNotNull (binding);
			Assert.AreEqual ("Model.Model.Text", binding.Path);
		}

		[Test]
		public void CreateBindingIndexed()
		{
			Binding binding = Binding.Create<ComplexMockViewModel> (vm => vm.Model.Model[5]);
			Assert.IsNotNull (binding);
			Assert.AreEqual ("Model.Model[5]", binding.Path);
		}

		[Test]
		public void CreateBindingIndexedNonConstant()
		{
			int x = 5;
			Assert.That (
				() => Binding.Create<ComplexMockViewModel> (vm => vm.Model.Model[x]),
				Throws.ArgumentException);
		}

		internal class ReferenceTypeIndexerViewModel
			: MockViewModel
		{
			public string this [string value]
			{
				get { return value; }
			}
		}

		[Test]
		public void CreateBindingNullToIndexer()
		{
			Assert.That (
				() => Binding.Create<ReferenceTypeIndexerViewModel> (vm => vm[null]),
				Throws.Nothing);
		}

		[Test]
		public void CreateBindingWithMethod()
		{
			Assert.That (
				() => Binding.Create<ComplexMockViewModel> (vm => vm.DoStuff()),
				Throws.ArgumentException);
		}

		[Test]
		[Description ("Indexers are seen as methods, we don't want to get them confused with real methods.")]
		public void CreateBindingWithMethodArgument()
		{
			Assert.That (
				() => Binding.Create<ComplexMockViewModel> (vm => vm.DoStuff (null)),
				Throws.ArgumentException);
		}

		object Method (MockViewModel vm)
		{
			return vm.Text;
		}

		[Test]
		public void CreateBindingMethod()
		{
			Func<MockViewModel, object> func = vm => vm.Text;
			Assert.That (() => Binding.Create<MockViewModel> (vm => func (vm)),
				Throws.ArgumentException);

			Assert.That (() => Binding.Create<MockViewModel> (vm => Method (vm)),
				Throws.ArgumentException);
		}

		[Test]
		public void CreateBindingPrivateIndexer()
		{
			Assert.That (() => Binding.Create<InternalIndexerViewModel> (vm => vm[5]),
				Throws.ArgumentException);
		}

		class InternalIndexerViewModel
		{
			internal int this [int x]
			{
				get { return x; }
			}
		}

		[Test]
		public void CreateBindingInvalidExpression()
		{
			Assert.That (() => Binding.Create<MockViewModel> (vm => vm.Text + vm.Text),
				Throws.ArgumentException);

			Assert.That (() => Binding.Create<MockViewModel> (vm => 5),
				Throws.ArgumentException);

			Assert.That (() => Binding.Create<MockViewModel> (vm => null),
				Throws.ArgumentException);
		}

		[Test]
		public void SetBindingContextBeforeContextBindingAndInnerBindings ()
		{
			var label = new Label ();
			var view = new StackLayout { Children = {label} };

			view.BindingContext = new {item0 = "Foo", item1 = "Bar"};
			label.SetBinding (BindableObject.BindingContextProperty, "item0");
			label.SetBinding (Label.TextProperty, Binding.SelfPath);

			Assert.AreEqual ("Foo", label.Text);
		}

		[Test]
		public void SetBindingContextAndInnerBindingBeforeContextBinding ()
		{
			var label = new Label ();
			var view = new StackLayout { Children = {label} };

			view.BindingContext = new {item0 = "Foo", item1 = "Bar"};
			label.SetBinding (Label.TextProperty, Binding.SelfPath);
			label.SetBinding (BindableObject.BindingContextProperty, "item0");

			Assert.AreEqual ("Foo", label.Text);
		}

		[Test]
		public void SetBindingContextAfterContextBindingAndInnerBindings ()
		{
			var label = new Label ();
			var view = new StackLayout { Children = {label} };

			label.SetBinding (BindableObject.BindingContextProperty, "item0");
			label.SetBinding (Label.TextProperty, Binding.SelfPath);
			view.BindingContext = new {item0 = "Foo", item1 = "Bar"};

			Assert.AreEqual ("Foo", label.Text);
		}

		[Test]
		public void SetBindingContextAfterInnerBindingsAndContextBinding ()
		{
			var label = new Label ();
			var view = new StackLayout { Children = {label} };

			label.SetBinding (Label.TextProperty, Binding.SelfPath);
			label.SetBinding (BindableObject.BindingContextProperty, "item0");
			view.BindingContext = new {item0 = "Foo", item1 = "Bar"};

			Assert.AreEqual ("Foo", label.Text);
		}

		[Test]
		public void Convert ()
		{
			var slider = new Slider ();
			var vm = new MockViewModel { Text = "0.5" };
			slider.BindingContext = vm;
			slider.SetBinding (Slider.ValueProperty, "Text", BindingMode.TwoWay);

			Assert.That (slider.Value, Is.EqualTo (0.5));

			slider.Value = 0.9;

			Assert.That (vm.Text, Is.EqualTo ("0.9"));
		}

		#if !WINDOWS_PHONE
		[Test]
		[SetCulture ("pt-PT")]
		[SetUICulture ("pt-PT")]
		public void ConvertIsCultureInvariant ()
		{
			var slider = new Slider ();
			var vm = new MockViewModel { Text = "0.5" };
			slider.BindingContext = vm;
			slider.SetBinding (Slider.ValueProperty, "Text", BindingMode.TwoWay);

			Assert.That (slider.Value, Is.EqualTo (0.5));

			slider.Value = 0.9;

			Assert.That (vm.Text, Is.EqualTo ("0.9"));
		}
		#endif

		[Test]
		public void FailToConvert ()
		{
			var slider = new Slider ();
			slider.BindingContext = new ComplexMockViewModel { Model = new ComplexMockViewModel() };

			Assert.That (() => {
				slider.SetBinding (Slider.ValueProperty, "Model");
			}, Throws.Nothing);

			Assert.That (slider.Value, Is.EqualTo (Slider.ValueProperty.DefaultValue));
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

			public void SignalAllPropertiesChanged (bool useNull)
			{
				var changed = PropertyChanged;
				if (changed != null)
					changed (this, new PropertyChangedEventArgs ((useNull) ? null : String.Empty));
			}
		}

		class MockBindable2 : MockBindable
		{
			public static readonly BindableProperty Text2Property = BindableProperty.Create<MockBindable2, string> (
				b => b.Text2, "default", BindingMode.TwoWay);

			public string Text2
			{
				get { return (string)GetValue (Text2Property); }
				set { SetValue (Text2Property, value); }
			}
		}

		[TestCase (true)]
		[TestCase (false)]
		public void NullPropertyUpdatesAllBindings (bool useStringEmpty)
		{
			var vm = new NullViewModel();
			var bindable = new MockBindable2();
			bindable.BindingContext = vm;
			bindable.SetBinding (MockBindable.TextProperty, "Foo");
			bindable.SetBinding (MockBindable2.Text2Property, "Bar");

			vm.Foo = "Foo";
			vm.Bar = "Bar";
			Assert.That (() => vm.SignalAllPropertiesChanged (useNull: !useStringEmpty), Throws.Nothing);

			Assert.That (bindable.Text, Is.EqualTo ("Foo"));
			Assert.That (bindable.Text2, Is.EqualTo ("Bar"));
		}

		[TestCase]
		public void BindingSourceOverContext ()
		{
			var label = new Label ();
			label.BindingContext = "bindingcontext";
			label.SetBinding (Label.TextProperty, Binding.SelfPath);
			Assert.AreEqual ("bindingcontext", label.Text);

			label.SetBinding (Label.TextProperty, new Binding (Binding.SelfPath, source: "bindingsource"));
			Assert.AreEqual ("bindingsource", label.Text);
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

			public int InvocationListSize ()
			{
				if (PropertyChanged == null)
					return 0;
				return PropertyChanged.GetInvocationList ().Length;
			}

			public virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
			}
		}

		[Test]
		public void BindingUnsubscribesForDeadTarget ()
		{
			TestViewModel viewmodel = new TestViewModel();

			int i = 0;
			Action create = null;
			create = () => {
				if (i++ < 1024) {
					create();
					return;
				}

				var button = new Button ();
				button.SetBinding (Button.TextProperty, "Foo");
				button.BindingContext = viewmodel;
			};

			create();

			Assume.That (viewmodel.InvocationListSize (), Is.EqualTo (1));

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect ();

			viewmodel.OnPropertyChanged ("Foo");

			Assert.AreEqual (0, viewmodel.InvocationListSize ());
		}

		[Test]
		public async Task BindingDoesNotStayAliveForDeadTarget()
		{
			TestViewModel viewModel = new TestViewModel();
			WeakReference bindingRef;

			{ 
				var binding = new Binding("Foo");

				var button = new Button();
				button.SetBinding(Button.TextProperty, binding);
				button.BindingContext = viewModel;

				bindingRef = new WeakReference(binding);
			}

			Assume.That(viewModel.InvocationListSize(), Is.EqualTo(1));

			//NOTE: this was the only way I could "for sure" get the binding to get GC'd
			GC.Collect();
			await Task.Delay(10);
			GC.Collect();

			Assert.IsFalse(bindingRef.IsAlive, "Binding should not be alive!");
		}

		[Test]
		public void BindingCreatesSingleSubscription ()
		{
			TestViewModel viewmodel = new TestViewModel();

			var button = new Button ();
			button.SetBinding (Button.TextProperty, "Foo");
			button.BindingContext = viewmodel;

			Assert.That (viewmodel.InvocationListSize (), Is.EqualTo (1));
		}

		public class IndexedViewModel : INotifyPropertyChanged
		{
			Dictionary<string, object> dict = new Dictionary<string, object> ();
			 
			public object this [string index]
			{
				get { return dict[index]; }
				set
				{
					dict[index] = value;
					OnPropertyChanged ("Item[" + index + "]");

				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
			}
		}

		[Test]
		public void IndexedViewModelPropertyChanged ()
		{
			var label = new Label ();
			var viewModel = new IndexedViewModel ();
			//viewModel["Foo"] = "Bar";

			label.BindingContext = new {
				Data = viewModel
			};
			label.SetBinding (Label.TextProperty, "Data[Foo]");
			

			Assert.AreEqual (null, label.Text);

			viewModel["Foo"] = "Baz";

			Assert.AreEqual ("Baz", label.Text);
		}
	}
}