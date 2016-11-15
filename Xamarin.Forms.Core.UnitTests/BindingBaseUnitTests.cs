using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	public abstract class BindingBaseUnitTests : BaseTestFixture
	{
		internal class Logger : LogListener
		{
			public IReadOnlyList<string> Messages {
				get { return messages; }
			}

			public override void Warning(string category, string message)
			{
				messages.Add("[" + category + "] " + message);
			}

			readonly List<string> messages = new List<string>();
		}

		internal Logger log;

		protected abstract BindingBase CreateBinding (BindingMode mode = BindingMode.Default, string stringFormat = null);

		internal class ComplexMockViewModel : MockViewModel
		{
			public ComplexMockViewModel Model {
				get { return model; }
				set {
					if (model == value)
						return;

					model = value;
					OnPropertyChanged("Model");
				}
			}

			internal int count;
			public int QueryCount {
				get { return count++; }
			}

			[IndexerName("Indexer")]
			public string this [int v] {
				get { return values [v]; }
				set {
					if (values [v] == value)
						return;

					values [v] = value;
					OnPropertyChanged("Indexer[" + v + "]");
				}
			}

			public string [] Array {
				get;
				set;
			}

			public object DoStuff()
			{
				return null;
			}

			public object DoStuff(object argument)
			{
				return null;
			}

			string [] values = new string [5];
			ComplexMockViewModel model;
		}

		[Test]
		public void CloneMode()
		{
			var binding = CreateBinding (BindingMode.Default);
			var clone = binding.Clone();

			Assert.AreEqual (binding.Mode, clone.Mode);
		}

		[Test]
		public void StringFormat()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var binding = CreateBinding (BindingMode.Default, "Foo {0}");

			var vm = new MockViewModel { Text = "Bar" };
			var bo = new MockBindable { BindingContext = vm };
			bo.SetBinding (property, binding);

			Assert.That (bo.GetValue (property), Is.EqualTo ("Foo Bar"));
		}

		[Test]
		public void StringFormatOnUpdate()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var binding = CreateBinding (BindingMode.Default, "Foo {0}");

			var vm = new MockViewModel { Text = "Bar" };
			var bo = new MockBindable { BindingContext = vm };
			bo.SetBinding (property, binding);

			vm.Text = "Baz";

			Assert.That (bo.GetValue (property), Is.EqualTo ("Foo Baz"));
		}

		[Test]
		[Description ("StringFormat should not be applied to OneWayToSource bindings")]
		public void StringFormatOneWayToSource()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var binding = CreateBinding (BindingMode.OneWayToSource, "Foo {0}");

			var vm = new MockViewModel { Text = "Bar" };
			var bo = new MockBindable { BindingContext = vm };
			bo.SetBinding (property, binding);

			bo.SetValue (property, "Bar");

			Assert.That (vm.Text, Is.EqualTo ("Bar"));
		}

		[Test]
		[Description ("StringFormat should only be applied from from source in TwoWay bindings")]
		public void StringFormatTwoWay()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var binding = CreateBinding (BindingMode.TwoWay, "Foo {0}");

			var vm = new MockViewModel { Text = "Bar" };
			var bo = new MockBindable { BindingContext = vm };
			bo.SetBinding (property, binding);

			bo.SetValue (property, "Baz");

			Assert.That (vm.Text, Is.EqualTo ("Baz"));
			Assert.That (bo.GetValue (property), Is.EqualTo ("Foo Baz"));
		}

		[Test]
		[Description ("You should get an exception when trying to change a binding after it's been applied")]
		public void ChangeAfterApply()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var binding = CreateBinding (BindingMode.OneWay);

			var vm = new MockViewModel { Text = "Bar" };
			var bo = new MockBindable { BindingContext = vm };
			bo.SetBinding (property, binding);

			Assert.That (() => binding.Mode = BindingMode.OneWayToSource, Throws.InvalidOperationException);
			Assert.That (() => binding.StringFormat = "{0}", Throws.InvalidOperationException);
		}

		[Test]
		public void StringFormatNonStringType()
		{
			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var binding = new Binding("Value", stringFormat: "{0:P2}");

			var vm = new  { Value = 0.95d };
			var bo = new MockBindable { BindingContext = vm };
			bo.SetBinding(property, binding);

			if (System.Threading.Thread.CurrentThread.CurrentCulture.Name == "tr-TR")
				Assert.That(bo.GetValue(property), Is.EqualTo("%95,00"));
			else
				Assert.That(bo.GetValue(property), Is.EqualTo("95.00 %"));
		}

		[Test]
		public void ReuseBindingInstance()
		{
			var vm = new MockViewModel();

			var bindable = new MockBindable();
			bindable.BindingContext = vm;

			var property = BindableProperty.Create("Foo", typeof(string), typeof(MockBindable));
			var binding = new Binding("Text");
			bindable.SetBinding(property, binding);

			var bindable2 = new MockBindable();
			bindable2.BindingContext = new MockViewModel();
			Assert.Throws<InvalidOperationException>(() => bindable2.SetBinding(property, binding),
				"Binding allowed reapplication with a different context");
		}

		[Test, Category("[Binding] Set Value")]
		public void ValueSetOnOneWay(
			[Values(true, false)] bool setContextFirst,
			[Values(true, false)] bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new MockViewModel {
				Text = value
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), null, propertyDefault);
			var binding = CreateBinding(bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			} else {
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual(value, viewmodel.Text,
				"BindingContext property changed");
			Assert.AreEqual(value, bindable.GetValue(property),
				"Target property did not change");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category("[Binding] Set Value")]
		public void ValueSetOnOneWayToSource(
			[Values(true, false)] bool setContextFirst,
			[Values(true, false)] bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new MockViewModel();

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault) {
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), defaultValue: value, defaultBindingMode: propertyDefault);
			var binding = CreateBinding(bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			} else {
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual(value, bindable.GetValue(property),
				"Target property changed");
			Assert.AreEqual(value, viewmodel.Text,
				"BindingContext property did not change");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category("[Binding] Set Value")]
		public void ValueSetOnTwoWay(
			[Values(true, false)] bool setContextFirst,
			[Values(true, false)] bool isDefault)
		{
			const string value = "Foo";
			var viewmodel = new MockViewModel {
				Text = value
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), defaultValue: "default value", defaultBindingMode: propertyDefault);
			var binding = CreateBinding(bindingMode);

			var bindable = new MockBindable();
			if (setContextFirst) {
				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);
			} else {
				bindable.SetBinding(property, binding);
				bindable.BindingContext = viewmodel;
			}

			Assert.AreEqual(value, viewmodel.Text,
				"BindingContext property changed");
			Assert.AreEqual(value, bindable.GetValue(property),
				"Target property did not change");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category("[Binding] Update Value")]
		public void ValueUpdatedWithSimplePathOnOneWayBinding(
			[Values(true, false)]bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new MockViewModel {
				Text = "Foo"
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = CreateBinding(bindingMode);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			viewmodel.Text = newvalue;
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual(newvalue, viewmodel.Text,
				"Source property changed when it shouldn't");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category("[Binding] Update Value")]
		public void ValueUpdatedWithSimplePathOnOneWayToSourceBinding(
			[Values(true, false)]bool isDefault)

		{
			const string newvalue = "New Value";
			var viewmodel = new MockViewModel {
				Text = "Foo"
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault) {
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = CreateBinding(bindingMode);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			string original = (string)bindable.GetValue(property);
			const string value = "value";
			viewmodel.Text = value;
			Assert.AreEqual(original, bindable.GetValue(property),
				"Target updated from Source on OneWayToSource");

			bindable.SetValue(property, newvalue);
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Bindable did not update on binding context property change");
			Assert.AreEqual(newvalue, viewmodel.Text,
				"Source property changed when it shouldn't");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test, Category("[Binding] Update Value")]
		public void ValueUpdatedWithSimplePathOnTwoWayBinding(
			[Values(true, false)]bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new MockViewModel {
				Text = "Foo"
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = CreateBinding(bindingMode);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			viewmodel.Text = newvalue;
			Assert.AreEqual(newvalue, bindable.GetValue(property),
				"Target property did not update change");
			Assert.AreEqual(newvalue, viewmodel.Text,
				"Source property changed from what it was set to");

			const string newvalue2 = "New Value in the other direction";

			bindable.SetValue(property, newvalue2);
			Assert.AreEqual(newvalue2, viewmodel.Text,
				"Source property did not update with Target's change");
			Assert.AreEqual(newvalue2, bindable.GetValue(property),
				"Target property changed from what it was set to");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ValueUpdatedWithOldContextDoesNotUpdateWithOneWayBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new MockViewModel {
				Text = "Foo"
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWay;
			if (isDefault) {
				propertyDefault = BindingMode.OneWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = CreateBinding(bindingMode);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			bindable.BindingContext = new MockViewModel();
			Assert.AreEqual(null, bindable.GetValue(property));

			viewmodel.Text = newvalue;
			Assert.AreEqual(null, bindable.GetValue(property),
				"Target updated from old Source property change");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ValueUpdatedWithOldContextDoesNotUpdateWithTwoWayBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new MockViewModel {
				Text = "Foo"
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.TwoWay;
			if (isDefault) {
				propertyDefault = BindingMode.TwoWay;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = CreateBinding(bindingMode);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			bindable.BindingContext = new MockViewModel();
			Assert.AreEqual(null, bindable.GetValue(property));

			viewmodel.Text = newvalue;
			Assert.AreEqual(null, bindable.GetValue(property),
				"Target updated from old Source property change");

			string original = viewmodel.Text;

			bindable.SetValue(property, newvalue);
			Assert.AreEqual(original, viewmodel.Text,
				"Source updated from old Target property change");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[TestCase(true)]
		[TestCase(false)]
		public void ValueUpdatedWithOldContextDoesNotUpdateWithOneWayToSourceBinding(bool isDefault)
		{
			const string newvalue = "New Value";
			var viewmodel = new MockViewModel {
				Text = "Foo"
			};

			BindingMode propertyDefault = BindingMode.OneWay;
			BindingMode bindingMode = BindingMode.OneWayToSource;
			if (isDefault) {
				propertyDefault = BindingMode.OneWayToSource;
				bindingMode = BindingMode.Default;
			}

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", propertyDefault);
			var binding = CreateBinding(bindingMode);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			bindable.BindingContext = new MockViewModel();
			Assert.AreEqual(property.DefaultValue, bindable.GetValue(property));

			viewmodel.Text = newvalue;
			Assert.AreEqual(property.DefaultValue, bindable.GetValue(property),
				"Target updated from old Source property change");
			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test]
		public void BindingStaysOnUpdateValueFromBinding()
		{
			const string newvalue = "New Value";
			var viewmodel = new MockViewModel {
				Text = "Foo"
			};

			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), null);
			var binding = CreateBinding(BindingMode.Default);

			var bindable = new MockBindable();
			bindable.BindingContext = viewmodel;
			bindable.SetBinding(property, binding);

			viewmodel.Text = newvalue;
			Assert.AreEqual(newvalue, bindable.GetValue(property));

			const string newValue2 = "new value 2";
			viewmodel.Text = newValue2;
			Assert.AreEqual(newValue2, bindable.GetValue(property));

			Assert.That(log.Messages.Count, Is.EqualTo(0),
				"An error was logged: " + log.Messages.FirstOrDefault());
		}

		[Test]
		public void OneWayToSourceContextSetToNull()
		{
			var binding = new Binding("Text", BindingMode.OneWayToSource);

			MockBindable bindable = new MockBindable {
				BindingContext = new MockViewModel()
			};
			bindable.SetBinding(MockBindable.TextProperty, binding);

			Assert.That(() => bindable.BindingContext = null, Throws.Nothing);
		}

		[Category("[Binding] Simple paths")]
		[TestCase(BindingMode.OneWay)]
		[TestCase(BindingMode.OneWayToSource)]
		[TestCase(BindingMode.TwoWay)]
		public void SourceAndTargetAreWeakWeakSimplePath(BindingMode mode)
		{
			var property = BindableProperty.Create("Text", typeof(string), typeof(MockBindable), "default value", BindingMode.OneWay);
			var binding = CreateBinding(mode);

			WeakReference weakViewModel = null, weakBindable = null;

			int i = 0;
			Action create = null;
			create = () => {
				if (i++ < 1024) {
					create();
					return;
				}

				MockBindable bindable = new MockBindable();
				weakBindable = new WeakReference(bindable);

				MockViewModel viewmodel = new MockViewModel();
				weakViewModel = new WeakReference(viewmodel);

				bindable.BindingContext = viewmodel;
				bindable.SetBinding(property, binding);

				Assume.That(() => bindable.BindingContext = null, Throws.Nothing);
			};

			create();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			if (mode == BindingMode.TwoWay || mode == BindingMode.OneWay)
				Assert.IsFalse(weakViewModel.IsAlive, "ViewModel wasn't collected");

			if (mode == BindingMode.TwoWay || mode == BindingMode.OneWayToSource)
				Assert.IsFalse(weakBindable.IsAlive, "Bindable wasn't collected");
		}

		[Test]
		public void PropertyChangeBindingsOccurThroughMainThread()
		{
			var vm = new MockViewModel { Text = "text" };

			var bindable = new MockBindable();
			var binding = CreateBinding();
			bindable.BindingContext = vm;
			bindable.SetBinding(MockBindable.TextProperty, binding);

			bool mainThread = false;
			Device.PlatformServices = new MockPlatformServices(invokeOnMainThread: a => mainThread = true);

			vm.Text = "updated";

			Assert.IsTrue(mainThread, "Binding did not occur on main thread");
			Assert.AreNotEqual(vm.Text, bindable.GetValue(MockBindable.TextProperty), "Binding was applied anyway through other means");
		}
	}

	[TestFixture]
	public class BindingBaseTests : BaseTestFixture
	{
		[Test]
		public void EnableCollectionSynchronizationInvalid()
		{
			Assert.That (() => BindingBase.EnableCollectionSynchronization (null, new object(),
				(collection, context, method, access) => { }), Throws.InstanceOf<ArgumentNullException>());
			Assert.That (() => BindingBase.EnableCollectionSynchronization (new string[0], new object(),
				null), Throws.InstanceOf<ArgumentNullException>());
			Assert.That (() => BindingBase.EnableCollectionSynchronization (new string[0], null,
				(collection, context, method, access) => { }), Throws.Nothing);
		}

		[Test]
		public void EnableCollectionSynchronization()
		{
			string[] stuff = new[] {"foo", "bar"};
			object context = new object();
			CollectionSynchronizationCallback callback = (collection, o, method, access) => { };

			BindingBase.EnableCollectionSynchronization (stuff, context, callback);

			CollectionSynchronizationContext syncContext;
			Assert.IsTrue (BindingBase.TryGetSynchronizedCollection (stuff, out syncContext));
			Assert.That (syncContext, Is.Not.Null);
			Assert.AreSame (syncContext.Callback, callback);
			Assert.That (syncContext.ContextReference, Is.Not.Null);
			Assert.That (syncContext.ContextReference.Target, Is.SameAs (context));
		}

		[Test]
		public void DisableCollectionSynchronization()
		{
			string[] stuff = new[] {"foo", "bar"};
			object context = new object();
			CollectionSynchronizationCallback callback = (collection, o, method, access) => { };

			BindingBase.EnableCollectionSynchronization (stuff, context, callback);

			BindingBase.DisableCollectionSynchronization (stuff);

			CollectionSynchronizationContext syncContext;
			Assert.IsFalse (BindingBase.TryGetSynchronizedCollection (stuff, out syncContext));
			Assert.IsNull (syncContext);
		}

		[Test]
		public void CollectionAndContextAreHeldWeakly()
		{
			WeakReference weakCollection = null, weakContext = null;

			int i = 0;
			Action create = null;
			create = () => {
				if (i++ < 1024) {
					create();
					return;
				}

				string[] collection = new[] {"foo", "bar"};
				weakCollection = new WeakReference (collection);

				object context = new object();
				weakContext = new WeakReference (context);

				BindingBase.EnableCollectionSynchronization (collection, context, (enumerable, o, method, access) => { });
			};

			create();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.IsFalse (weakCollection.IsAlive);
			Assert.IsFalse (weakContext.IsAlive);
		}

		[Test]
		public void CollectionAndContextAreHeldWeaklyClosingOverCollection()
		{
			WeakReference weakCollection = null, weakContext = null;

			int i = 0;
			Action create = null;
			create = () => {
				if (i++ < 1024) {
					create();
					return;
				}

				string[] collection = new[] {"foo", "bar"};
				weakCollection = new WeakReference (collection);

				object context = new object();
				weakContext = new WeakReference (context);

				BindingBase.EnableCollectionSynchronization (collection, context, (enumerable, o, method, access) => {
					collection[0] = "baz";
				});
			};

			create();

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.IsFalse (weakCollection.IsAlive);
			Assert.IsFalse (weakContext.IsAlive);
		}

		[Test]
		public void DisableCollectionSynchronizationInvalid()
		{
			Assert.That (() => BindingBase.DisableCollectionSynchronization (null), Throws.InstanceOf<ArgumentNullException>());
		}

		[Test]
		public void TryGetSynchronizedCollectionInvalid()
		{
			CollectionSynchronizationContext context;
			Assert.That (() => BindingBase.TryGetSynchronizedCollection (null, out context),
				Throws.InstanceOf<ArgumentNullException>());
		}

	}
}
