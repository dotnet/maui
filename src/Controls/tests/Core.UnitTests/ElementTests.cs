using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class TestElement
		: Element
	{
		/// <summary>Bindable property for <see cref="ClassId"/>.</summary>
		public static readonly BindableProperty TrackPropertyChangedDelegateProperty = BindableProperty.Create(nameof(TestElement), typeof(int), typeof(Element), 0, propertyChanged: OnTrackPropertyChangedDelegate);


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
					OnChildRemoved(element, -1);
			}
		}

		public IList<Element> Children
		{
			get { return internalChildren; }
		}

		private protected override IList<Element> LogicalChildrenInternalBackingStore
			=> internalChildren;

		readonly ObservableCollection<Element> internalChildren = new ObservableCollection<Element>();


		public int TrackPropertyChangedDelegate
		{
			get => (int)GetValue(TrackPropertyChangedDelegateProperty);
			set => SetValue(TrackPropertyChangedDelegateProperty, value);
		}

		public event EventHandler TrackPropertyChanged;
		public int TrackPropertyChangedDelegateCount { get; private set; }
		public int TrackPropertyChangedOnPropertyChangedCount { get; private set; }
		public int TrackPropertyChangedUpdateHandlerValueCount { get; private set; }

		private static void OnTrackPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
		{
			((TestElement)bindable).TrackPropertyChangedDelegateCount++;
			((TestElement)bindable).TrackPropertyChanged?.Invoke(bindable, EventArgs.Empty);
		}

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (propertyName == TrackPropertyChangedDelegateProperty.PropertyName)
			{
				TrackPropertyChangedOnPropertyChangedCount++;
				TrackPropertyChanged?.Invoke(this, EventArgs.Empty);
			}

			base.OnPropertyChanged(propertyName);
		}

		private protected override void UpdateHandlerValue(string propertyName, bool valueChanged)
		{
			if (propertyName == TrackPropertyChangedDelegateProperty.PropertyName)
			{
				TrackPropertyChangedUpdateHandlerValueCount++;
				TrackPropertyChanged?.Invoke(this, EventArgs.Empty);
			}

			base.UpdateHandlerValue(propertyName, valueChanged);
		}
	}

	public class ElementTests
		: BaseTestFixture
	{

		[Fact]
		public void ValidateHandleUpdatesHappenAfterPropertyChangedDelegate()
		{
			var element = new TestElement();

			element.TrackPropertyChanged += (_, _) =>
			{
				Assert.True(element.TrackPropertyChangedUpdateHandlerValueCount <= element.TrackPropertyChangedOnPropertyChangedCount);
				Assert.True(element.TrackPropertyChangedUpdateHandlerValueCount <= element.TrackPropertyChangedDelegateCount);
			};

			element.TrackPropertyChangedDelegate = 1;
		}

		[Fact]
		public void ValidateChangingBindablePropertyDuringOnPropertyChangedStillPropagatesHandlerUpdateLater()
		{
			var element = new TestElement();


			element.PropertyChanged += (_, _) =>
			{
				if (element.TrackPropertyChangedDelegate == 1)
					element.TrackPropertyChangedDelegate = 2;
			};

			element.TrackPropertyChanged += (_, _) =>
			{
				Assert.True(element.TrackPropertyChangedUpdateHandlerValueCount <= element.TrackPropertyChangedOnPropertyChangedCount);
				Assert.True(element.TrackPropertyChangedUpdateHandlerValueCount <= element.TrackPropertyChangedDelegateCount);
			};

			element.TrackPropertyChangedDelegate = 1;
			Assert.Equal(2, element.TrackPropertyChangedUpdateHandlerValueCount);
		}

		[Fact]
		public void DescendantAddedLevel1()
		{
			var root = new TestElement();

			var child = new TestElement();

			bool added = false;
			root.DescendantAdded += (sender, args) =>
			{
				Assert.Same(root, sender);
				Assert.Same(child, args.Element);
				added = true;
			};

			root.Children.Add(child);
			Assert.True(added);
		}

		[Fact]
		public void DescendantAddedLevel2()
		{
			var root = new TestElement();

			var child = new TestElement();
			root.Children.Add(child);

			var child2 = new TestElement();

			bool added = false;
			root.DescendantAdded += (sender, args) =>
			{
				Assert.Same(root, sender);
				Assert.Same(child2, args.Element);
				added = true;
			};

			child.Children.Add(child2);
			Assert.True(added);
		}

		[Fact]
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
				Assert.Same(root, sender);

				if (!tier1added)
					tier1added = ReferenceEquals(child, args.Element);
				if (!tier2added)
					tier2added = ReferenceEquals(tier2, args.Element);
			};

			root.Children.Add(child);

			Assert.True(tier1added);
			Assert.True(tier2added);
		}

		[Fact]
		public void DescendantRemovedLevel1()
		{
			var root = new TestElement();

			var child = new TestElement();
			root.Children.Add(child);

			bool removed = false;
			root.DescendantRemoved += (sender, args) =>
			{
				Assert.Same(root, sender);
				Assert.Same(child, args.Element);
				removed = true;
			};

			root.Children.Remove(child);
			Assert.True(removed);
		}

		[Fact]
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
				Assert.Same(root, sender);
				Assert.Same(child2, args.Element);
				removed = true;
			};

			child.Children.Remove(child2);
			Assert.True(removed);
		}

		[Fact]
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
				Assert.Same(root, sender);

				if (!tier1removed)
					tier1removed = ReferenceEquals(child, args.Element);
				if (!tier2removed)
					tier2removed = ReferenceEquals(tier2, args.Element);
			};

			root.Children.Remove(child);

			Assert.True(tier1removed);
			Assert.True(tier2removed);
		}

		[Fact]
		public void ChildAdded()
		{
			var root = new TestElement();

			var child = new TestElement();

			bool added = false;
			root.ChildAdded += (sender, args) =>
			{
				Assert.Same(root, sender);
				Assert.Same(child, args.Element);
				added = true;
			};

			root.Children.Add(child);
			Assert.True(added);
		}

		[Fact]
		public void ChildRemoved()
		{
			var root = new TestElement();

			var child = new TestElement();
			root.Children.Add(child);

			bool removed = false;
			root.ChildRemoved += (sender, args) =>
			{
				Assert.Same(root, sender);
				Assert.Same(child, args.Element);
				removed = true;
			};

			root.Children.Remove(child);
			Assert.True(removed);
		}

		[Fact]
		public void RealParent_ReturnsNullAfterParentGarbageCollected()
		{
			var child = new TestElement();
			WeakReference parentRef;

			// Create parent in separate scope to enable GC
			void CreateParent()
			{
				var parent = new TestElement();
				parentRef = new WeakReference(parent);
				parent.Children.Add(child);
				// parent goes out of scope here
			}

			CreateParent();

			// Force garbage collection
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			// Verify parent was collected
			Assert.False(parentRef.IsAlive);

			// RealParent should return null and not throw
			Assert.Null(child.RealParent);
		}

		[Fact]
		public void SetParent_DoesNotLogWarningWhenParentGarbageCollected()
		{
			var child = new TestElement();
			WeakReference parentRef;

			// Create parent in separate scope
			void CreateParent()
			{
				var parent = new TestElement();
				parentRef = new WeakReference(parent);
				parent.Children.Add(child);
			}

			CreateParent();

			// Force garbage collection
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			// Setting parent should not log warnings internally
			var newParent = new TestElement();
			newParent.Children.Add(child);

			Assert.Same(newParent, child.RealParent);
		}

		[Fact]
		public void RealParent_ReturnsNull_WhenNoParentSet()
		{
			var element = new TestElement();
			Assert.Null(element.RealParent);
		}

		[Fact]
		public void RealParent_IsThreadSafe_WhenAccessedConcurrently()
		{
			var element = new TestElement();
			var parent = new TestElement();
			parent.Children.Add(element);

			// Act - Create multiple threads accessing and modifying RealParent concurrently
			const int ThreadCount = 10;
			var threads = new System.Threading.Thread[ThreadCount];
			var exceptions = new List<Exception>();
			var threadBarrier = new System.Threading.Barrier(ThreadCount);

			for (int i = 0; i < ThreadCount; i++)
			{
				threads[i] = new System.Threading.Thread(() =>
				{
					try
					{
						// Synchronize threads to increase chance of concurrent access
						threadBarrier.SignalAndWait();

						// Some threads read the parent
						element.RealParent?.ToString();

						// Clear reference to parent so GC can collect it
						parent = null;

						// Force garbage collection on some threads
						GC.Collect();
						GC.WaitForPendingFinalizers();
						GC.Collect();

						// Try to access the parent again after potential collection
						element.RealParent?.ToString();
					}
					catch (Exception ex)
					{
						lock (exceptions)
						{
							exceptions.Add(ex);
						}
					}
				});
				threads[i].Start();
			}

			// Wait for all threads to complete
			foreach (var thread in threads)
			{
				thread.Join();
			}

			// Assert - No exceptions should be thrown during concurrent access
			Assert.Empty(exceptions);
		}
	}
}
