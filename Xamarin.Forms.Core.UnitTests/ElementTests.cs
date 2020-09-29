using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	public class TestElement
		: Element
	{
		public TestElement()
		{
			internalChildren.CollectionChanged += OnChildrenChanged;
		}

		void OnChildrenChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (Element element in e.NewItems)
					OnChildAdded(element);
			}

			if (e.OldItems != null)
			{
				foreach (Element element in e.OldItems)
					OnChildRemoved(element);
			}
		}

		public IList<Element> Children
		{
			get { return internalChildren; }
		}

		internal override ReadOnlyCollection<Element> LogicalChildrenInternal
		{
			get { return new ReadOnlyCollection<Element>(internalChildren); }
		}

		readonly ObservableCollection<Element> internalChildren = new ObservableCollection<Element>();
	}

	[TestFixture]
	public class ElementTests
		: BaseTestFixture
	{
		[Test]
		public void DescendantAddedLevel1()
		{
			var root = new TestElement();

			var child = new TestElement();

			bool added = false;
			root.DescendantAdded += (sender, args) =>
			{
				Assert.That(args.Element, Is.SameAs(child));
				added = true;
			};

			root.Children.Add(child);
		}

		[Test]
		public void DescendantAddedLevel2()
		{
			var root = new TestElement();

			var child = new TestElement();
			root.Children.Add(child);

			var child2 = new TestElement();

			bool added = false;
			root.DescendantAdded += (sender, args) =>
			{
				Assert.That(args.Element, Is.SameAs(child2));
				added = true;
			};

			child.Children.Add(child2);
		}

		[Test]
		public void DescendantAddedExistingChildren()
		{
			var root = new TestElement();

			var tier2 = new TestElement();

			var child = new TestElement
			{
				Children = {
					tier2
				}
			};

			bool tier1added = false;
			bool tier2added = false;
			root.DescendantAdded += (sender, args) =>
			{
				if (!tier1added)
					tier1added = ReferenceEquals(child, args.Element);
				if (!tier2added)
					tier2added = ReferenceEquals(tier2, args.Element);
			};

			root.Children.Add(child);

			Assert.That(tier1added, Is.True);
			Assert.That(tier2added, Is.True);
		}

		[Test]
		public void DescendantRemovedLevel1()
		{
			var root = new TestElement();

			var child = new TestElement();
			root.Children.Add(child);

			bool removed = false;
			root.DescendantRemoved += (sender, args) =>
			{
				Assert.That(args.Element, Is.SameAs(child));
				removed = true;
			};

			root.Children.Remove(child);
		}

		[Test]
		public void DescendantRemovedLevel2()
		{
			var root = new TestElement();

			var child = new TestElement();
			root.Children.Add(child);

			var child2 = new TestElement();
			child.Children.Add(child2);

			bool removed = false;
			root.DescendantRemoved += (sender, args) =>
			{
				Assert.That(args.Element, Is.SameAs(child2));
				removed = true;
			};

			child.Children.Remove(child2);
		}

		[Test]
		public void DescendantRemovedWithChildren()
		{
			var root = new TestElement();

			var tier2 = new TestElement();

			var child = new TestElement
			{
				Children = {
					tier2
				}
			};

			root.Children.Add(child);

			bool tier1removed = false;
			bool tier2removed = false;
			root.DescendantRemoved += (sender, args) =>
			{
				if (!tier1removed)
					tier1removed = ReferenceEquals(child, args.Element);
				if (!tier2removed)
					tier2removed = ReferenceEquals(tier2, args.Element);
			};

			root.Children.Remove(child);

			Assert.That(tier1removed, Is.True);
			Assert.That(tier2removed, Is.True);
		}
	}
}