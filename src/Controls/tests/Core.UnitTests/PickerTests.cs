using System;
using System.Collections.ObjectModel;
using System.Globalization;
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
	}
}
