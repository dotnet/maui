#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;


using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using NSubstitute;
using Xunit;

using StackLayout = Microsoft.Maui.Controls.Compatibility.StackLayout;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

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

            Assert.False(await controllerRef.WaitForCollect(), "BindableLayoutController should not be alive!");
            Assert.False(await proxyRef.WaitForCollect(), "WeakCollectionChangedProxy should not be alive!");
        }

        [Fact("BindableLayout Still Updates after a GC")]
        public async Task UpdatesAfterGC()
        {
            var list = new ObservableCollection<string> { "Foo", "Bar" };
            var layout = new StackLayout { IsPlatformEnabled = true };
            BindableLayout.SetItemTemplate(layout, new DataTemplate(() => new Label()));
            BindableLayout.SetItemsSource(layout, list);

            Assert.Equal(2, layout.Children.Count);

            await TestHelpers.Collect();

            list.Add("Baz");
            Assert.Equal(3, layout.Children.Count);
        }

        [Fact("BindableLayout disconnects handlers when removing views")]
        public void DisconnectsHandlersWhenRemovingViews()
        {
            var mauiApp = MauiApp.CreateBuilder()
                .UseMauiApp<ApplicationStub>()
                .ConfigureMauiHandlers(handlers => handlers.AddHandler<ContentPage, HandlerStub>())
                .ConfigureMauiHandlers(handlers => handlers.AddHandler<Button, HandlerStub>())
                .ConfigureMauiHandlers(handlers => handlers.AddHandler<VerticalStackLayout, BindableLayoutHandlerStub>())
                .Build();

            MauiContext mauiContext = new MauiContext(mauiApp.Services);

            var bindableLayout = new VerticalStackLayout();
            var items = new ObservableCollection<int>(Enumerable.Range(0, 10));
            BindableLayout.SetItemsSource(bindableLayout, items);
            BindableLayout.SetItemTemplate(bindableLayout, new DataTemplate(() => new Button()));
            BindableLayout.SetEmptyView(bindableLayout, new Button());

            bindableLayout.ToHandler(mauiContext);

            // Ensure we have the handlers on all elements
            Assert.All(bindableLayout.Children, c => Assert.NotNull(c.Handler));

            // Test removal of an item
            var lastChildIndex = items.Count - 1;
            var lastChild = bindableLayout[lastChildIndex];
            items.RemoveAt(lastChildIndex);
            Assert.Null(lastChild.Handler);

            // Test removal of all items
            var children = bindableLayout.Children.ToList();
            items.Clear();
            Assert.All(children, c => Assert.Null(c.Handler));

            // Test removal of empty view
            var emptyView = bindableLayout.FirstOrDefault() as Button;
            Assert.NotNull(emptyView);
            Assert.NotNull(emptyView.Handler);
            items.Add(1000);
            Assert.Null(emptyView.Handler);

            // Test replacing the items source with an empty enumerable
            lastChildIndex = 0;
            lastChild = bindableLayout[lastChildIndex];
            BindableLayout.SetItemsSource(bindableLayout, Enumerable.Empty<int>());
            Assert.Null(lastChild.Handler);
            Assert.NotNull(emptyView.Handler);
        }

        class BindableLayoutHandlerStub : HandlerStub
        {
            public static CommandMapper<IView, IViewHandler> CommandMapper = new()
            {
                [nameof(ILayoutHandler.Add)] = MapCreatePlatformHandler,
                [nameof(ILayoutHandler.Insert)] = MapCreatePlatformHandler,
            };

            static void MapCreatePlatformHandler(IViewHandler handler, IView view, object arg)
            {
                if (arg is LayoutHandlerUpdate args)
                {
                    args.View.ToHandler(handler.MauiContext!);
                }
            }

            public BindableLayoutHandlerStub() : base(new PropertyMapper<IView>(), CommandMapper)
            {
            }

            public override void SetVirtualView(IView view)
            {
                base.SetVirtualView(view);
                var bindableLayout = (IBindableLayout)view;
                foreach (var child in bindableLayout.Children.OfType<IView>())
                {
                    child.ToHandler(MauiContext!);
                }
            }
        }

        // Checks if for every item in the items source there's a corresponding view
        static bool IsLayoutWithItemsSource(IEnumerable itemsSource, Compatibility.Layout layout)
        {
            if (itemsSource == null)
            {
                return layout.InternalChildren.Count() == 0;
            }

            int i = 0;
            foreach (object item in itemsSource)
            {
                if (BindableLayout.GetItemTemplate(layout) is DataTemplate dataTemplate ||
                    BindableLayout.GetItemTemplateSelector(layout) is DataTemplateSelector dataTemplateSelector)
                {
                    if (!Equals(item, layout.InternalChildren[i].BindingContext))
                    {
                        return false;
                    }
                }
                else
                {
                    if (!Equals(item?.ToString(), ((Label)layout.InternalChildren[i]).Text))
                    {
                        return false;
                    }
                }

                ++i;
            }

            return layout.InternalChildren.Count == i;
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

        /// <summary>
        /// Tests RemoveAt method when layout implements IBindableLayout but not ILayout,
        /// verifying that RemoveAt is called on the Children collection.
        /// </summary>
        [Fact]
        public void RemoveAt_LayoutIsNotILayout_CallsRemoveAtOnChildren()
        {
            // Arrange
            var mockLayout = Substitute.For<IBindableLayout>();
            var mockChildren = Substitute.For<IList>();
            mockLayout.Children.Returns(mockChildren);
            int index = 1;

            // Act
            mockLayout.RemoveAt(index);

            // Assert
            mockChildren.Received(1).RemoveAt(index);
        }

        /// <summary>
        /// Tests RemoveAt method with various valid index values when layout is not ILayout.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(5)]
        [InlineData(10)]
        public void RemoveAt_LayoutIsNotILayoutWithValidIndex_CallsRemoveAtOnChildrenWithCorrectIndex(int index)
        {
            // Arrange
            var mockLayout = Substitute.For<IBindableLayout>();
            var mockChildren = Substitute.For<IList>();
            mockLayout.Children.Returns(mockChildren);

            // Act
            mockLayout.RemoveAt(index);

            // Assert
            mockChildren.Received(1).RemoveAt(index);
        }

        /// <summary>
        /// Tests RemoveAt method with negative index when layout is not ILayout,
        /// verifying that the exception from Children.RemoveAt is propagated.
        /// </summary>
        [Fact]
        public void RemoveAt_LayoutIsNotILayoutWithNegativeIndex_PropagatesException()
        {
            // Arrange
            var mockLayout = Substitute.For<IBindableLayout>();
            var mockChildren = Substitute.For<IList>();
            mockChildren.When(x => x.RemoveAt(-1)).Do(x => throw new ArgumentOutOfRangeException());
            mockLayout.Children.Returns(mockChildren);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => mockLayout.RemoveAt(-1));
        }

        /// <summary>
        /// Tests RemoveAt method with out of bounds index when layout is not ILayout,
        /// verifying that the exception from Children.RemoveAt is propagated.
        /// </summary>
        [Fact]
        public void RemoveAt_LayoutIsNotILayoutWithOutOfBoundsIndex_PropagatesException()
        {
            // Arrange
            var mockLayout = Substitute.For<IBindableLayout>();
            var mockChildren = Substitute.For<IList>();
            mockChildren.When(x => x.RemoveAt(100)).Do(x => throw new ArgumentOutOfRangeException());
            mockLayout.Children.Returns(mockChildren);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => mockLayout.RemoveAt(100));
        }

        /// <summary>
        /// Tests RemoveAt method with boundary integer values when layout is not ILayout.
        /// </summary>
        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void RemoveAt_LayoutIsNotILayoutWithBoundaryIndex_CallsRemoveAtOnChildren(int index)
        {
            // Arrange
            var mockLayout = Substitute.For<IBindableLayout>();
            var mockChildren = Substitute.For<IList>();
            mockLayout.Children.Returns(mockChildren);

            // Act
            mockLayout.RemoveAt(index);

            // Assert
            mockChildren.Received(1).RemoveAt(index);
        }

        /// <summary>
        /// Tests RemoveAt method when layout is null,
        /// verifying that a NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void RemoveAt_LayoutIsNull_ThrowsNullReferenceException()
        {
            // Arrange
            IBindableLayout layout = null;

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => layout.RemoveAt(0));
        }

        /// <summary>
        /// Tests RemoveAt method when layout implements both IBindableLayout and ILayout,
        /// verifying that RemoveAt is called on the ILayout interface.
        /// </summary>
        [Fact]
        public void RemoveAt_LayoutIsILayout_CallsRemoveAtOnILayout()
        {
            // Arrange
            var mockLayout = Substitute.For<IBindableLayout, ILayout>();
            var mauiLayout = (ILayout)mockLayout;
            int index = 2;

            // Act
            mockLayout.RemoveAt(index);

            // Assert
            mauiLayout.Received(1).RemoveAt(index);
        }

        /// <summary>
        /// Tests RemoveAt method when Children collection is null,
        /// verifying that a NullReferenceException is thrown.
        /// </summary>
        [Fact]
        public void RemoveAt_LayoutIsNotILayoutWithNullChildren_ThrowsNullReferenceException()
        {
            // Arrange
            var mockLayout = Substitute.For<IBindableLayout>();
            mockLayout.Children.Returns((IList)null);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => mockLayout.RemoveAt(0));
        }
    }

    public class BindableLayoutRemoveTests
    {
        /// <summary>
        /// Tests the Remove extension method when layout implements ILayout and item implements IView.
        /// Should call mauiLayout.Remove(view) and not fall back to Children.Remove.
        /// </summary>
        [Fact]
        public void Remove_LayoutImplementsILayoutAndItemImplementsIView_CallsMauiLayoutRemove()
        {
            // Arrange
            var layout = Substitute.For<IBindableLayout, ILayout>();
            var item = Substitute.For<IView>();
            var children = Substitute.For<IList>();
            layout.Children.Returns(children);
            ((ILayout)layout).Remove(item).Returns(true);

            // Act
            layout.Remove(item);

            // Assert
            ((ILayout)layout).Received(1).Remove(item);
            children.DidNotReceive().Remove(Arg.Any<object>());
        }

        /// <summary>
        /// Tests the Remove extension method when layout does not implement ILayout.
        /// Should fall back to Children.Remove regardless of item type.
        /// </summary>
        [Fact]
        public void Remove_LayoutDoesNotImplementILayout_FallsBackToChildrenRemove()
        {
            // Arrange
            var layout = Substitute.For<IBindableLayout>();
            var item = Substitute.For<IView>();
            var children = Substitute.For<IList>();
            layout.Children.Returns(children);

            // Act
            layout.Remove(item);

            // Assert
            children.Received(1).Remove(item);
        }

        /// <summary>
        /// Tests the Remove extension method when item does not implement IView.
        /// Should fall back to Children.Remove regardless of layout type.
        /// </summary>
        [Fact]
        public void Remove_ItemDoesNotImplementIView_FallsBackToChildrenRemove()
        {
            // Arrange
            var layout = Substitute.For<IBindableLayout, ILayout>();
            var item = new object();
            var children = Substitute.For<IList>();
            layout.Children.Returns(children);

            // Act
            layout.Remove(item);

            // Assert
            children.Received(1).Remove(item);
            ((ILayout)layout).DidNotReceive().Remove(Arg.Any<IView>());
        }

        /// <summary>
        /// Tests the Remove extension method when item is null.
        /// Should fall back to Children.Remove since null is not IView.
        /// </summary>
        [Fact]
        public void Remove_ItemIsNull_FallsBackToChildrenRemove()
        {
            // Arrange
            var layout = Substitute.For<IBindableLayout, ILayout>();
            object item = null;
            var children = Substitute.For<IList>();
            layout.Children.Returns(children);

            // Act
            layout.Remove(item);

            // Assert
            children.Received(1).Remove(null);
            ((ILayout)layout).DidNotReceive().Remove(Arg.Any<IView>());
        }

        /// <summary>
        /// Tests the Remove extension method with various combinations of layout and item types.
        /// Verifies correct branching logic for different input scenarios.
        /// </summary>
        [Theory]
        [InlineData(true, true, true)] // Layout is ILayout, Item is IView -> use mauiLayout.Remove
        [InlineData(true, false, false)] // Layout is ILayout, Item is not IView -> use Children.Remove
        [InlineData(false, true, false)] // Layout is not ILayout, Item is IView -> use Children.Remove
        [InlineData(false, false, false)] // Layout is not ILayout, Item is not IView -> use Children.Remove
        public void Remove_VariousLayoutAndItemCombinations_UsesCorrectRemoveMethod(bool layoutImplementsILayout, bool itemImplementsIView, bool shouldUseMauiLayoutRemove)
        {
            // Arrange
            var layout = layoutImplementsILayout
                ? Substitute.For<IBindableLayout, ILayout>()
                : Substitute.For<IBindableLayout>();

            var item = itemImplementsIView
                ? Substitute.For<IView>()
                : new object();

            var children = Substitute.For<IList>();
            layout.Children.Returns(children);

            if (layoutImplementsILayout)
            {
                ((ILayout)layout).Remove(Arg.Any<IView>()).Returns(true);
            }

            // Act
            layout.Remove(item);

            // Assert
            if (shouldUseMauiLayoutRemove)
            {
                ((ILayout)layout).Received(1).Remove((IView)item);
                children.DidNotReceive().Remove(Arg.Any<object>());
            }
            else
            {
                children.Received(1).Remove(item);
                if (layoutImplementsILayout)
                {
                    ((ILayout)layout).DidNotReceive().Remove(Arg.Any<IView>());
                }
            }
        }

        /// <summary>
        /// Tests the Remove extension method when mauiLayout.Remove returns false.
        /// Verifies that the return value is properly discarded and execution completes.
        /// </summary>
        [Fact]
        public void Remove_MauiLayoutRemoveReturnsFalse_DiscardsReturnValue()
        {
            // Arrange
            var layout = Substitute.For<IBindableLayout, ILayout>();
            var item = Substitute.For<IView>();
            var children = Substitute.For<IList>();
            layout.Children.Returns(children);
            ((ILayout)layout).Remove(item).Returns(false);

            // Act & Assert - Should not throw
            layout.Remove(item);

            // Assert
            ((ILayout)layout).Received(1).Remove(item);
            children.DidNotReceive().Remove(Arg.Any<object>());
        }
    }


    public partial class BindableLayoutControllerTests
    {
        /// <summary>
        /// Tests that the BindableLayoutController constructor properly initializes with a valid IBindableLayout.
        /// Verifies that the weak reference and collection changed event handler are correctly assigned.
        /// </summary>
        [Fact]
        public void Constructor_ValidLayout_InitializesFieldsCorrectly()
        {
            // Arrange
            var mockLayout = Substitute.For<IBindableLayout>();

            // Act
            var controller = new BindableLayoutController(mockLayout);

            // Assert
            Assert.NotNull(controller);

            // Access the private fields through reflection to verify initialization
            var layoutWeakRefField = typeof(BindableLayoutController)
                .GetField("_layoutWeakReference", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var collectionChangedHandlerField = typeof(BindableLayoutController)
                .GetField("_collectionChangedEventHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var weakRef = (WeakReference<IBindableLayout>)layoutWeakRefField.GetValue(controller);
            var handler = (NotifyCollectionChangedEventHandler)collectionChangedHandlerField.GetValue(controller);

            Assert.NotNull(weakRef);
            Assert.True(weakRef.TryGetTarget(out var retrievedLayout));
            Assert.Same(mockLayout, retrievedLayout);
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the BindableLayoutController constructor properly handles null layout parameter.
        /// Verifies that a weak reference is still created even with null target.
        /// </summary>
        [Fact]
        public void Constructor_NullLayout_InitializesWithNullWeakReference()
        {
            // Arrange & Act
            var controller = new BindableLayoutController(null);

            // Assert
            Assert.NotNull(controller);

            // Access the private fields through reflection to verify initialization
            var layoutWeakRefField = typeof(BindableLayoutController)
                .GetField("_layoutWeakReference", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var collectionChangedHandlerField = typeof(BindableLayoutController)
                .GetField("_collectionChangedEventHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var weakRef = (WeakReference<IBindableLayout>)layoutWeakRefField.GetValue(controller);
            var handler = (NotifyCollectionChangedEventHandler)collectionChangedHandlerField.GetValue(controller);

            Assert.NotNull(weakRef);
            Assert.False(weakRef.TryGetTarget(out var retrievedLayout));
            Assert.Null(retrievedLayout);
            Assert.NotNull(handler);
        }

        /// <summary>
        /// Tests that the BindableLayoutController constructor assigns the ItemsSourceCollectionChanged method
        /// to the collection changed event handler field correctly.
        /// </summary>
        [Fact]
        public void Constructor_ValidLayout_AssignsCorrectEventHandler()
        {
            // Arrange
            var mockLayout = Substitute.For<IBindableLayout>();

            // Act
            var controller = new BindableLayoutController(mockLayout);

            // Assert
            var collectionChangedHandlerField = typeof(BindableLayoutController)
                .GetField("_collectionChangedEventHandler", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var handler = (NotifyCollectionChangedEventHandler)collectionChangedHandlerField.GetValue(controller);

            // Verify the handler is pointing to the ItemsSourceCollectionChanged method
            Assert.NotNull(handler);
            Assert.Equal("ItemsSourceCollectionChanged", handler.Method.Name);
            Assert.Same(controller, handler.Target);
        }

        /// <summary>
        /// Tests that the WeakReference created in the constructor can be successfully used
        /// to retrieve the target layout object.
        /// </summary>
        [Fact]
        public void Constructor_ValidLayout_WeakReferenceCanRetrieveTarget()
        {
            // Arrange
            var mockLayout = Substitute.For<IBindableLayout>();
            var mockChildren = Substitute.For<IList>();
            mockLayout.Children.Returns(mockChildren);

            // Act
            var controller = new BindableLayoutController(mockLayout);

            // Assert
            var layoutWeakRefField = typeof(BindableLayoutController)
                .GetField("_layoutWeakReference", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var weakRef = (WeakReference<IBindableLayout>)layoutWeakRefField.GetValue(controller);

            Assert.True(weakRef.TryGetTarget(out var retrievedLayout));
            Assert.Same(mockLayout, retrievedLayout);
            Assert.Same(mockChildren, retrievedLayout.Children);
        }

        /// <summary>
        /// Tests that multiple BindableLayoutController instances can be created with the same layout
        /// and each maintains its own independent weak reference.
        /// </summary>
        [Fact]
        public void Constructor_SameLayoutMultipleControllers_EachHasIndependentWeakReference()
        {
            // Arrange
            var mockLayout = Substitute.For<IBindableLayout>();

            // Act
            var controller1 = new BindableLayoutController(mockLayout);
            var controller2 = new BindableLayoutController(mockLayout);

            // Assert
            var layoutWeakRefField = typeof(BindableLayoutController)
                .GetField("_layoutWeakReference", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var weakRef1 = (WeakReference<IBindableLayout>)layoutWeakRefField.GetValue(controller1);
            var weakRef2 = (WeakReference<IBindableLayout>)layoutWeakRefField.GetValue(controller2);

            Assert.NotSame(weakRef1, weakRef2);

            Assert.True(weakRef1.TryGetTarget(out var layout1));
            Assert.True(weakRef2.TryGetTarget(out var layout2));
            Assert.Same(mockLayout, layout1);
            Assert.Same(mockLayout, layout2);
            Assert.Same(layout1, layout2);
        }
    }
}