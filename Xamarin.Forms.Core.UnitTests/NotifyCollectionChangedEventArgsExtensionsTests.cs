using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class NotifyCollectionChangedEventArgsExtensionsTests : BaseTestFixture
	{
		[Test]
		public void Add()
		{
			List<string> applied = new List<string> { "foo", "bar", "baz" };

			Action reset = () => Assert.Fail("Reset should not be called");
			Action<object, int, bool> insert = (o, i, create) =>
			{
				Assert.That(create, Is.True);
				applied.Insert(i, (string)o);
			};

			Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

			var items = new ObservableCollection<string>(applied);
			items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

			items.Add("monkey");

			CollectionAssert.AreEqual(items, applied);
		}

		[Test]
		public void Insert()
		{
			List<string> applied = new List<string> { "foo", "bar", "baz" };

			Action reset = () => Assert.Fail("Reset should not be called");
			Action<object, int, bool> insert = (o, i, create) =>
			{
				Assert.That(create, Is.True);
				applied.Insert(i, (string)o);
			};
			Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

			var items = new ObservableCollection<string>(applied);
			items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

			items.Insert(1, "monkey");

			CollectionAssert.AreEqual(items, applied);
		}

		[Test]
		public void Move()
		{
			List<string> applied = new List<string> { "foo", "bar", "baz" };

			Action reset = () => Assert.Fail("Reset should not be called");
			Action<object, int, bool> insert = (o, i, create) =>
			{
				Assert.That(create, Is.False);
				applied.Insert(i, (string)o);
			};

			Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

			var items = new ObservableCollection<string>(applied);
			items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

			items.Move(0, 2);

			CollectionAssert.AreEqual(items, applied);
		}

		[Test]
		public void Replace()
		{
			List<string> applied = new List<string> { "foo", "bar", "baz" };

			Action reset = () => Assert.Fail("Reset should not be called");
			Action<object, int, bool> insert = (o, i, create) =>
			{
				Assert.That(create, Is.True);
				applied.Insert(i, (string)o);
			};

			Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

			var items = new ObservableCollection<string>(applied);
			items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

			items[1] = "monkey";

			CollectionAssert.AreEqual(items, applied);
		}
	}
}