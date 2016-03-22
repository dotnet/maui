using System;
using System.Linq;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	public abstract class BindingBaseUnitTests : BaseTestFixture
	{
		protected abstract BindingBase CreateBinding (BindingMode mode, string stringFormat = null);

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
			var property = BindableProperty.Create<MockBindable, string> (w => w.Foo, null);

			var binding = CreateBinding (BindingMode.Default, "Foo {0}");

			var vm = new MockViewModel { Text = "Bar" };
			var bo = new MockBindable { BindingContext = vm };
			bo.SetBinding (property, binding);

			Assert.That (bo.GetValue (property), Is.EqualTo ("Foo Bar"));
		}

		[Test]
		public void StringFormatOnUpdate()
		{
			var property = BindableProperty.Create<MockBindable, string> (w => w.Foo, null);

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
			var property = BindableProperty.Create<MockBindable, string> (w => w.Foo, null);

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
			var property = BindableProperty.Create<MockBindable, string> (w => w.Foo, null);

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
			var property = BindableProperty.Create<MockBindable, string> (w => w.Foo, null);

			var binding = CreateBinding (BindingMode.OneWay);

			var vm = new MockViewModel { Text = "Bar" };
			var bo = new MockBindable { BindingContext = vm };
			bo.SetBinding (property, binding);

			Assert.That (() => binding.Mode = BindingMode.OneWayToSource, Throws.InvalidOperationException);
			Assert.That (() => binding.StringFormat = "{0}", Throws.InvalidOperationException);
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
