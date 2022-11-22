using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Internals;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class NotifyCollectionChangedEventArgsExtensionsTests : BaseTestFixture
	{
		[Fact]
		public void Add()
		{
			List<string> applied = new List<string> { "foo", "bar", "baz" };

			Action reset = () => throw new XunitException("Reset should not be called");
			Action<object, int, bool> insert = (o, i, create) =>
			{
				Assert.True(create);
				applied.Insert(i, (string)o);
			};

			Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

			var items = new ObservableCollection<string>(applied);
			items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

			items.Add("monkey");

			Assert.Equal(items, applied);
		}

		[Fact]
		public void Insert()
		{
			List<string> applied = new List<string> { "foo", "bar", "baz" };

			Action reset = () => throw new XunitException("Reset should not be called");
			Action<object, int, bool> insert = (o, i, create) =>
			{
				Assert.True(create);
				applied.Insert(i, (string)o);
			};
			Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

			var items = new ObservableCollection<string>(applied);
			items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

			items.Insert(1, "monkey");

			Assert.Equal(items, applied);
		}

		[Fact]
		public void Move()
		{
			List<string> applied = new List<string> { "foo", "bar", "baz" };

			Action reset = () => throw new XunitException("Reset should not be called");
			Action<object, int, bool> insert = (o, i, create) =>
			{
				Assert.False(create);
				applied.Insert(i, (string)o);
			};

			Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

			var items = new ObservableCollection<string>(applied);
			items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

			items.Move(0, 2);

			Assert.Equal(items, applied);
		}

		[Fact]
		public void Replace()
		{
			List<string> applied = new List<string> { "foo", "bar", "baz" };

			Action reset = () => throw new XunitException("Reset should not be called");
			Action<object, int, bool> insert = (o, i, create) =>
			{
				Assert.True(create);
				applied.Insert(i, (string)o);
			};

			Action<object, int> removeAt = (o, i) => applied.RemoveAt(i);

			var items = new ObservableCollection<string>(applied);
			items.CollectionChanged += (s, e) => e.Apply(insert, removeAt, reset);

			items[1] = "monkey";

			Assert.Equal(items, applied);
		}
	}
}
