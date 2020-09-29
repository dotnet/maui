using System;
using System.Globalization;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TypeConverter(typeof(ToBarConverter))]
	internal class Bar
	{

	}

	internal class Baz
	{

	}

	internal class MockBindable
		: VisualElement
	{
		public static readonly BindableProperty TextProperty = BindableProperty.Create<MockBindable, string>(
			b => b.Text, "default", BindingMode.TwoWay);

		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public string Foo { get; set; }

		public int TargetInt { get; set; }

		public static readonly BindableProperty BarProperty =
			BindableProperty.Create<MockBindable, Bar>(w => w.Bar, default(Bar));

		public Bar Bar
		{
			get { return (Bar)GetValue(BarProperty); }
			set { SetValue(BarProperty, value); }
		}

		public static readonly BindableProperty BazProperty =
			BindableProperty.Create<MockBindable, Baz>(w => w.Baz, default(Baz));

		[TypeConverter(typeof(ToBazConverter))]
		public Baz Baz
		{
			get { return (Baz)GetValue(BazProperty); }
			set { SetValue(BazProperty, value); }
		}

		public static readonly BindableProperty QuxProperty =
			BindableProperty.Create<MockBindable, Baz>(w => w.Qux, default(Baz));

		public Baz Qux
		{
			get { return (Baz)GetValue(QuxProperty); }
			set { SetValue(QuxProperty, value); }
		}
	}

	internal class ToBarConverter : TypeConverter
	{
		public override object ConvertFrom(System.Globalization.CultureInfo culture, object value)
		{
			return new Bar();
		}
	}

	internal class ToBazConverter : TypeConverter
	{
		public override object ConvertFrom(System.Globalization.CultureInfo culture, object value)
		{
			return new Baz();
		}
	}

	[TestFixture]
	public class BindableObjectUnitTests : BaseTestFixture
	{
		[SetUp]
		public void Setup()
		{
			Device.PlatformServices = new MockPlatformServices();
		}

		[TearDown]
		public void TearDown()
		{
			Device.PlatformServices = null;
		}

		[Test]
		public void BindingContext()
		{
			var mock = new MockBindable();
			Assert.IsNull(mock.BindingContext);

			object obj = new object();
			mock.BindingContext = obj;
			Assert.AreSame(obj, mock.BindingContext);
		}

		[Test]
		public void BindingContextChangedEvent()
		{
			var mock = new MockBindable();
			mock.BindingContextChanged += (sender, args) => Assert.Pass();

			mock.BindingContext = new object();

			Assert.Fail("The BindingContextChanged event was not fired.");
		}

		[Test]
		public void BindingContextChangedOnce()
		{
			var count = 0;

			var mock = new MockBindable();
			mock.BindingContextChanged += (sender, args) => ++count;

			mock.BindingContext = new object();
			Assert.AreEqual(count, 1);
		}

		class MockVMEquals
		{
			public string Key { get; set; }
			public string Text { get; set; }
			public override bool Equals(object obj)
			{
				var other = obj as MockVMEquals;
				if (other == null)
					return false;
				return Key == other.Key;
			}

			public override int GetHashCode()
			{
				return base.GetHashCode();
			}
		}

		[Test]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=59507
		public void BindingContextChangedCompareReferences()
		{
			var mock = new MockBindable();
			mock.BindingContext = new MockVMEquals { Key = "Foo", Text = "Foo" };
			mock.BindingContextChanged += (sender, args) => Assert.Pass();

			mock.BindingContext = new MockVMEquals { Key = "Foo", Text = "Bar" };

			Assert.Fail("The BindingContextChanged event was not fired.");
		}

		[Test]
		public void ParentAndChildBindingContextChanged()
		{
			int parentCount = 0, childCount = 0;

			var didNotChange = " BindingContext did not change.";
			var changedMoreThanOnce = " BindingContext changed more than once.";
			var changedWhenNoChange = " BindingContext was changed when there was no change in context.";

			var parent = new MockBindable();
			parent.BindingContextChanged += (sender, args) => { ++parentCount; };

			var child = new MockBindable();
			child.BindingContextChanged += (sender, args) => { ++childCount; };

			child.Parent = parent;
			Assert.AreEqual(parentCount, 0, "Parent BindingContext was changed while parenting a child.");
			Assert.AreEqual(childCount, 0, "Child" + changedWhenNoChange);

			child.BindingContext = new object(); // set manually
			Assert.GreaterOrEqual(childCount, 1, "Child" + didNotChange);
			Assert.AreEqual(childCount, 1, "Child" + changedMoreThanOnce);
			Assert.AreEqual(parentCount, 0, "Parent" + changedWhenNoChange);

			parent.BindingContext = new object();
			Assert.GreaterOrEqual(parentCount, 1, "Parent" + didNotChange);
			Assert.AreEqual(parentCount, 1, "Parent" + changedMoreThanOnce);
			Assert.AreEqual(childCount, 1, "Child" + changedWhenNoChange);

			child.BindingContext = new object();
			Assert.GreaterOrEqual(childCount, 2, "Child" + didNotChange);
			Assert.AreEqual(childCount, 2, "Child" + changedMoreThanOnce);
			Assert.AreEqual(parentCount, 1, "Parent" + changedWhenNoChange);
		}

		[Test]
		public void ParentSetOnNullChildBindingContext()
		{
			var parent = new MockBindable();

			var child = new MockBindable();
			child.BindingContextChanged += (sender, args) => { Assert.Fail("Child BindingContext was changed when there was no change in context."); };

			child.Parent = parent; // this should not trigger binding context change on child since there is no change
			parent.BindingContext = new object();
			parent.BindingContext = new object();
		}

		[Test]
		public void ParentSetOnNonNullChildBindingContext()
		{
			var count = 0;

			var parent = new MockBindable();
			parent.BindingContextChanged += (sender, args) => { ++count; };

			var child = new MockBindable();
			child.BindingContextChanged += (sender, args) => { ++count; };

			child.BindingContext = new object(); // set manually
			Assert.AreEqual(count, 1);

			child.Parent = parent; // this should not trigger binding context change because child binding was set manually
			Assert.AreEqual(count, 1);

			parent.BindingContext = new object();
			Assert.AreEqual(count, 2);

			child.BindingContext = new object();
			Assert.AreEqual(count, 3);
		}

		[Test]
		[Description("When the BindingContext changes, any bindings should be immediately applied.")]
		public void BindingContextChangedBindingsApplied()
		{
			var mock = new MockBindable();
			mock.SetBinding(MockBindable.TextProperty, new Binding("."));
			mock.BindingContext = "Test";

			Assert.AreEqual("Test", mock.GetValue(MockBindable.TextProperty));

			mock.BindingContext = "Testing";

			Assert.AreEqual("Testing", mock.GetValue(MockBindable.TextProperty),
				"Bindings were not reapplied to the new binding context");
		}

		[Test]
		[Description("When the BindingContext changes, the new context needs to listen for updates.")]
		public void BindingContextChangedBindingsListening()
		{
			var mock = new MockBindable();
			mock.SetBinding(MockBindable.TextProperty, new Binding("Text"));

			var vm = new MockViewModel();
			mock.BindingContext = vm;

			mock.BindingContext = (vm = new MockViewModel());

			vm.Text = "test";

			Assert.AreEqual("test", mock.GetValue(MockBindable.TextProperty),
				"The new ViewModel was not being listened to after being set");
		}

		[Test]
		[Description("When an INPC implementer is unset as the BindingContext, its changes shouldn't be listened to any further.")]
		public void BindingContextUnsetStopsListening()
		{
			var mock = new MockBindable();
			mock.SetBinding(MockBindable.TextProperty, new Binding("Text"));

			var vm = new MockViewModel();
			mock.BindingContext = vm;

			mock.BindingContext = null;

			vm.Text = "test";

			Assert.IsNull(mock.GetValue(Entry.TextProperty), "ViewModel was still being listened to after set to null");
		}

		[Test]
		public void PropertyChanging()
		{
			var mock = new MockBindable();
			mock.PropertyChanging += (sender, args) =>
			{
				Assert.AreEqual(MockBindable.TextProperty.PropertyName, args.PropertyName);
				Assert.AreEqual(MockBindable.TextProperty.DefaultValue, mock.GetValue(MockBindable.TextProperty));
				Assert.Pass();
			};

			mock.SetValue(MockBindable.TextProperty, "foo");

			Assert.Fail("The PropertyChanging event was not fired.");
		}

		[Test]
		public void PropertyChangingSameValue()
		{
			const string value = "foo";

			var mock = new MockBindable();
			mock.SetValue(MockBindable.TextProperty, value);
			mock.PropertyChanging += (s, e) => Assert.Fail();

			mock.SetValue(MockBindable.TextProperty, value);

			Assert.Pass();
		}

		[Test]
		public void PropertyChangingDefaultValue()
		{
			var prop = BindableProperty.Create<MockBindable, string>(w => w.Foo, "DefaultValue");

			var mock = new MockBindable();
			mock.PropertyChanging += (s, e) => Assert.Fail();
			mock.SetValue(prop, prop.DefaultValue);

			Assert.Pass();
		}

		[Test]
		public void PropertyChanged()
		{
			const string value = "foo";

			var mock = new MockBindable();
			mock.PropertyChanged += (sender, args) =>
			{
				Assert.AreEqual(MockBindable.TextProperty.PropertyName, args.PropertyName);
				Assert.AreEqual(value, mock.GetValue(MockBindable.TextProperty));
				Assert.Pass();
			};

			mock.SetValue(MockBindable.TextProperty, value);

			Assert.Fail("The PropertyChanged event was not fired.");
		}

		[Test]
		public void PropertyChangedSameValue()
		{
			const string value = "foo";

			var mock = new MockBindable();
			mock.SetValue(MockBindable.TextProperty, value);
			mock.PropertyChanged += (s, e) => Assert.Fail();

			mock.SetValue(MockBindable.TextProperty, value);

			Assert.Pass();
		}

		[Test]
		public void PropertyChangedDefaultValue()
		{
			var prop = BindableProperty.Create<MockBindable, string>(w => w.Foo, "DefaultValue");

			var mock = new MockBindable();
			mock.PropertyChanged += (s, e) => Assert.Fail();

			mock.SetValue(prop, prop.DefaultValue);

			Assert.Pass();
		}

		[Test]
		public void GetSetValue()
		{
			const string value = "foo";
			var mock = new MockBindable();
			mock.SetValue(MockBindable.TextProperty, value);

			Assert.AreEqual(value, mock.GetValue(MockBindable.TextProperty));
		}

		[Test]
		public void GetValueDefault()
		{
			var nulldefault = BindableProperty.Create<MockBindable, string>(w => w.Foo, null);
			TestGetValueDefault(nulldefault);

			var foodefault = BindableProperty.Create<MockBindable, string>(w => w.Foo, "Foo");
			TestGetValueDefault(foodefault);
		}

		void TestGetValueDefault(BindableProperty property)
		{
			var mock = new MockBindable();
			object value = mock.GetValue(property);
			Assert.AreEqual(property.DefaultValue, value);
		}

		[Test]
		public void SetValueInvalid()
		{
			var mock = new MockBindable();
			Assert.Throws<ArgumentNullException>(() => mock.SetValue((BindableProperty)null, "null"));
		}

		[Test]
		public void GetValueInvalid()
		{
			var mock = new MockBindable();
			Assert.Throws<ArgumentNullException>(() => mock.GetValue(null));
		}

		[Test]
		public void ClearValueInvalid()
		{
			var mock = new MockBindable();
			Assert.Throws<ArgumentNullException>(() => mock.ClearValue((BindableProperty)null));
		}

		[Test]
		public void ClearValue()
		{
			const string value = "foo";
			var mock = new MockBindable();
			mock.SetValue(MockBindable.TextProperty, value);
			Assert.AreEqual(value, mock.GetValue(MockBindable.TextProperty));

			mock.ClearValue(MockBindable.TextProperty);
			TestGetValueDefault(MockBindable.TextProperty);
		}

		[Test]
		public void ClearValueTriggersINPC()
		{
			var bindable = new MockBindable();
			bool changingfired = false;
			bool changedfired = false;
			bool changingdelegatefired = false;
			bool changeddelegatefired = false;
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), "foo",
				propertyChanged: (b, o, n) => changeddelegatefired = true,
				propertyChanging: (b, o, n) => changingdelegatefired = true
			);
			bindable.PropertyChanged += (sender, e) => { changedfired |= e.PropertyName == "Foo"; };
			bindable.PropertyChanging += (sender, e) => { changingfired |= e.PropertyName == "Foo"; };

			bindable.SetValue(property, "foobar");
			changingfired = changedfired = changeddelegatefired = changingdelegatefired = false;

			bindable.ClearValue(property);
			Assert.True(changingfired);
			Assert.True(changedfired);
			Assert.True(changingdelegatefired);
			Assert.True(changeddelegatefired);
		}

		[Test]
		public void ClearValueDoesNotTriggersINPCOnSameValues()
		{
			var bindable = new MockBindable();
			bool changingfired = false;
			bool changedfired = false;
			bool changingdelegatefired = false;
			bool changeddelegatefired = false;
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), "foo",
				propertyChanged: (b, o, n) => changeddelegatefired = true,
				propertyChanging: (b, o, n) => changingdelegatefired = true
			);
			bindable.PropertyChanged += (sender, e) => { changedfired |= e.PropertyName == "Foo"; };
			bindable.PropertyChanging += (sender, e) => { changingfired |= e.PropertyName == "Foo"; };

			bindable.SetValue(property, "foobar");
			bindable.SetValue(property, "foo");
			changingfired = changedfired = changeddelegatefired = changingdelegatefired = false;

			bindable.ClearValue(property);
			Assert.False(changingfired);
			Assert.False(changedfired);
			Assert.False(changingdelegatefired);
			Assert.False(changeddelegatefired);
		}

		[Test]
		public void SetBindingInvalid()
		{
			var mock = new MockBindable();
			Assert.Throws<ArgumentNullException>(() => mock.SetBinding(null, new Binding(".")));
			Assert.Throws<ArgumentNullException>(() => mock.SetBinding(MockBindable.TextProperty, null));
		}

		[Test]
		public void RemoveUnaddedBinding()
		{
			var mock = new MockBindable();
			Assert.That(() => mock.RemoveBinding(MockBindable.TextProperty), Throws.Nothing);
		}

		[Test]
		public void RemoveBindingInvalid()
		{
			var mock = new MockBindable();
			Assert.Throws<ArgumentNullException>(() => mock.RemoveBinding(null));
		}

		[Test]
		public void RemovedBindingDoesNotUpdate()
		{
			const string newvalue = "New Value";
			var viewmodel = new MockViewModel
			{
				Text = "Foo"
			};

			var binding = new Binding("Text");

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(MockBindable.TextProperty, binding);

			string original = (string)bindable.GetValue(MockBindable.TextProperty);

			bindable.RemoveBinding(MockBindable.TextProperty);

			viewmodel.Text = newvalue;
			Assert.AreEqual(original, bindable.GetValue(MockBindable.TextProperty),
				"Property updated from a removed binding");
		}

		[Test]
		public void CoerceValue()
		{
			var property = BindableProperty.Create<MockBindable, string>(w => w.Foo, null,
				coerceValue: (bo, o) => o.ToUpper());

			const string value = "value";
			var mock = new MockBindable();
			mock.SetValue(property, value);
			Assert.AreEqual(value.ToUpper(), mock.GetValue(property));
		}

		[Test]
		public void ValidateValue()
		{
			var property = BindableProperty.Create<MockBindable, string>(w => w.Foo, null,
				validateValue: (b, v) => false);

			var mock = new MockBindable();
			Assert.Throws<ArgumentException>(() => mock.SetValue(property, null));
		}

		[Test]
		public void BindablePropertyChanged()
		{
			bool changed = false;

			string oldv = "bar";
			string newv = "foo";

			var property = BindableProperty.Create<MockBindable, string>(w => w.Foo, oldv,
				propertyChanged: (b, ov, nv) =>
				{
					Assert.AreSame(oldv, ov);
					Assert.AreSame(newv, nv);
					changed = true;
				});

			var mock = new MockBindable();
			mock.SetValue(property, newv);

			Assert.IsTrue(changed, "PropertyChanged was not called");
		}

		[Test]
		public void RecursiveChange()
		{
			bool changedA1 = false, changedA2 = false, changedB1 = false, changedB2 = false;

			var mock = new MockBindable();
			mock.PropertyChanged += (sender, args) =>
			{
				if (!changedA1)
				{
					Assert.AreEqual("1", mock.GetValue(MockBindable.TextProperty));
					Assert.IsFalse(changedA2);
					Assert.IsFalse(changedB1);
					Assert.IsFalse(changedB2);
					mock.SetValue(MockBindable.TextProperty, "2");
					changedA1 = true;
				}
				else
				{
					Assert.AreEqual("2", mock.GetValue(MockBindable.TextProperty));
					Assert.IsFalse(changedA2);
					Assert.IsTrue(changedB1);
					Assert.IsFalse(changedB2);
					changedA2 = true;
				}
			};
			mock.PropertyChanged += (sender, args) =>
			{
				if (!changedB1)
				{
					Assert.AreEqual("1", mock.GetValue(MockBindable.TextProperty));
					Assert.IsTrue(changedA1);
					Assert.IsFalse(changedA2);
					Assert.IsFalse(changedB2);
					changedB1 = true;
				}
				else
				{
					Assert.AreEqual("2", mock.GetValue(MockBindable.TextProperty));
					Assert.IsTrue(changedA1);
					Assert.IsTrue(changedA2);
					Assert.IsFalse(changedB2);
					changedB2 = true;
				}
			};
			mock.SetValue(MockBindable.TextProperty, "1");
			Assert.AreEqual("2", mock.GetValue(MockBindable.TextProperty));
			Assert.IsTrue(changedA1);
			Assert.IsTrue(changedA2);
			Assert.IsTrue(changedB1);
			Assert.IsTrue(changedB2);
		}

		[Test]
		public void RaiseOnEqual()
		{
			string foo = "foo";
			var mock = new MockBindable();
			mock.SetValue(MockBindable.TextProperty, foo);

			bool changing = false;
			mock.PropertyChanging += (o, e) =>
			{
				Assert.That(e.PropertyName, Is.EqualTo(MockBindable.TextProperty.PropertyName));
				changing = true;
			};

			bool changed = true;
			mock.PropertyChanged += (o, e) =>
			{
				Assert.That(e.PropertyName, Is.EqualTo(MockBindable.TextProperty.PropertyName));
				changed = true;
			};

			mock.SetValueCore(MockBindable.TextProperty, foo,
				SetValueFlags.ClearOneWayBindings | SetValueFlags.ClearDynamicResource | SetValueFlags.RaiseOnEqual);

			Assert.That(changing, Is.True, "PropertyChanging event did not fire");
			Assert.That(changed, Is.True, "PropertyChanged event did not fire");
		}

		[Test]
		public void BindingContextGetter()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };

			view.BindingContext = new { item0 = "Foo", item1 = "Bar" };
			label.SetBinding(BindableObject.BindingContextProperty, "item0");
			label.SetBinding(Label.TextProperty, Binding.SelfPath);

			Assert.AreSame(label.BindingContext, label.GetValue(BindableObject.BindingContextProperty));
		}

		[Test]
		public void BoundBindingContextUpdate()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };
			var vm = new MockViewModel { Text = "FooBar" };

			view.BindingContext = vm;
			label.SetBinding(BindableObject.BindingContextProperty, "Text");
			label.SetBinding(Label.TextProperty, Binding.SelfPath);

			Assert.AreEqual("FooBar", label.BindingContext);

			vm.Text = "Baz";
			Assert.AreEqual("Baz", label.BindingContext);
		}

		[Test]
		public void BoundBindingContextChange()
		{
			var label = new Label();
			var view = new StackLayout { Children = { label } };

			view.BindingContext = new MockViewModel { Text = "FooBar" };
			;
			label.SetBinding(BindableObject.BindingContextProperty, "Text");
			label.SetBinding(Label.TextProperty, Binding.SelfPath);

			Assert.AreEqual("FooBar", label.BindingContext);

			view.BindingContext = new MockViewModel { Text = "Baz" };
			;
			Assert.AreEqual("Baz", label.BindingContext);
		}

		[Test]
		public void TestReadOnly()
		{
			var bindablePropertyKey = BindableProperty.CreateReadOnly<MockBindable, string>(w => w.Foo, "DefaultValue");
			var bindableProperty = bindablePropertyKey.BindableProperty;

			var bindable = new MockBindable();
			Assert.AreEqual("DefaultValue", bindable.GetValue(bindableProperty));

			bindable.SetValue(bindablePropertyKey, "Bar");
			Assert.AreEqual("Bar", bindable.GetValue(bindableProperty));

			Assert.Throws<InvalidOperationException>(() => bindable.SetValue(bindableProperty, "Baz"));
			Assert.AreEqual("Bar", bindable.GetValue(bindableProperty));

			Assert.Throws<InvalidOperationException>(() => bindable.ClearValue(bindableProperty));

			bindable.ClearValue(bindablePropertyKey);
			Assert.AreEqual("DefaultValue", bindable.GetValue(bindableProperty));
		}

		[Test]
		public void TestBindingTwoWayOnReadOnly()
		{
			var bindablePropertyKey = BindableProperty.CreateReadOnly<MockBindable, string>(w => w.Foo, "DefaultValue", BindingMode.OneWayToSource);
			var bindableProperty = bindablePropertyKey.BindableProperty;

			var bindable = new MockBindable();
			var vm = new MockViewModel();

			bindable.SetBinding(bindableProperty, new Binding("Text", BindingMode.TwoWay));
			Assert.DoesNotThrow(() => bindable.BindingContext = vm);

			Assert.AreEqual("DefaultValue", bindable.GetValue(bindableProperty));
		}

		[Test]
		public void DefaultValueCreator()
		{
			int invoked = 0;
			object defaultValue = new object();
			var bindableProperty = BindableProperty.Create("Foo", typeof(object), typeof(MockBindable), defaultValue, defaultValueCreator: o =>
			{
				invoked++;
				return new object();
			});
			var bindable = new MockBindable();

			Assert.AreSame(defaultValue, bindableProperty.DefaultValue);
			var newvalue = bindable.GetValue(bindableProperty);
			Assert.AreNotSame(defaultValue, newvalue);
			Assert.NotNull(newvalue);
			Assert.AreEqual(1, invoked);
		}

		[Test]
		public void DefaultValueCreatorIsInvokedOnlyAtFirstTime()
		{
			int invoked = 0;
			var bindableProperty = BindableProperty.Create("Foo", typeof(object), typeof(MockBindable), null, defaultValueCreator: o =>
			{
				invoked++;
				return new object();
			});
			var bindable = new MockBindable();

			var value0 = bindable.GetValue(bindableProperty);
			var value1 = bindable.GetValue(bindableProperty);
			Assert.NotNull(value0);
			Assert.NotNull(value1);
			Assert.AreSame(value0, value1);
			Assert.AreEqual(1, invoked);
		}

		[Test]
		public void DefaultValueCreatorNotSharedAccrossInstances()
		{
			int invoked = 0;
			var bindableProperty = BindableProperty.Create("Foo", typeof(object), typeof(MockBindable), null, defaultValueCreator: o =>
			{
				invoked++;
				return new object();
			});
			var bindable0 = new MockBindable();
			var bindable1 = new MockBindable();
			var value0 = bindable0.GetValue(bindableProperty);
			var value1 = bindable1.GetValue(bindableProperty);

			Assert.AreNotSame(value0, value1);
			Assert.AreEqual(2, invoked);
		}

		[Test]
		public void DefaultValueCreatorInvokedAfterClearValue()
		{
			int invoked = 0;
			var bindableProperty = BindableProperty.Create("Foo", typeof(object), typeof(MockBindable), null, defaultValueCreator: o =>
			{
				invoked++;
				return new object();
			});
			var bindable = new MockBindable();

			Assert.AreEqual(0, invoked);

			var value0 = bindable.GetValue(bindableProperty);
			Assert.NotNull(value0);
			Assert.AreEqual(1, invoked);
			bindable.ClearValue(bindableProperty);

			var value1 = bindable.GetValue(bindableProperty);
			Assert.NotNull(value1);
			Assert.AreEqual(2, invoked);
			Assert.AreNotSame(value0, value1);
		}

		[Test]
		public void DefaultValueCreatorOnlyInvokedOnGetValue()
		{
			int invoked = 0;
			var bindableProperty = BindableProperty.Create("Foo", typeof(object), typeof(MockBindable), null, defaultValueCreator: o =>
			{
				invoked++;
				return new object();
			});
			var bindable = new MockBindable();

			Assert.AreEqual(0, invoked);

			var newvalue = bindable.GetValue(bindableProperty);
			Assert.NotNull(newvalue);
			Assert.AreEqual(1, invoked);
		}

		[Test]
		public void DefaultValueCreatorDoesNotTriggerINPC()
		{
			int invoked = 0;
			int propertychanged = 0;
			int changedfired = 0;
			var bindableProperty = BindableProperty.Create("Foo", typeof(object), typeof(MockBindable), null,
				propertyChanged: (bindable, oldvalue, newvalue) =>
				{
					propertychanged++;
				},
				defaultValueCreator: o =>
				{
					invoked++;
					return new object();
				});

			var bp = new MockBindable();
			bp.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == "Foo")
					changedfired++;
			};
			var value0 = bp.GetValue(bindableProperty);
			Assert.NotNull(value0);
			Assert.AreEqual(1, invoked);
			Assert.AreEqual(0, propertychanged);
			Assert.AreEqual(0, changedfired);

		}

		[Test]
		public void IsSetIsFalseWhenPropNotSet()
		{
			string defaultValue = "default";
			var bindableProperty = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), defaultValue);
			var bindable = new MockBindable();

			Assert.AreEqual(defaultValue, bindableProperty.DefaultValue);

			var isSet = bindable.IsSet(bindableProperty);
			Assert.IsFalse(isSet);
		}

		[Test]
		public void IsSetIsTrueWhenPropSet()
		{
			string defaultValue = "default";
			string newValue = "new value";

			var bindableProperty = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), defaultValue);
			var bindable = new MockBindable();

			Assert.AreEqual(defaultValue, bindableProperty.DefaultValue);

			bindable.SetValue(bindableProperty, newValue);

			var isSet = bindable.IsSet(bindableProperty);
			Assert.IsTrue(isSet);
		}

		[Test]
		public void IsSetIsFalseWhenPropCleared()
		{
			string defaultValue = "default";
			string newValue = "new value";

			var bindableProperty = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), defaultValue);
			var bindable = new MockBindable();

			bindable.SetValue(bindableProperty, newValue);
			bindable.ClearValue(bindableProperty);

			Assert.That(bindable.IsSet(bindableProperty), Is.False);
		}

		[Test]
		public void IsSetIsTrueWhenPropSetToDefault()
		{
			string defaultValue = "default";

			var bindableProperty = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), defaultValue);
			var bindable = new MockBindable();

			Assert.AreEqual(defaultValue, bindableProperty.DefaultValue);

			bindable.SetValue(bindableProperty, defaultValue);

			var isSet = bindable.IsSet(bindableProperty);
			Assert.IsTrue(isSet);
		}

		[Test]
		public void IsSetIsTrueWhenPropSetByDefaultValueCreator()
		{
			string defaultValue = "default";
			string defaultValueC = "defaultVC";

			var bindableProperty = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), defaultValue, defaultValueCreator: o =>
			{
				return defaultValueC;
			});
			var bindable = new MockBindable();

			Assert.AreEqual(defaultValue, bindableProperty.DefaultValue);

			var created = bindable.GetValue(bindableProperty);
			Assert.AreEqual(defaultValueC, created);

			var isSet = bindable.IsSet(bindableProperty);
			Assert.IsTrue(isSet);
		}

		[Test]
		public void StyleValueIsOverridenByValue()
		{
			var label = new Label();
			label.SetValue(Label.TextProperty, "Foo", true);
			Assert.AreEqual("Foo", label.Text);

			label.SetValue(Label.TextProperty, "Bar");
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleBindingIsOverridenByValue()
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, new Binding("foo"), true);
			label.BindingContext = new { foo = "Foo" };
			Assert.AreEqual("Foo", label.Text);

			label.SetValue(Label.TextProperty, "Bar");
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleDynResourceIsOverridenByValue()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo", true);
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"}
			};
			Assert.AreEqual("Foo", label.Text);

			label.SetValue(Label.TextProperty, "Bar");
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleValueIsOverridenByBinding()
		{
			var label = new Label();
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetValue(Label.TextProperty, "Foo", true);
			Assert.AreEqual("Foo", label.Text);

			label.SetBinding(Label.TextProperty, "bar");
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleBindingIsOverridenByBinding()
		{
			var label = new Label();
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetBinding(Label.TextProperty, new Binding("foo"), true);
			Assert.AreEqual("Foo", label.Text);

			label.SetBinding(Label.TextProperty, "bar");
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleDynResourceIsOverridenByBinding()
		{
			var label = new Label();
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetDynamicResource(Label.TextProperty, "foo", true);
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
			};
			Assert.AreEqual("Foo", label.Text);

			label.SetBinding(Label.TextProperty, "bar");
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleValueIsOverridenByDynResource()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
				{"bar", "Bar"}
			};
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetValue(Label.TextProperty, "Foo", true);
			Assert.AreEqual("Foo", label.Text);

			label.SetDynamicResource(Label.TextProperty, "bar");
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleBindingIsOverridenByDynResource()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
				{"bar", "Bar"}
			};
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetBinding(Label.TextProperty, new Binding("foo"), true);
			Assert.AreEqual("Foo", label.Text);

			label.SetDynamicResource(Label.TextProperty, "bar");
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleDynResourceIsOverridenByDynResource()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
				{"bar", "Bar"}
			};
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetDynamicResource(Label.TextProperty, "foo", true);

			Assert.AreEqual("Foo", label.Text);

			label.SetDynamicResource(Label.TextProperty, "bar");
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void ValueIsPreservedOnStyleValue()
		{
			var label = new Label();
			label.SetValue(Label.TextProperty, "Foo");
			Assert.AreEqual("Foo", label.Text);

			label.SetValue(Label.TextProperty, "Bar", true);
			Assert.AreEqual("Foo", label.Text);
		}
		[Test]
		public void BindingIsPreservedOnStyleValue()
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, new Binding("foo"));
			label.BindingContext = new { foo = "Foo" };
			Assert.AreEqual("Foo", label.Text);

			label.SetValue(Label.TextProperty, "Bar", true);
			Assert.AreEqual("Foo", label.Text);
		}
		[Test]
		public void DynResourceIsPreservedOnStyleValue()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo");
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"}
			};
			Assert.AreEqual("Foo", label.Text);

			label.SetValue(Label.TextProperty, "Bar", true);
			Assert.AreEqual("Foo", label.Text);
		}

		[Test]
		public void ValueIsPreservedOnStyleBinding()
		{
			var label = new Label();
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetValue(Label.TextProperty, "Foo");
			Assert.AreEqual("Foo", label.Text);

			label.SetBinding(Label.TextProperty, new Binding("bar"), true);
			Assert.AreEqual("Foo", label.Text);
		}

		[Test]
		public void BindingIsPreservedOnStyleBinding()
		{
			var label = new Label();
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetBinding(Label.TextProperty, new Binding("foo"));
			Assert.AreEqual("Foo", label.Text);

			label.SetBinding(Label.TextProperty, new Binding("bar"), true);
			Assert.AreEqual("Foo", label.Text);
		}

		[Test]
		public void DynResourceIsPreservedOnStyleBinding()
		{
			var label = new Label();
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetDynamicResource(Label.TextProperty, "foo");
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
			};
			Assert.AreEqual("Foo", label.Text);

			label.SetBinding(Label.TextProperty, new Binding("bar"), true);
			Assert.AreEqual("Foo", label.Text);
		}

		[Test]
		public void ValueIsPreservedOnStyleDynResource()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
				{"bar", "Bar"}
			};
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetValue(Label.TextProperty, "Foo");
			Assert.AreEqual("Foo", label.Text);

			label.SetDynamicResource(Label.TextProperty, "bar", true);
			Assert.AreEqual("Foo", label.Text);
		}

		[Test]
		public void BindingIsPreservedOnStyleDynResource()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
				{"bar", "Bar"}
			};
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetBinding(Label.TextProperty, new Binding("foo"));
			Assert.AreEqual("Foo", label.Text);

			label.SetDynamicResource(Label.TextProperty, "bar", true);
			Assert.AreEqual("Foo", label.Text);
		}

		[Test]
		public void DynResourceIsPreservedOnStyleDynResource()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
				{"bar", "Bar"}
			};
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetDynamicResource(Label.TextProperty, "foo");

			Assert.AreEqual("Foo", label.Text);

			label.SetDynamicResource(Label.TextProperty, "bar", true);
			Assert.AreEqual("Foo", label.Text);
		}

		[Test]
		public void StyleValueIsOverridenByStyleValue()
		{
			var label = new Label();
			label.SetValue(Label.TextProperty, "Foo", true);
			Assert.AreEqual("Foo", label.Text);

			label.SetValue(Label.TextProperty, "Bar", true);
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleBindingIsOverridenByStyleValue()
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, new Binding("foo"), true);
			label.BindingContext = new { foo = "Foo" };
			Assert.AreEqual("Foo", label.Text);

			label.SetValue(Label.TextProperty, "Bar", true);
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleDynResourceIsOverridenByStyleValue()
		{
			var label = new Label();
			label.SetDynamicResource(Label.TextProperty, "foo", true);
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"}
			};
			Assert.AreEqual("Foo", label.Text);

			label.SetValue(Label.TextProperty, "Bar", true);
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleValueIsOverridenByStyleBinding()
		{
			var label = new Label();
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetValue(Label.TextProperty, "Foo", true);
			Assert.AreEqual("Foo", label.Text);

			label.SetBinding(Label.TextProperty, new Binding("bar"), true);
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleBindingIsOverridenByStyleBinding()
		{
			var label = new Label();
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetBinding(Label.TextProperty, new Binding("foo"), true);
			Assert.AreEqual("Foo", label.Text);

			label.SetBinding(Label.TextProperty, new Binding("bar"), true);
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleDynResourceIsOverridenByStyleBinding()
		{
			var label = new Label();
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetDynamicResource(Label.TextProperty, "foo", true);
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
			};
			Assert.AreEqual("Foo", label.Text);

			label.SetBinding(Label.TextProperty, new Binding("bar"), true);
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleValueIsOverridenByStyleDynResource()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
				{"bar", "Bar"}
			};
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetValue(Label.TextProperty, "Foo", true);
			Assert.AreEqual("Foo", label.Text);

			label.SetDynamicResource(Label.TextProperty, "bar", true);
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleBindingIsOverridenByStyleDynResource()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
				{"bar", "Bar"}
			};
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetBinding(Label.TextProperty, new Binding("foo"), true);
			Assert.AreEqual("Foo", label.Text);

			label.SetDynamicResource(Label.TextProperty, "bar", true);
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void StyleDynResourceIsOverridenByStyleDynResource()
		{
			var label = new Label();
			label.Resources = new ResourceDictionary {
				{"foo", "Foo"},
				{"bar", "Bar"}
			};
			label.BindingContext = new { foo = "Foo", bar = "Bar" };
			label.SetDynamicResource(Label.TextProperty, "foo", true);

			Assert.AreEqual("Foo", label.Text);

			label.SetDynamicResource(Label.TextProperty, "bar", true);
			Assert.AreEqual("Bar", label.Text);
		}

		[Test]
		public void SetValueCoreImplicitelyCastBasicType()
		{
			var prop = BindableProperty.Create("Foo", typeof(int), typeof(MockBindable), 0);
			var bindable = new MockBindable();

			Assert.DoesNotThrow(() => bindable.SetValue(prop, (object)(short)42));
			Assert.AreEqual(42, bindable.GetValue(prop));

			bindable.SetValue(prop, (object)(long)-42);
			Assert.AreNotEqual(-42, bindable.GetValue(prop));
		}

		class CastFromString
		{
			public string Result { get; private set; }
			public static implicit operator CastFromString(string source)
			{
				var o = new CastFromString();
				o.Result = source;
				return o;
			}
		}

		[Test]
		public void SetValueCoreInvokesOpImplicitOnPropertyType()
		{
			var prop = BindableProperty.Create("Foo", typeof(CastFromString), typeof(MockBindable), null);
			var bindable = new MockBindable();

			Assert.Null(bindable.GetValue(prop));
			bindable.SetValue(prop, "foo");

			Assert.AreEqual("foo", ((CastFromString)bindable.GetValue(prop)).Result);
		}

		class CastToString
		{
			string Result { get; set; }

			public CastToString(string result)
			{
				Result = result;
			}

			public static implicit operator string(CastToString source)
			{
				return source.Result;
			}

			public override string ToString()
			{
				throw new InvalidOperationException();
			}
		}

		[Test]
		public void SetValueCoreInvokesOpImplicitOnValue()
		{
			var prop = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable), null);
			var bindable = new MockBindable();

			Assert.Null(bindable.GetValue(prop));
			bindable.SetValue(prop, new CastToString("foo"));

			Assert.AreEqual("foo", bindable.GetValue(prop));
		}

		[Test]
		public void DefaultValueCreatorCalledForChangeDelegates()
		{
			int changedOld = -1;
			int changedNew = -1;

			int changingOld = -1;
			int changingNew = -1;
			var prop = BindableProperty.Create("Foo", typeof(int), typeof(MockBindable), 0, defaultValueCreator: b => 10,
												propertyChanged: (b, value, newValue) =>
												{
													changedOld = (int)value;
													changedNew = (int)newValue;
												},
												propertyChanging: (b, value, newValue) =>
												{
													changingOld = (int)value;
													changingNew = (int)newValue;
												});

			var bindable = new MockBindable();


			var defaultValue = (int)bindable.GetValue(prop);

			Assert.AreEqual(10, defaultValue);

			bindable.SetValue(prop, 5);

			bindable.ClearValue(prop);

			Assert.AreEqual(5, changedOld);
			Assert.AreEqual(5, changingOld);
			Assert.AreEqual(10, changedNew);
			Assert.AreEqual(10, changingNew);
		}

		[Test]
		public void GetValuesDefaults()
		{
			var prop = BindableProperty.Create("Foo", typeof(int), typeof(MockBindable), 0);
			var prop1 = BindableProperty.Create("Foo1", typeof(int), typeof(MockBindable), 1);
			var prop2 = BindableProperty.Create("Foo2", typeof(int), typeof(MockBindable), 2);
			var bindable = new MockBindable();


			object[] values = bindable.GetValues(prop, prop1, prop2);
			Assert.That(values.Length == 3);
			Assert.That(values[0], Is.EqualTo(0));
			Assert.That(values[1], Is.EqualTo(1));
			Assert.That(values[2], Is.EqualTo(2));
		}

		[Test]
		public void GetValues()
		{
			var prop = BindableProperty.Create("Foo", typeof(int), typeof(MockBindable), 0);
			var prop1 = BindableProperty.Create("Foo1", typeof(int), typeof(MockBindable), 1);
			var prop2 = BindableProperty.Create("Foo2", typeof(int), typeof(MockBindable), 2);
			var bindable = new MockBindable();
			bindable.SetValue(prop, 3);
			bindable.SetValue(prop2, 5);


			object[] values = bindable.GetValues(prop, prop1, prop2);
			Assert.That(values.Length == 3);
			Assert.That(values[0], Is.EqualTo(3));
			Assert.That(values[1], Is.EqualTo(1));
			Assert.That(values[2], Is.EqualTo(5));
		}

		class BindingContextConverter
			: IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				return new MockViewModel { Text = value + "Converted" };
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}

		[Test]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=24485
		public void BindingContextBoundThroughConverter()
		{
			var bindable = new MockBindable();
			bindable.BindingContext = "test";
			bindable.SetBinding(BindableObject.BindingContextProperty, new Binding(".", converter: new BindingContextConverter()));
			bindable.SetBinding(MockBindable.TextProperty, "Text");

			Assert.That(() => bindable.Text, Is.EqualTo("testConverted"));
		}

		public class VMLocator
		{
			public event EventHandler Invoked;
			public int Count;
			public object VM
			{
				get
				{
					Count++;
					var eh = Invoked;
					if (eh != null)
						eh(this, EventArgs.Empty);
					return new object();
				}
			}
		}

		[Test]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=27299
		public void BindingOnBindingContextDoesntReapplyBindingContextBinding()
		{
			var bindable = new MockBindable();
			var locator = new VMLocator();
			Assert.AreEqual(0, locator.Count);
			locator.Invoked += (sender, e) => Assert.IsTrue(locator.Count <= 1);
			bindable.SetBinding(BindableObject.BindingContextProperty, new Binding("VM", source: locator));
			Assert.IsTrue(locator.Count == 1);
		}

		[Test]
		//https://github.com/xamarin/Xamarin.Forms/issues/2019
		public void EventSubscribingOnBindingContextChanged()
		{
			var source = new MockBindable();
			var bindable = new MockBindable();
			var property = BindableProperty.Create("foo", typeof(string), typeof(MockBindable), null);
			bindable.SetBinding(property, new Binding("BindingContext", source: source));
			Assert.That((string)bindable.GetValue(property), Is.EqualTo(null));
			BindableObject.SetInheritedBindingContext(source, "bar"); //inherited BC, only trigger BCChanged
			Assert.That((string)bindable.GetValue(property), Is.EqualTo("bar"));
		}

		[Test]
		public void BindingsEditableAfterUnapplied()
		{
			var bindable = new MockBindable();

			var binding = new Binding(".");
			bindable.SetBinding(MockBindable.TextProperty, binding);
			bindable.BindingContext = "foo";

			Assume.That(bindable.Text, Is.EqualTo(bindable.BindingContext));

			bindable.RemoveBinding(MockBindable.TextProperty);

			Assert.That(() => binding.Path = "foo", Throws.Nothing);
		}

		[Test]
		// https://bugzilla.xamarin.com/show_bug.cgi?id=24054
		public void BindingsAppliedUnappliedWithNullContext()
		{
			var bindable = new MockBindable();

			var binding = new Binding(".");
			bindable.SetBinding(MockBindable.TextProperty, binding);
			bindable.BindingContext = "foo";

			Assume.That(bindable.Text, Is.EqualTo(bindable.BindingContext));

			bindable.BindingContext = null;

			Assume.That(bindable.Text, Is.EqualTo(bindable.BindingContext));

			bindable.BindingContext = "bar";

			Assume.That(bindable.Text, Is.EqualTo(bindable.BindingContext));

			bindable.RemoveBinding(MockBindable.TextProperty);

			Assert.That(() => binding.Path = "Foo", Throws.Nothing);
		}


		class InfoToString
		{
			public override string ToString() => "converted";
		}

		[Test]
		//https://github.com/xamarin/Xamarin.Forms/issues/6281
		public void SetValueToTextInvokesToString()
		{
			var prop = BindableProperty.Create("foo", typeof(string), typeof(MockBindable), null);
			var bindable = new MockBindable();
			bindable.SetValue(prop, new InfoToString());

			Assert.That(bindable.GetValue(prop), Is.EqualTo("converted"));
		}

		[Test]
		//https://github.com/xamarin/Xamarin.Forms/issues/6281
		public void SetBindingToTextInvokesToString()
		{
			var prop = BindableProperty.Create("foo", typeof(string), typeof(MockBindable), null);
			var bindable = new MockBindable() { BindingContext = new { info = new InfoToString() } };
			bindable.SetBinding(prop, "info");

			Assert.That(bindable.GetValue(prop), Is.EqualTo("converted"));
		}

	}
}