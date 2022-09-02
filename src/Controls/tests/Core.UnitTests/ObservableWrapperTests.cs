using System;
using System.Collections.ObjectModel;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ObservableWrapperTests : BaseTestFixture
	{
		[Fact]
		public void Constructor()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(observableCollection);

			Assert.Empty(wrapper);

			Assert.Throws<ArgumentNullException>(() => new ObservableWrapper<View, View>(null));
		}

		[Fact]
		public void IgnoresInternallyAdded()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(observableCollection);

			var child = new View();

			observableCollection.Add(child);

			Assert.Empty(wrapper);
		}

		[Fact]
		public void TracksExternallyAdded()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(observableCollection);

			var child = new Button();

			wrapper.Add(child);

			Assert.Equal(child, wrapper[0]);
			Assert.Equal(child, observableCollection[0]);
		}

		[Fact]
		public void AddWithInternalItemsAlreadyAdded()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(observableCollection);

			var view = new View();
			observableCollection.Add(view);

			var btn = new Button();

			wrapper.Add(btn);

			Assert.Equal(btn, wrapper[0]);
			Assert.Single(wrapper);

			Assert.Contains(btn, observableCollection);
			Assert.Contains(view, observableCollection);
			Assert.Equal(2, observableCollection.Count);
		}

		[Fact]
		public void IgnoresInternallyAddedSameType()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(observableCollection);

			var child = new View();

			observableCollection.Add(child);

			Assert.Empty(wrapper);
		}

		[Fact]
		public void TracksExternallyAddedSameType()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(observableCollection);

			var child = new Button();

			wrapper.Add(child);

			Assert.Equal(child, wrapper[0]);
			Assert.Equal(child, observableCollection[0]);
		}

		[Fact]
		public void AddWithInternalItemsAlreadyAddedSameType()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(observableCollection);

			var view = new View();
			observableCollection.Add(view);

			var btn = new Button();

			wrapper.Add(btn);

			Assert.Equal(btn, wrapper[0]);
			Assert.Single(wrapper);

			Assert.Contains(btn, observableCollection);
			Assert.Contains(view, observableCollection);
			Assert.Equal(2, observableCollection.Count);
		}

		[Fact]
		public void CannotRemoveInternalItem()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(observableCollection);

			var child = new View();

			observableCollection.Add(child);

			Assert.Empty(wrapper);

			Assert.False(wrapper.Remove(child));

			Assert.Contains(child, observableCollection);
		}

		[Fact]
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

		[Fact]
		public void Indexer()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(observableCollection);

			wrapper.Add(new Button());

			var newButton = new Button();

			wrapper[0] = newButton;

			Assert.Equal(newButton, wrapper[0]);
		}

		[Fact]
		public void IndexerSameType()
		{
			var observableCollection = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, View>(observableCollection);

			wrapper.Add(new Button());

			var newButton = new Button();

			wrapper[0] = newButton;

			Assert.Equal(newButton, wrapper[0]);
		}

		[Fact]
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

			Assert.Equal(target[2], child1);
			Assert.Equal(target[3], child2);
			Assert.Equal(target[4], child3);
			Assert.Equal(target[5], child4);
			Assert.Equal(target[6], child5);
		}

		[Fact]
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

			Assert.Equal(0, addIndex);
			Assert.Equal(child, addedResult);
		}

		[Fact]
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

			Assert.Equal(-1, addIndex);
			Assert.Null(addedResult);
		}

		[Fact]
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

			Assert.Equal(0, addIndex);
			Assert.Equal(child, addedResult);
		}

		[Fact]
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

			Assert.Equal(0, removeIndex);
			Assert.Equal(child, removedResult);
		}

		[Fact]
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

			Assert.Equal(-1, addIndex);
			Assert.Null(addedResult);
		}

		[Fact]
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

			Assert.Equal(child, removedResult);
			Assert.Equal(0, removeIndex);
		}

		[Fact]
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

			Assert.Equal(child, removedResult);
			Assert.Equal(2, removeIndex);
		}

		[Fact]
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

			Assert.Equal(0, index);
			Assert.Equal(child1, oldItem);
			Assert.Equal(child2, newItem);
		}

		[Fact]
		public void Clear()
		{
			var oc = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(oc);

			oc.Add(new Stepper());

			wrapper.Add(new Button());
			wrapper.Add(new Button());

			wrapper.Clear();
			Assert.Single(oc);
			Assert.Empty(wrapper);
		}

		[Fact]
		public void DifferentTypes()
		{
			var oc = new ObservableCollection<Element>();
			var wrapper = new ObservableWrapper<Element, Button>(oc);

			// Wrong type!
			oc.Add(new Label());

			var child1 = new Button();
			var child2 = new Button();
			wrapper.Add(child1);
			wrapper.Add(child2);

			// Do things that might cast
			foreach (var item in wrapper)
			{ }
			var target = new Button[4];
			wrapper.CopyTo(target, 2);
			Assert.Equal(target[2], child1);
			Assert.Equal(target[3], child2);
		}

		[Fact]
		public void CopyToArrayBaseType()
		{
			var oc = new ObservableCollection<View>();
			var wrapper = new ObservableWrapper<View, Button>(oc);

			oc.Add(new Stepper());

			var child1 = new Button();
			var child2 = new Button();
			wrapper.Add(child1);
			wrapper.Add(child2);

			var target = new View[4];
			wrapper.CopyTo((Array)target, 2);
			Assert.Equal(target[2], child1);
			Assert.Equal(target[3], child2);
		}
	}
}
