using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class PickerTests : BaseTestFixture
	{
		class PickerTestsContextFixture
		{
			public class PickerTestsNestedClass
			{
				public string Nested { get; set; }
			}

			public PickerTestsNestedClass Nested { get; set; }

			public string DisplayName { get; set; }

			public string ComplexName { get; set; }

			public PickerTestsContextFixture(string displayName, string complexName)
			{
				DisplayName = displayName;
				ComplexName = complexName;
			}

			public PickerTestsContextFixture()
			{
			}
		}

		class PickerTestsBindingContext
		{
			public ObservableCollection<object> Items { get; set; }

			public object SelectedItem { get; set; }
		}

		class PickerTestValueConverter : IValueConverter
		{
			public bool ConvertCalled { get; private set; }

			public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
			{
				ConvertCalled = true;
				var cf = (PickerTestsContextFixture)value;
				return cf.DisplayName;
			}

			public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
			{
				throw new NotImplementedException();
			}
		}

		class ObservableRangeCollection<T> : ObservableCollection<T>
		{
			static readonly PropertyChangedEventArgs CountChangedArgs = new(nameof(Count));
			static readonly PropertyChangedEventArgs IndexerChangedArgs = new("Item[]");

			public void InsertRange(int index, IEnumerable<T> items)
			{
				CheckReentrancy();
				int currIndex = index;
				foreach (T item in items)
				{
					Items.Insert(currIndex++, item);
				}

				OnPropertyChanged(CountChangedArgs);
				OnPropertyChanged(IndexerChangedArgs);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items.ToList(), index));
			}

			public void RemoveRange(int index, int count)
			{
				CheckReentrancy();
				T[] removeItems = new T[count];
				for (int i = 0; i < count; i++)
				{
					// Always remove at index, since removing each item at index shifts the next item to that spot
					removeItems[i] = Items[index];
					Items.RemoveAt(index);
				}

				OnPropertyChanged(CountChangedArgs);
				OnPropertyChanged(IndexerChangedArgs);
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, removeItems, index));
			}
		}

		[Fact]
		public void TestSetSelectedIndexOnNullRows()
		{
			var picker = new Picker();

			Assert.Empty(picker.Items);
			Assert.Equal(-1, picker.SelectedIndex);

			picker.SelectedIndex = 2;

			Assert.Equal(-1, picker.SelectedIndex);
		}

		[Fact]
		public void TestSelectedIndexInRange()
		{
			var picker = new Picker
			{
				Items = { "John", "Paul", "George", "Ringo" },
				SelectedIndex = 2
			};

			Assert.Equal(2, picker.SelectedIndex);

			picker.SelectedIndex = 42;
			Assert.Equal(3, picker.SelectedIndex);

			picker.SelectedIndex = -1;
			Assert.Equal(-1, picker.SelectedIndex);

			picker.SelectedIndex = -42;
			Assert.Equal(-1, picker.SelectedIndex);
		}

		[Fact]
		public void TestSelectedIndexInRangeDefaultSelectedIndex()
		{
			var picker = new Picker
			{
				Items = { "John", "Paul", "George", "Ringo" }
			};

			Assert.Equal(-1, picker.SelectedIndex);

			picker.SelectedIndex = -5;
			Assert.Equal(-1, picker.SelectedIndex);

			picker.SelectedIndex = 2;
			Assert.Equal(2, picker.SelectedIndex);

			picker.SelectedIndex = 42;
			Assert.Equal(3, picker.SelectedIndex);

			picker.SelectedIndex = -1;
			Assert.Equal(-1, picker.SelectedIndex);

			picker.SelectedIndex = -42;
			Assert.Equal(-1, picker.SelectedIndex);
		}

		[Fact]
		public void TestSelectedIndexChangedOnCollectionShrink()
		{
			var picker = new Picker { Items = { "John", "Paul", "George", "Ringo" }, SelectedIndex = 3 };

			Assert.Equal(3, picker.SelectedIndex);

			picker.Items.RemoveAt(3);
			picker.Items.RemoveAt(2);

			Assert.Equal(1, picker.SelectedIndex);

			picker.Items.Clear();
			Assert.Equal(-1, picker.SelectedIndex);
		}

		[Fact]
		public void TestSelectedIndexOutOfRangeUpdatesSelectedItem()
		{
			var picker = new Picker
			{
				ItemsSource = new ObservableCollection<string>
				{
					"Monkey",
					"Banana",
					"Lemon"
				},
				SelectedIndex = 0
			};
			Assert.Equal("Monkey", picker.SelectedItem);
			picker.SelectedIndex = 42;
			Assert.Equal("Lemon", picker.SelectedItem);
			picker.SelectedIndex = -42;
			Assert.Null(picker.SelectedItem);
		}

		[Fact]
		public void TestUnsubscribeINotifyCollectionChanged()
		{
			var list = new ObservableCollection<string>();
			var picker = new Picker
			{
				ItemsSource = list
			};
			Assert.Empty(picker.Items);
			var newList = new ObservableCollection<string>();
			picker.ItemsSource = newList;
			list.Add("item");
			Assert.Empty(picker.Items);
		}

		[Fact]
		public void TestEmptyCollectionResetItems()
		{
			var list = new ObservableCollection<string>
			{
				"John",
				"George",
				"Ringo"
			};
			var picker = new Picker
			{
				ItemsSource = list
			};
			Assert.Equal(3, picker.Items.Count);
			picker.ItemsSource = new ObservableCollection<string>();
			Assert.Empty(picker.Items);
		}

		[Fact]
		public void TestSetItemsSourceProperty()
		{
			var items = new ObservableCollection<object>
			{
				new { Name = "John" },
				"Paul",
				"Ringo",
				0,
				new DateTime(1970, 1, 1),
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items
			};
			Assert.Equal(5, picker.Items.Count);
			Assert.Equal("John", picker.Items[0]);
			Assert.Null(picker.Items[3]);
		}

		[Fact]
		public void TestDisplayConverter()
		{
			var obj = new PickerTestsContextFixture("John", "John Doe");
			var converter = new PickerTestValueConverter();
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding(Binding.SelfPath, converter: converter),
				ItemsSource = new ObservableCollection<object>
				{
					obj
				}
			};
			Assert.True(converter.ConvertCalled);
			Assert.Equal("John", picker.Items[0]);
		}

		[Fact]
		public void TestItemsSourceCollectionChangedAppend()
		{
			var items = new ObservableCollection<object>
			{
				new { Name = "John" },
				"Paul",
				"Ringo"
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items,
				SelectedIndex = 0
			};
			Assert.Equal(3, picker.Items.Count);
			Assert.Equal("John", picker.Items[0]);
			items.Add(new { Name = "George" });
			Assert.Equal(4, picker.Items.Count);
			Assert.Equal("George", picker.Items[picker.Items.Count - 1]);
		}

		[Fact]
		public void TestItemsSourceCollectionChangedClear()
		{
			var items = new ObservableCollection<object>
			{
				new { Name = "John" },
				"Paul",
				"Ringo"
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items,
				SelectedIndex = 0
			};
			Assert.Equal(3, picker.Items.Count);
			items.Clear();
			Assert.Empty(picker.Items);
		}

		[Fact]
		public void TestItemsSourceCollectionChangedInsert()
		{
			var items = new ObservableCollection<object>
			{
				new { Name = "John" },
				"Paul",
				"Ringo"
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items,
				SelectedIndex = 0
			};
			Assert.Equal(3, picker.Items.Count);
			Assert.Equal("John", picker.Items[0]);
			items.Insert(1, new { Name = "George" });
			Assert.Equal(4, picker.Items.Count);
			Assert.Equal("George", picker.Items[1]);
		}

		[Fact]
		public void TestItemsSourceCollectionChangedReAssign()
		{
			var items = new ObservableCollection<object>
			{
				new { Name = "John" },
				"Paul",
				"Ringo"
			};
			var bindingContext = new { Items = items };
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				BindingContext = bindingContext
			};
			picker.SetBinding(Picker.ItemsSourceProperty, "Items");
			Assert.Equal(3, picker.Items.Count);
			items = new ObservableCollection<object>
			{
				"Peach",
				"Orange"
			};
			picker.BindingContext = new { Items = items };
			picker.ItemDisplayBinding = null;
			Assert.Equal(2, picker.Items.Count);
			Assert.Equal("Peach", picker.Items[0]);
		}

		[Fact]
		public void TestItemsSourceCollectionChangedRemove()
		{
			var items = new ObservableCollection<object>
			{
				new { Name = "John" },
				new { Name = "Paul" },
				new { Name = "Ringo"},
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items,
				SelectedIndex = 0
			};
			Assert.Equal(3, picker.Items.Count);
			Assert.Equal("John", picker.Items[0]);
			items.RemoveAt(1);
			Assert.Equal(2, picker.Items.Count);
			Assert.Equal("Ringo", picker.Items[1]);
		}

		[Theory]
		[InlineData(0, new string[] { "George" })]
		[InlineData(1, new string[] { "George" })]
		[InlineData(2, new string[] { "George" })]
		[InlineData(3, new string[] { "George" })]
		[InlineData(0, new string[] { "George", "Pete" })]
		[InlineData(1, new string[] { "George", "Pete" })]
		[InlineData(2, new string[] { "George", "Pete" })]
		[InlineData(3, new string[] { "George", "Pete" })]
		public void TestItemsSourceCollectionChangedInsertBeforeSelected(int insertionIndex, string[] insertNames)
		{
			var items = new ObservableRangeCollection<object>
			{
				new { Name = "John" },
				new { Name = "Paul" },
				new { Name = "Ringo" }
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items,
				SelectedIndex = 1
			};
			var originalSelectedItem = picker.SelectedItem;
			items.InsertRange(insertionIndex, insertNames.Select(name => new { Name = name }));
			Assert.Equal(3 + insertNames.Length, picker.Items.Count);
			// The selected item should remain the same, but the index should update
			Assert.Equal(originalSelectedItem, picker.SelectedItem);
			Assert.Equal(items.IndexOf(originalSelectedItem), picker.SelectedIndex);
		}

		[Theory]
		// Cases where removed items do NOT include the selected item (Paul at index 1)
		[InlineData(0, 1, true)]  // Remove John - Paul should be preserved
		[InlineData(2, 1, true)]  // Remove Ringo - Paul should be preserved
		[InlineData(2, 2, true)]  // Remove Ringo and George - Paul should be preserved
		// Cases where removed items include the selected item
		[InlineData(1, 1, false)] // Remove Paul - selection changes
		[InlineData(0, 2, false)] // Remove John and Paul - selection changes
		[InlineData(1, 2, false)] // Remove Paul and Ringo - selection changes
		public void TestItemsSourceCollectionChangedRemoveBeforeSelected(int removeIndex, int removeCount, bool selectedItemPreserved)
		{
			var items = new ObservableRangeCollection<object>
			{
				new { Name = "John" },
				new { Name = "Paul" },
				new { Name = "Ringo" },
				new { Name = "George" }
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items,
				SelectedIndex = 1
			};
			var originalSelectedItem = picker.SelectedItem;
			items.RemoveRange(removeIndex, removeCount);

			Assert.Equal(4 - removeCount, picker.Items.Count);
			if (selectedItemPreserved)
			{
				// The selected item should remain the same, but the index should update
				Assert.Equal(originalSelectedItem, picker.SelectedItem);
				Assert.Equal(items.IndexOf(originalSelectedItem), picker.SelectedIndex);
			}
			else
			{
				// The selected item was removed, so a different item is now selected
				Assert.NotEqual(originalSelectedItem, picker.SelectedItem);
			}
		}

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		public void TestItemsSourceCollectionChangedRemoveAtEndSelected(int removeCount)
		{
			var items = new ObservableRangeCollection<object>
			{
				new { Name = "John" },
				new { Name = "Paul" },
				new { Name = "Ringo" },
				new { Name = "George" }
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items,
				SelectedIndex = 4 - removeCount
			};
			items.RemoveRange(4 - removeCount, removeCount);

			Assert.Equal(4 - removeCount, picker.Items.Count);
			Assert.Equal(items.Count - 1, picker.SelectedIndex);
			Assert.Equal(items[^1], picker.SelectedItem);
		}

		[Fact]
		public void TestSelectedIndexAssignedItemsSourceCollectionChangedAppend()
		{
			var items = new ObservableCollection<object>
			{
				new { Name = "John" },
				new { Name = "Paul" },
				"Ringo"
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items,
			};

			picker.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(Picker.SelectedIndex))
				{
					items.Add(new { Name = "George" });
				}
			};

			picker.SelectedIndex = 1;

			Assert.Equal(4, picker.Items.Count);
			Assert.Equal("George", picker.Items[picker.Items.Count - 1]);
			Assert.Equal(1, picker.SelectedIndex);
			Assert.Equal(items[1], picker.SelectedItem);
		}

		[Fact]
		public void TestSelectedIndexAssignedItemsSourceCollectionChangedClear()
		{
			var items = new ObservableCollection<object>
			{
				new { Name = "John" },
				"Paul",
				"Ringo"
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items,
			};

			picker.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(Picker.SelectedIndex))
				{
					items.Clear();
				}
			};

			picker.SelectedIndex = 1;

			Assert.Empty(picker.Items);
			Assert.Equal(-1, picker.SelectedIndex);
			Assert.Null(picker.SelectedItem);
		}

		[Fact]
		public void TestSelectedIndexAssignedItemsSourceCollectionChangedInsert()
		{
			var items = new ObservableCollection<object>
			{
				new { Name = "John" },
				"Paul",
				"Ringo"
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items,
			};

			picker.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(Picker.SelectedIndex))
				{
					items.Insert(1, new { Name = "George" });
				}
			};

			picker.SelectedIndex = 2;

			Assert.Equal(4, picker.Items.Count);
			Assert.Equal("George", picker.Items[1]);
			Assert.Equal(2, picker.SelectedIndex);
			Assert.Equal(items[2], picker.SelectedItem);
		}

		[Fact]
		public void TestSelectedIndexAssignedItemsSourceCollectionChangedReAssign()
		{
			var items = new ObservableCollection<object>
			{
				new { Name = "John" },
				"Paul",
				"Ringo"
			};
			var bindingContext = new { Items = items };
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				BindingContext = bindingContext
			};
			picker.SetBinding(Picker.ItemsSourceProperty, "Items");

			picker.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(Picker.SelectedIndex))
				{
					items = new ObservableCollection<object>
					{
						"Peach",
						"Orange"
					};
					picker.BindingContext = new { Items = items };
					picker.ItemDisplayBinding = null;
				}
			};

			picker.SelectedIndex = 1;

			Assert.Equal(2, picker.Items.Count);
			Assert.Equal("Peach", picker.Items[0]);
			Assert.Equal(1, picker.SelectedIndex);
			Assert.Equal("Orange", picker.SelectedItem);
		}

		[Fact]
		public void TestSelectedItemAssignedItemsSourceCollectionChangedRemove()
		{
			var items = new ObservableCollection<object>
			{
				new { Name = "John" },
				new { Name = "Paul" },
				new { Name = "Ringo"},
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Name"),
				ItemsSource = items,
			};

			picker.PropertyChanged += (sender, e) =>
			{
				if (e.PropertyName == nameof(Picker.SelectedIndex))
				{
					items.RemoveAt(1);
				}
			};

			picker.SelectedIndex = 1;

			Assert.Equal(2, picker.Items.Count);
			Assert.Equal("Ringo", picker.Items[1]);
			Assert.Equal(1, picker.SelectedIndex);
			Assert.Equal(items[1], picker.SelectedItem);
		}

		[Fact]
		public void SettingSelectedIndexUpdatesSelectedItem()
		{
			var source = Enum.GetNames(typeof(HorizontalAlignment));

			var picker = new Picker
			{
				WidthRequest = 200,
				ItemsSource = source,
				SelectedItem = source[0]
			};

			picker.SelectedIndex = 1;
			Assert.Equal(source[1], picker.SelectedItem);
		}


		[Fact]
		public void TestItemsSourceCollectionOfStrings()
		{
			var items = new ObservableCollection<string>
			{
				"John",
				"Paul",
				"Ringo"
			};
			var picker = new Picker
			{
				ItemsSource = items,
				SelectedIndex = 0
			};
			Assert.Equal(3, picker.Items.Count);
			Assert.Equal("John", picker.Items[0]);
		}

		[Fact]
		public void TestSelectedItemDefault()
		{
			var bindingContext = new PickerTestsBindingContext
			{
				Items = new ObservableCollection<object>
				{
					new PickerTestsContextFixture("John", "John")
				}
			};
			var picker = new Picker
			{
				BindingContext = bindingContext
			};
			picker.SetBinding(Picker.ItemsSourceProperty, "Items");
			picker.SetBinding(Picker.SelectedItemProperty, "SelectedItem");
			Assert.Single(picker.Items);
			Assert.Equal(-1, picker.SelectedIndex);
			Assert.Equal(bindingContext.SelectedItem, picker.SelectedItem);
		}

		[Fact]
		public void ThrowsWhenModifyingItemsIfItemsSourceIsSet()
		{
			var picker = new Picker
			{
				ItemsSource = new System.Collections.Generic.List<object>()
			};
			Assert.Throws<InvalidOperationException>(() => picker.Items.Add("foo"));
		}

		[Fact]
		public void TestNestedDisplayMemberPathExpression()
		{
			var obj = new PickerTestsContextFixture
			{
				Nested = new PickerTestsContextFixture.PickerTestsNestedClass
				{
					Nested = "NestedProperty"
				}
			};
			var picker = new Picker
			{
				ItemDisplayBinding = new Binding("Nested.Nested"),
				ItemsSource = new ObservableCollection<object>
				{
					obj
				},
				SelectedIndex = 0
			};
			Assert.Equal("NestedProperty", picker.Items[0]);
		}

		[Fact]
		public void TestItemsSourceEnums()
		{
			var picker = new Picker
			{
				ItemsSource = new ObservableCollection<TextAlignment>
				{
					TextAlignment.Start,
					TextAlignment.Center,
					TextAlignment.End
				},
				SelectedIndex = 0
			};
			Assert.Equal("Start", picker.Items[0]);
		}

		[Fact]
		public void TestSelectedItemSet()
		{
			var obj = new PickerTestsContextFixture("John", "John");
			var bindingContext = new PickerTestsBindingContext
			{
				Items = new ObservableCollection<object>
				{
					obj
				},
				SelectedItem = obj
			};
			var picker = new Picker
			{
				BindingContext = bindingContext,
				ItemDisplayBinding = new Binding("DisplayName"),
			};
			picker.SetBinding(Picker.ItemsSourceProperty, "Items");
			picker.SetBinding(Picker.SelectedItemProperty, "SelectedItem");
			Assert.Single(picker.Items);
			Assert.Equal(0, picker.SelectedIndex);
			Assert.Equal(obj, picker.SelectedItem);
		}

		[Fact]
		public void TestSelectedItemChangeSelectedIndex()
		{
			var obj = new PickerTestsContextFixture("John", "John");
			var bindingContext = new PickerTestsBindingContext
			{
				Items = new ObservableCollection<object>
				{
					obj
				},
			};
			var picker = new Picker
			{
				BindingContext = bindingContext,
				ItemDisplayBinding = new Binding("DisplayName"),
			};
			picker.SetBinding(Picker.ItemsSourceProperty, "Items");
			picker.SetBinding(Picker.SelectedItemProperty, "SelectedItem");
			Assert.Single(picker.Items);
			Assert.Equal(-1, picker.SelectedIndex);
			Assert.Null(picker.SelectedItem);

			picker.SelectedItem = obj;
			Assert.Equal(0, picker.SelectedIndex);
			Assert.Equal(obj, picker.SelectedItem);
		}

		[Fact]
		public void NullItemReturnsEmptyStringFromInterface()
		{
			var picker = new Picker
			{
				ItemsSource = new ObservableCollection<object>
				{
					(string)null, "John Doe"
				}
			};

			var thing = (picker as IPicker).GetItem(0);
			Assert.NotNull(thing);
		}

		// https://github.com/dotnet/maui/issues/29235
		[Fact]
		public void PickerPreservesSelectedItemAfterRemovingItemBeforeSelection()
		{
			// Arrange: Create a picker with items and select "Item2" (index 2)
			var items = new ObservableCollection<string> { "Item0", "Item1", "Item2" };
			var picker = new Picker
			{
				ItemsSource = items,
				SelectedIndex = 2 // Select "Item2"
			};

			Assert.Equal("Item2", picker.SelectedItem);
			Assert.Equal(2, picker.SelectedIndex);

			// Act: Remove the first item (before the selection)
			items.RemoveAt(0);

			// Assert: SelectedItem should still be "Item2", but index should now be 1
			Assert.Equal("Item2", picker.SelectedItem);
			Assert.Equal(1, picker.SelectedIndex);
		}

		// https://github.com/dotnet/maui/issues/29235
		[Fact]
		public void PickerPreservesSelectedItemAfterInsertingItemBeforeSelection()
		{
			// Arrange: Create a picker with items and select "Dog" (index 1)
			var items = new ObservableCollection<string> { "Cat", "Dog", "Rabbit" };
			var picker = new Picker
			{
				ItemsSource = items,
				SelectedIndex = 1 // Select "Dog"
			};

			Assert.Equal("Dog", picker.SelectedItem);
			Assert.Equal(1, picker.SelectedIndex);

			// Act: Insert an item at the beginning
			items.Insert(0, "Goat");

			// Assert: SelectedItem should still be "Dog", and index should now be 2
			Assert.Equal("Dog", picker.SelectedItem);
			Assert.Equal(2, picker.SelectedIndex);
		}
	}
}
