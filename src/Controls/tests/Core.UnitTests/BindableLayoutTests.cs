using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

	public class BindableLayoutTests : BaseTestFixture
	{
		[Fact]
		public void TracksEmpty()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>();
			BindableLayout.SetItemsSource(layout, itemsSource);

			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void TracksAdd()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>();
			BindableLayout.SetItemsSource(layout, itemsSource);

			itemsSource.Add(1);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void TracksInsert()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>() { 0, 1, 2, 3, 4 };
			BindableLayout.SetItemsSource(layout, itemsSource);

			itemsSource.Insert(2, 5);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void TracksRemove()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>() { 0, 1 };
			BindableLayout.SetItemsSource(layout, itemsSource);

			itemsSource.RemoveAt(0);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));

			itemsSource.Remove(1);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void TracksRemoveAll()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableRangeCollection<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);

			itemsSource.RemoveAll();
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void TracksReplace()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>() { 0, 1, 2 };
			BindableLayout.SetItemsSource(layout, itemsSource);

			itemsSource[0] = 3;
			itemsSource[1] = 4;
			itemsSource[2] = 5;
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void TracksMove()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>() { 0, 1 };
			BindableLayout.SetItemsSource(layout, itemsSource);

			itemsSource.Move(0, 1);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));

			itemsSource.Move(1, 0);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void TracksClear()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>() { 0, 1 };
			BindableLayout.SetItemsSource(layout, itemsSource);

			itemsSource.Clear();
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void TracksNull()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));

			itemsSource = null;
			BindableLayout.SetItemsSource(layout, itemsSource);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void ItemTemplateIsSet()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);

			BindableLayout.SetItemTemplate(layout, new DataTemplateBoxView());

			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
			Assert.Equal(itemsSource.Count, layout.Children.Cast<BoxView>().Count());
		}

		[Fact]
		public void ItemTemplateSelectorIsSet()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);
			BindableLayout.SetItemTemplateSelector(layout, new DataTemplateSelectorFrame());

			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
			Assert.Equal(itemsSource.Count, layout.Children.Cast<Frame>().Count());
		}

		[Fact]
		public void ChangingTemplateRecreatesChildren()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);

			BindableLayout.SetItemTemplate(layout, new DataTemplate(() => new Label()));
			Assert.All(layout.Children, c => Assert.IsType<Label>(c));

			BindableLayout.SetItemTemplate(layout, new DataTemplate(() => new Button()));
			Assert.All(layout.Children, c => Assert.IsType<Button>(c));
		}

		[Fact]
		public void ChangingItemMaintainingTemplateUpdatesBindingContextOnly()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);
			BindableLayout.SetItemTemplate(layout, new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				return label;
			}));

			var firstChild = layout.Children[0] as Label;
			Assert.NotNull(firstChild);
			Assert.Equal("0", firstChild.Text);

			itemsSource[0] = 42;
			var firstChildAfterChange = layout.Children[0] as Label;
			Assert.NotNull(firstChildAfterChange);
			Assert.Equal("42", firstChildAfterChange.Text);
			Assert.Same(firstChild, firstChildAfterChange);
		}

		[Fact]
		public void ChangingItemsMaintainingTemplateUpdatesBindingContextOnly()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new List<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);
			BindableLayout.SetItemTemplate(layout, new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				return label;
			}));

			var children = layout.Children.Cast<Label>().ToList();
			BindableLayout.SetItemsSource(layout, new List<int>(Enumerable.Range(10, 10)));

			var childrenAfterChange = layout.Children.Cast<Label>().ToList();
			for (var i = 0; i < children.Count; i++)
			{
				Assert.Same(children[i], childrenAfterChange[i]);
				Assert.Equal((i + 10).ToString(), childrenAfterChange[i].Text);
			}
		}

		[Fact]
		public void ChangingItemsRemovesExceedingChildren()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new List<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);
			BindableLayout.SetItemTemplate(layout, new DataTemplate(() => new Label()));

			var children = layout.Children.ToList();
			BindableLayout.SetItemsSource(layout, new List<int>(Enumerable.Range(0, 5)));

			var childrenAfterChange = layout.Children.ToList();
			Assert.Equal(5, childrenAfterChange.Count);

			for (int i = 0; i < 5; i++)
			{
				Assert.Same(children[i], childrenAfterChange[i]);
			}

			for (int i = 5; i < 10; i++)
			{
				Assert.Null(children[i].BindingContext);
			}
		}

		[Fact]
		public void ContainerIsPassedInSelectTemplate()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);

			int containerPassedCount = 0;
			BindableLayout.SetItemTemplateSelector(layout, new MyDataTemplateSelectorTest((item, container) =>
			{
				if (container == layout)
					++containerPassedCount;
				return null;
			}));

			Assert.Equal(containerPassedCount, itemsSource.Count);
		}

		[Fact]
		public void ItemTemplateTakesPrecendenceOverItemTemplateSelector()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);
			BindableLayout.SetItemTemplate(layout, new DataTemplateBoxView());
			BindableLayout.SetItemTemplateSelector(layout, new DataTemplateSelectorFrame());

			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
			Assert.Equal(itemsSource.Count, layout.Children.Cast<BoxView>().Count());
		}

		[Fact]
		public void ItemsSourceTakePrecendenceOverLayoutChildren()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			layout.Children.Add(new Label());
			layout.Children.Add(new Label());
			layout.Children.Add(new Label());

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact, Category(TestCategory.Memory)]
		public async Task LayoutIsGarbageCollectedAfterItsRemoved()
		{
			var pageRoot = new Grid();
			var page = new ContentPage() { Content = pageRoot };

			WeakReference CreateReference()
			{
				var layout = new StackLayout { IsPlatformEnabled = true };

				var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
				BindableLayout.SetItemsSource(layout, itemsSource);
				pageRoot.Children.Add(layout);
				var reference = new WeakReference(layout);
				pageRoot.Children.Remove(layout);
				return reference;
			}

			var weakReference = CreateReference();

			await TestHelpers.Collect();

			Assert.False(weakReference.IsAlive);

			// Ensure that the ContentPage isn't collected during the test
			GC.KeepAlive(page);
		}

		[Fact]
		public void ThrowsExceptionOnUsingDataTemplateSelectorForItemTemplate()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);

			Assert.Throws<NotSupportedException>(() => BindableLayout.SetItemTemplate(layout, new DataTemplateSelectorFrame()));
		}

		[Fact]
		public void DontTrackAfterItemsSourceChanged()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			BindableLayout.SetItemsSource(layout, itemsSource);
			BindableLayout.SetItemsSource(layout, new ObservableCollection<int>(Enumerable.Range(0, 10)));

			bool wasCalled = false;
			layout.ChildAdded += (_, __) => wasCalled = true;
			itemsSource.Add(11);
			Assert.False(wasCalled);
		}

		[Fact]
		public void WorksWithNullItems()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int?>(Enumerable.Range(0, 10).Cast<int?>());
			itemsSource.Add(null);
			BindableLayout.SetItemsSource(layout, itemsSource);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void WorksWithDuplicateItems()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 10));
			foreach (int item in itemsSource.ToList())
			{
				itemsSource.Add(item);
			}

			BindableLayout.SetItemsSource(layout, itemsSource);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));

			itemsSource.Remove(0);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void RemovesEmptyViewWhenAddingTheFirstItem()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>();

			var emptyView = new Label();
			BindableLayout.SetEmptyView(layout, emptyView);

			BindableLayout.SetItemsSource(layout, itemsSource);
			Assert.Single(layout.Children);
			Assert.Equal(emptyView, layout[0]);

			itemsSource.Add(0);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));
		}

		[Fact]
		public void AddsEmptyViewWhenRemovingRemainingItem()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			var itemsSource = new ObservableCollection<int>(Enumerable.Range(0, 1));

			var emptyView = new Label();
			BindableLayout.SetEmptyView(layout, emptyView);

			BindableLayout.SetItemsSource(layout, itemsSource);
			Assert.True(IsLayoutWithItemsSource(itemsSource, layout));

			itemsSource.RemoveAt(0);
			Assert.Single(layout.Children);
			Assert.Equal(emptyView, layout[0]);
		}

		[Fact]
		public void ValidateBindableProperties()
		{
			var layout = new StackLayout
			{
				IsPlatformEnabled = true,
			};

			// EmptyView
			object emptyView = new object();
			BindableLayout.SetEmptyView(layout, emptyView);

			Assert.Equal(emptyView, BindableLayout.GetEmptyView(layout));
			Assert.Equal(emptyView, layout.GetValue(BindableLayout.EmptyViewProperty));

			// EmptyViewTemplateProperty
			DataTemplate emptyViewTemplate = new DataTemplate(typeof(Label));
			BindableLayout.SetEmptyViewTemplate(layout, emptyViewTemplate);

			Assert.Equal(emptyViewTemplate, BindableLayout.GetEmptyViewTemplate(layout));
			Assert.Equal(emptyViewTemplate, layout.GetValue(BindableLayout.EmptyViewTemplateProperty));


			// ItemsSourceProperty
			IEnumerable itemsSource = Array.Empty<object>();
			BindableLayout.SetItemsSource(layout, itemsSource);

			Assert.Equal(itemsSource, BindableLayout.GetItemsSource(layout));
			Assert.Equal(itemsSource, layout.GetValue(BindableLayout.ItemsSourceProperty));

			// ItemTemplateProperty
			DataTemplate itemTemplate = new DataTemplate(typeof(Label));
			BindableLayout.SetItemTemplate(layout, itemTemplate);

			Assert.Equal(itemTemplate, BindableLayout.GetItemTemplate(layout));
			Assert.Equal(itemTemplate, layout.GetValue(BindableLayout.ItemTemplateProperty));


			// ItemTemplateSelectorProperty
			var itemTemplateSelector = new DataTemplateSelectorFrame();
			BindableLayout.SetItemTemplateSelector(layout, itemTemplateSelector);

			Assert.Equal(itemTemplateSelector, BindableLayout.GetItemTemplateSelector(layout));
			Assert.Equal(itemTemplateSelector, layout.GetValue(BindableLayout.ItemTemplateSelectorProperty));
		}

		[Fact]
		public async Task ItemViewBindingContextIsSetToNullOnClear()
		{
			var list = new ObservableCollection<string> { "Foo" };

			var layout = new StackLayout { IsPlatformEnabled = true };
			BindableLayout.SetItemTemplate(layout, new DataTemplate(() => new Label()));
			BindableLayout.SetItemsSource(layout, list);

			// Verify that the item view is bound to collection item
			var itemView = layout.Children.FirstOrDefault() as Label;
			Assert.NotNull(itemView);
			Assert.Equal(list[0], itemView.BindingContext);

			// Clear collection by setting it to null
			BindableLayout.SetItemsSource(layout, null);

			// Verify item view binding context is null
			Assert.Null(itemView.BindingContext);
		}

		[Fact]
		public async Task ItemViewBindingContextIsSetToNullOnRemove()
		{
			var list = new ObservableCollection<string> { "Foo" };

			var layout = new StackLayout { IsPlatformEnabled = true };
			BindableLayout.SetItemTemplate(layout, new DataTemplate(() => new Label()));
			BindableLayout.SetItemsSource(layout, list);

			// Verify that the item view is bound to collection item
			var itemView = layout.Children.FirstOrDefault() as Label;
			Assert.NotNull(itemView);
			Assert.Equal(list[0], itemView.BindingContext);

			// Remove the item
			list.RemoveAt(0);

			// Verify item view binding context is null
			Assert.Null(itemView.BindingContext);
		}

		[Fact]
		public async Task EmptyViewTemplateContentInheritsLayoutBindingContext()
		{
			var list = new ObservableCollection<string>();

			var bindingContext = "Foo";

			var layout = new StackLayout { IsPlatformEnabled = true, BindingContext = bindingContext };
			BindableLayout.SetEmptyViewTemplate(layout, new DataTemplate(() => new Label()));
			BindableLayout.SetItemsSource(layout, list);

			// Verify that the empty view is bound to layout's binding context
			var emptyView = layout.Children.FirstOrDefault() as Label;
			Assert.NotNull(emptyView);
			Assert.Equal(bindingContext, emptyView.BindingContext);

			// Change binding context on layout
			layout.BindingContext = bindingContext = "Bar";

			// Verify empty view inherited the binding context
			Assert.Equal(bindingContext, emptyView.BindingContext);
		}

		[Fact]
		public async Task DoesNotLeak()
		{
			WeakReference controllerRef, proxyRef;
			var list = new ObservableCollection<string> { "Foo", "Bar", "Baz" };

			// Scope for BindableLayout
			{
				var layout = new StackLayout { IsPlatformEnabled = true };
				BindableLayout.SetItemTemplate(layout, new DataTemplate(() => new Label()));
				BindableLayout.SetItemsSource(layout, list);

				var controller = BindableLayout.GetBindableLayoutController(layout);
				controllerRef = new WeakReference(controller);

				// BindableLayoutController._collectionChangedProxy
				var flags = BindingFlags.NonPublic | BindingFlags.Instance;
				var proxy = controller.GetType().GetField("_collectionChangedProxy", flags).GetValue(controller);
				Assert.NotNull(proxy);
				proxyRef = new WeakReference(proxy);
			}

			// First GC
			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Assert.False(controllerRef.IsAlive, "BindableLayoutController should not be alive!");

			// Second GC
			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Assert.False(proxyRef.IsAlive, "WeakCollectionChangedProxy should not be alive!");
		}

		[Fact("BindableLayout Still Updates after a GC")]
		public async Task UpdatesAfterGC()
		{
			var list = new ObservableCollection<string> { "Foo", "Bar" };
			var layout = new StackLayout { IsPlatformEnabled = true };
			BindableLayout.SetItemTemplate(layout, new DataTemplate(() => new Label()));
			BindableLayout.SetItemsSource(layout, list);

			Assert.Equal(2, layout.Children.Count);

			await Task.Yield();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			list.Add("Baz");
			Assert.Equal(3, layout.Children.Count);
		}

		// Checks if for every item in the items source there's a corresponding view
		static bool IsLayoutWithItemsSource(IEnumerable itemsSource, Compatibility.Layout layout)
		{
			if (itemsSource == null)
			{
				return layout.Children.Count() == 0;
			}

			int i = 0;
			foreach (object item in itemsSource)
			{
				if (BindableLayout.GetItemTemplate(layout) is DataTemplate dataTemplate ||
					BindableLayout.GetItemTemplateSelector(layout) is DataTemplateSelector dataTemplateSelector)
				{
					if (!Equals(item, layout.Children[i].BindingContext))
					{
						return false;
					}
				}
				else
				{
					if (!Equals(item?.ToString(), ((Label)layout.Children[i]).Text))
					{
						return false;
					}
				}

				++i;
			}

			return layout.Children.Count == i;
		}

		class DataTemplateBoxView : DataTemplate
		{
			public DataTemplateBoxView() : base(() => new BoxView())
			{
			}
		}

		class DataTemplateFrame : DataTemplate
		{
			public DataTemplateFrame() : base(() => new Frame())
			{
			}
		}

		class DataTemplateSelectorFrame : DataTemplateSelector
		{
			DataTemplateFrame dt = new DataTemplateFrame();

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				return dt;
			}
		}

		class ObservableRangeCollection<T> : ObservableCollection<T>
		{
			public ObservableRangeCollection(IEnumerable<T> collection)
				: base(collection)
			{
			}

			public void RemoveAll()
			{
				CheckReentrancy();

				var changedItems = new List<T>(Items);
				foreach (var i in changedItems)
					Items.Remove(i);

				OnPropertyChanged(new PropertyChangedEventArgs("Count"));
				OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems, 0));
			}
		}

		class MyDataTemplateSelectorTest : DataTemplateSelector
		{
			readonly Func<object, BindableObject, DataTemplate> _func;

			public MyDataTemplateSelectorTest(Func<object, BindableObject, DataTemplate> func)
				=> _func = func;

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
				=> _func(item, container);
		}
	}
}
