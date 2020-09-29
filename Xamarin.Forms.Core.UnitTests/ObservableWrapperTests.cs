using System;
using System.Collections.ObjectModel;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ObservableWrapperTests : BaseTestFixture
	{
		[Test]
		public void Constructor()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(observableCollection);

			Assert.IsEmpty(wrapper);

			Assert.Throws<ArgumentNullException>(() => new ObservableWrapper<View, View>(null));
		}

		[Test]
		public void IgnoresInternallyAdded()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(observableCollection);

			var child = new View();

			observableCollection.Add(child);

			Assert.IsEmpty(wrapper);
		}

		[Test]
		public void TracksExternallyAdded()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(observableCollection);

			var child = new Button();

			wrapper.Add(child);

			Assert.AreEqual(child, wrapper[0]);
			Assert.AreEqual(child, observableCollection[0]);
		}

		[Test]
		public void AddWithInternalItemsAlreadyAdded()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(observableCollection);

			var view = new View();
			observableCollection.Add(view);

			var btn = new Button();

			wrapper.Add(btn);

			Assert.AreEqual(btn, wrapper[0]);
			Assert.AreEqual(1, wrapper.Count);

			Assert.Contains(btn, observableCollection);
			Assert.Contains(view, observableCollection);
			Assert.AreEqual(2, observableCollection.Count);
		}

		[Test]
		public void IgnoresInternallyAddedSameType()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(observableCollection);

			var child = new View();

			observableCollection.Add(child);

			Assert.IsEmpty(wrapper);
		}

		[Test]
		public void TracksExternallyAddedSameType()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(observableCollection);

			var child = new Button();

			wrapper.Add(child);

			Assert.AreEqual(child, wrapper[0]);
			Assert.AreEqual(child, observableCollection[0]);
		}

		[Test]
		public void AddWithInternalItemsAlreadyAddedSameType()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(observableCollection);

			var view = new View();
			observableCollection.Add(view);

			var btn = new Button();

			wrapper.Add(btn);

			Assert.AreEqual(btn, wrapper[0]);
			Assert.AreEqual(1, wrapper.Count);

			Assert.Contains(btn, observableCollection);
			Assert.Contains(view, observableCollection);
			Assert.AreEqual(2, observableCollection.Count);
		}

		[Test]
		public void CannotRemoveInternalItem()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(observableCollection);

			var child = new View();

			observableCollection.Add(child);

			Assert.IsEmpty(wrapper);

			Assert.False(wrapper.Remove(child));

			Assert.Contains(child, observableCollection);
		}

		[Test]
		public void ReadOnly()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(observableCollection);

			Assert.False(wrapper.IsReadOnly);

			wrapper.Add(new Button());

			wrapper.IsReadOnly = true;

			Assert.True(wrapper.IsReadOnly);

			Assert.Throws<NotSupportedException>(() => wrapper.Remove(wrapper[0]));
			Assert.Throws<NotSupportedException>(() => wrapper.Add(new Button()));
			Assert.Throws<NotSupportedException>(() => wrapper.RemoveAt(0));
			Assert.Throws<NotSupportedException>(() => wrapper.Insert(0, new Button()));
			Assert.Throws<NotSupportedException>(wrapper.Clear);
		}

		[Test]
		public void Indexer()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(observableCollection);

			wrapper.Add(new Button());

			var newButton = new Button();

			wrapper[0] = newButton;

			Assert.AreEqual(newButton, wrapper[0]);
		}

		[Test]
		public void IndexerSameType()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(observableCollection);

			wrapper.Add(new Button());

			var newButton = new Button();

			wrapper[0] = newButton;

			Assert.AreEqual(newButton, wrapper[0]);
		}

		[Test]
		public void CopyTo()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(observableCollection);

			var child1 = new Button();
			var child2 = new Button();
			var child3 = new Button();
			var child4 = new Button();
			var child5 = new Button();

			observableCollection.Add(new Stepper());
			wrapper.Add(child1);
			observableCollection.Add(new Button());
			wrapper.Add(child2);
			wrapper.Add(child3);
			wrapper.Add(child4);
			wrapper.Add(child5);
			observableCollection.Add(new Button());

			var target = new View[30];
			wrapper.CopyTo(target, 2);

			Assert.AreEqual(target[2], child1);
			Assert.AreEqual(target[3], child2);
			Assert.AreEqual(target[4], child3);
			Assert.AreEqual(target[5], child4);
			Assert.AreEqual(target[6], child5);
		}

		[Test]
		public void INCCSimpleAdd()
		{
			var oc = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(oc);

			var child = new Button();

			Button addedResult = null;
			int addIndex = -1;
			wrapper.CollectionChanged += (sender, args) =>
			{
				addedResult = args.NewItems[0] as Button;
				addIndex = args.NewStartingIndex;
			};

			wrapper.Add(child);

			Assert.AreEqual(0, addIndex);
			Assert.AreEqual(child, addedResult);
		}

		[Test]
		public void INCCSimpleAddToInner()
		{
			var oc = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(oc);

			var child = new Button();

			Button addedResult = null;
			int addIndex = -1;
			wrapper.CollectionChanged += (sender, args) =>
			{
				addedResult = args.NewItems[0] as Button;
				addIndex = args.NewStartingIndex;
			};

			oc.Add(child);

			Assert.AreEqual(-1, addIndex);
			Assert.AreEqual(null, addedResult);
		}

		[Test]
		public void INCCComplexAdd()
		{
			var oc = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(oc);

			oc.Add(new Stepper());

			var child = new Button();

			Button addedResult = null;
			int addIndex = -1;
			wrapper.CollectionChanged += (sender, args) =>
			{
				addedResult = args.NewItems[0] as Button;
				addIndex = args.NewStartingIndex;
			};

			wrapper.Add(child);

			Assert.AreEqual(0, addIndex);
			Assert.AreEqual(child, addedResult);
		}

		[Test]
		public void INCCSimpleRemove()
		{
			var oc = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(oc);

			var child = new Button();
			wrapper.Add(child);

			Button removedResult = null;
			int removeIndex = -1;
			wrapper.CollectionChanged += (sender, args) =>
			{
				removedResult = args.OldItems[0] as Button;
				removeIndex = args.OldStartingIndex;
			};

			wrapper.Remove(child);

			Assert.AreEqual(0, removeIndex);
			Assert.AreEqual(child, removedResult);
		}

		[Test]
		public void INCCSimpleRemoveFromInner()
		{
			var oc = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(oc);

			var child = new Button();
			oc.Add(child);

			Button addedResult = null;
			int addIndex = -1;
			wrapper.CollectionChanged += (sender, args) =>
			{
				addedResult = args.OldItems[0] as Button;
				addIndex = args.OldStartingIndex;
			};

			oc.Remove(child);

			Assert.AreEqual(-1, addIndex);
			Assert.AreEqual(null, addedResult);
		}

		[Test]
		public void INCCComplexRemove()
		{
			var oc = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(oc);

			oc.Add(new Stepper());

			var child = new Button();
			wrapper.Add(child);

			Button removedResult = null;
			int removeIndex = -1;
			wrapper.CollectionChanged += (sender, args) =>
			{
				removedResult = args.OldItems[0] as Button;
				removeIndex = args.OldStartingIndex;
			};

			wrapper.Remove(child);

			Assert.AreEqual(child, removedResult);
			Assert.AreEqual(0, removeIndex);
		}

		[Test]
		public void INCCComplexRemoveLast()
		{
			var oc = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(oc);

			oc.Add(new Stepper());

			wrapper.Add(new Button());
			wrapper.Add(new Button());
			var child = new Button();
			wrapper.Add(child);

			Button removedResult = null;
			int removeIndex = -1;
			wrapper.CollectionChanged += (sender, args) =>
			{
				removedResult = args.OldItems[0] as Button;
				removeIndex = args.OldStartingIndex;
			};

			wrapper.Remove(child);

			Assert.AreEqual(child, removedResult);
			Assert.AreEqual(2, removeIndex);
		}

		[Test]
		public void INCCReplace()
		{
			var oc = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(oc);

			var child1 = new Button();
			var child2 = new Button();

			wrapper.Add(child1);

			int index = -1;
			Button oldItem = null;
			Button newItem = null;
			wrapper.CollectionChanged += (sender, args) =>
			{
				index = args.NewStartingIndex;
				oldItem = args.OldItems[0] as Button;
				newItem = args.NewItems[0] as Button;
			};

			wrapper[0] = child2;

			Assert.AreEqual(0, index);
			Assert.AreEqual(child1, oldItem);
			Assert.AreEqual(child2, newItem);
		}
	}
}