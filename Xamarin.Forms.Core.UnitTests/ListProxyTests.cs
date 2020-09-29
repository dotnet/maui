using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ListProxyTests : BaseTestFixture
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

		[Test]
		public void ListCount()
		{
			var list = new List<string> { "foo", "bar" };
			var proxy = new ListProxy(list);

			Assert.AreEqual(list.Count, proxy.Count);
			list.Add("baz");
			Assert.AreEqual(list.Count, proxy.Count);
		}

		[Test]
		public void CollectionCount()
		{
			var list = new Collection<string> { "foo", "bar" };
			var proxy = new ListProxy(list);

			Assert.AreEqual(list.Count, proxy.Count);
			list.Add("baz");
			Assert.AreEqual(list.Count, proxy.Count);
		}

		[Test]
		[Description("Count should ensure that the window is created if neccessary")]
		public void EnumerableInitialCount()
		{
			var enumerable = Enumerable.Range(0, 100);
			var proxy = new ListProxy(enumerable, 10);

			Assert.AreEqual(10, proxy.Count);
		}

		[Test]
		public void EnumerableCount()
		{
			var enumerable = Enumerable.Range(0, 100);
			var proxy = new ListProxy(enumerable, 10);

			int changed = 0;
			proxy.CountChanged += (o, e) => changed++;

			var enumerator = proxy.GetEnumerator();
			enumerator.MoveNext();

			Assert.AreEqual(10, proxy.Count);
			Assert.AreEqual(1, changed);

			enumerator.MoveNext();

			Assert.AreEqual(10, proxy.Count);
			Assert.AreEqual(1, changed);

			while (enumerator.MoveNext())
			{
			}

			enumerator.Dispose();

			Assert.AreEqual(100, proxy.Count);
			Assert.AreEqual(19, changed);

			using (enumerator = proxy.GetEnumerator())
			{

				Assert.AreEqual(100, proxy.Count);

				while (enumerator.MoveNext())
					Assert.AreEqual(100, proxy.Count);

				Assert.AreEqual(100, proxy.Count);
			}

			Assert.AreEqual(19, changed);
		}

		[Test]
		public void InsideWindowSize()
		{
			var numbers = Enumerable.Range(0, 100);
			var proxy = new ListProxy(numbers, 10);

			int i = (int)proxy[5];
			Assert.That(i, Is.EqualTo(5));
		}

		[Test]
		public void IndexOutsideWindowSize()
		{
			var numbers = Enumerable.Range(0, 100);
			var proxy = new ListProxy(numbers, 10);

			int i = (int)proxy[50];
			Assert.That(i, Is.EqualTo(50));
		}

		[Test]
		public void IndexInsideToOutsideWindowSize()
		{
			var numbers = Enumerable.Range(0, 100);
			var proxy = new ListProxy(numbers, 10);

			int i = (int)proxy[5];
			Assert.That(i, Is.EqualTo(5));

			i = (int)proxy[50];
			Assert.That(i, Is.EqualTo(50));
		}

		[Test]
		public void IndexOutsideToPreWindowSize()
		{
			var numbers = Enumerable.Range(0, 100);
			var proxy = new ListProxy(numbers, 10);

			int i = (int)proxy[50];
			Assert.That(i, Is.EqualTo(50));

			i = (int)proxy[5];
			Assert.That(i, Is.EqualTo(5));
		}

		[Test]
		public void EnumerableIndexOutOfRange()
		{
			var numbers = Enumerable.Range(0, 100);
			var proxy = new ListProxy(numbers);

			Assert.That(() => proxy[100], Throws.InstanceOf<ArgumentOutOfRangeException>());
		}

		class IntCollection
			: ICollection
		{
			readonly List<int> ints;

			public IntCollection(IEnumerable<int> ints)
			{
				this.ints = ints.ToList();
			}

			public IEnumerator GetEnumerator()
			{
				return ints.GetEnumerator();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void CopyTo(Array array, int index)
			{
				throw new NotImplementedException();
			}

			public int Count { get { return ints.Count; } }

			public object SyncRoot
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsSynchronized
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsReadOnly { get { return true; } }
		}

		[Test]
		public void CollectionIndexOutOfRange()
		{
			var numbers = new IntCollection(Enumerable.Range(0, 100));
			var proxy = new ListProxy(numbers);

			Assert.That(() => proxy[100], Throws.InstanceOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void ListIndexOutOfRange()
		{
			var numbers = Enumerable.Range(0, 100).ToList();
			var proxy = new ListProxy(numbers);

			Assert.That(() => proxy[100], Throws.InstanceOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void CollectionChangedWhileEnumerating()
		{
			var c = new ObservableCollection<string> { "foo", "bar" };
			var p = new ListProxy(c);

			IEnumerator<object> e = p.GetEnumerator();
			Assert.IsTrue(e.MoveNext(), "Initial MoveNext() failed, test can't continue");

			c.Add("baz");

			Assert.That(() => e.MoveNext(), Throws.InvalidOperationException,
				"MoveNext did not throw an exception when the underlying collection had changed");
		}

		[Test]
		public void SynchronizedCollectionAccess()
		{
			var collection = new ObservableCollection<string> { "foo" };
			var context = new object();

			var list = new ListProxy(collection);

			bool executed = false;
			BindingBase.EnableCollectionSynchronization(collection, context, (enumerable, o, method, access) =>
			{
				executed = true;
				Assert.AreSame(collection, enumerable);
				Assert.AreSame(context, o);
				Assert.IsNotNull(method);
				Assert.IsFalse(access);

				lock (enumerable)
					method();
			});

			object value = list[0];

			Assert.IsTrue(executed, "Callback was not executed");
		}

		[Test]
		public void SynchronizedCollectionAdd()
		{
			bool invoked = false;
			Device.PlatformServices = new MockPlatformServices(isInvokeRequired: true, invokeOnMainThread: action =>
			{
				invoked = true;
				action();
			});

			var collection = new ObservableCollection<string> { "foo" };
			var context = new object();

			var list = new ListProxy(collection);

			Assert.IsFalse(invoked, "An invoke shouldn't be executed just setting up ListProxy");

			bool executed = false;
			BindingBase.EnableCollectionSynchronization(collection, context, (enumerable, o, method, access) =>
			{
				executed = true;
				Assert.AreSame(collection, enumerable);
				Assert.AreSame(context, o);
				Assert.IsNotNull(method);
				Assert.IsFalse(access);

				lock (enumerable)
					method();
			});

			var mre = new ManualResetEvent(false);

			Task.Factory.StartNew(() =>
			{
				lock (collection)
					collection.Add("foo");

				mre.Set();
			}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);

			mre.WaitOne(5000);

			Assert.IsTrue(executed, "Callback was not executed");
			Assert.IsTrue(invoked, "Callback was not executed on the UI thread");
		}

		[Test]
		public void ClearEnumerable()
		{
			var proxy = new ListProxy(Enumerable.Range(0, 100));
			var enumerator = proxy.GetEnumerator();
			enumerator.MoveNext();
			enumerator.MoveNext();

			proxy.Clear();

			Assert.AreEqual(100, proxy.Count);
			Assert.That(() => enumerator.MoveNext(), Throws.InvalidOperationException);
		}

		[Test]
		public void ClearCollection()
		{
			var proxy = new ListProxy(new IntCollection(Enumerable.Range(0, 100)));
			var enumerator = proxy.GetEnumerator();
			enumerator.MoveNext();
			enumerator.MoveNext();

			proxy.Clear();

			Assert.AreEqual(100, proxy.Count);
			Assert.That(() => enumerator.MoveNext(), Throws.InvalidOperationException);
		}

		[Test]
		public void ClearList()
		{
			var proxy = new ListProxy(Enumerable.Range(0, 100).ToList());
			var enumerator = proxy.GetEnumerator();
			enumerator.MoveNext();
			enumerator.MoveNext();

			proxy.Clear();

			Assert.AreEqual(100, proxy.Count);
			Assert.That(() => enumerator.MoveNext(), Throws.InvalidOperationException);
		}

		[Test]
		public void IndexOfValueTypeNonList()
		{
			var proxy = new ListProxy(Enumerable.Range(0, 100));
			Assert.AreEqual(1, proxy.IndexOf(1));
		}

		[Test]
		public void EnumeratorForEnumerable()
		{
			var proxy = new ListProxy(Enumerable.Range(0, 2));

			var enumerator = proxy.GetEnumerator();
			Assert.That(enumerator.Current, Is.Null);
			Assert.That(enumerator.MoveNext(), Is.True);
			Assert.That(enumerator.Current, Is.EqualTo(0));
			Assert.That(enumerator.MoveNext(), Is.True);
			Assert.That(enumerator.Current, Is.EqualTo(1));
			Assert.That(enumerator.MoveNext(), Is.False);
		}

		[Test]
		public void ProxyIsWeaklyHeldByINotifyCollectionChanged()
		{
			ObservableCollection<string> collection = new ObservableCollection<string>();

			WeakReference weakProxy = null;

			int i = 0;
			Action create = null;
			create = () =>
			{
				if (i++ < 1024)
				{
					create();
					return;
				}

				var proxy = new ListProxy(collection);
				weakProxy = new WeakReference(proxy);
			};

			create();

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert.That(weakProxy.IsAlive, Is.False);
		}

		[Test]
		public void IEnumerableAddDoesNotReport0()
		{
			var custom = new CustomINCC();
			custom.Add("test");
			custom.Add("test2");

			var proxy = new ListProxy(custom);
			Assert.That(proxy.Count, Is.EqualTo(2));

			custom.Add("testing");
			Assert.That(proxy.Count, Is.EqualTo(3));
		}

		class CustomINCC : IEnumerable<string>, INotifyCollectionChanged
		{
			public event NotifyCollectionChangedEventHandler CollectionChanged;
			List<string> Items = new List<string>();

			public void Add(string s)
			{
				Items.Add(s);
				if (CollectionChanged != null)
					CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, s));
			}

			public IEnumerator<string> GetEnumerator()
			{
				return Items.GetEnumerator();
			}
			IEnumerator IEnumerable.GetEnumerator()
			{
				return Items.GetEnumerator();
			}
		}

		// Need a member to keep this reference around, otherwise it gets optimized
		// out early in Release mode during the WeakToWeak test
#pragma warning disable 0414 // Never accessed, it's just here to prevent GC
		ListProxy _proxyForWeakToWeakTest;
#pragma warning restore 0414

		[Test]
		public void WeakToWeak()
		{
			WeakCollectionChangedList list = new WeakCollectionChangedList();
			_proxyForWeakToWeakTest = new ListProxy(list);

			Assert.True(list.AddObject(), "GC hasn't run");

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.IsTrue(list.AddObject(), "GC run, but proxy should still hold a reference");

			_proxyForWeakToWeakTest = null;

			GC.Collect();
			GC.WaitForPendingFinalizers();

			Assert.IsFalse(list.AddObject(), "Proxy is gone and GC has run");
		}

		public class WeakCollectionChangedList : List<object>, INotifyCollectionChanged
		{
			List<WeakHandler> handlers = new List<WeakHandler>();

			public WeakCollectionChangedList()
			{

			}
			public event NotifyCollectionChangedEventHandler CollectionChanged
			{
				add { handlers.Add(new WeakHandler(this, value)); }
				remove { throw new NotImplementedException(); }
			}


			public bool AddObject()
			{
				bool invoked = false;
				var me = new object();
				Console.WriteLine($"Handler count is {handlers.Count}");
				foreach (var handler in handlers.ToList())
				{
					if (handler.IsActive)
					{
						invoked = true;
						handler.Handler.DynamicInvoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, me));
					}
					else
					{
						Console.WriteLine($"Handler is inactive");
						handlers.Remove(handler);
					}
				}

				return invoked;
			}

			class WeakHandler
			{
				WeakReference source;
				WeakReference originalHandler;

				public bool IsActive
				{
					get { return this.source != null && this.source.IsAlive && this.originalHandler != null && this.originalHandler.IsAlive; }
				}

				public NotifyCollectionChangedEventHandler Handler
				{
					get
					{
						if (this.originalHandler == null)
						{
							return default(NotifyCollectionChangedEventHandler);
						}
						else
						{
							return (NotifyCollectionChangedEventHandler)this.originalHandler.Target;
						}
					}
				}

				public WeakHandler(object source, NotifyCollectionChangedEventHandler originalHandler)
				{
					this.source = new WeakReference(source);
					this.originalHandler = new WeakReference(originalHandler);
				}
			}
		}
	}
}