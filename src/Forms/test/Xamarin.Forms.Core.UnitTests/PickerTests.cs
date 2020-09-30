using System;
using System.Collections.ObjectModel;
using System.Globalization;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
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

		[Test]
		public void TestSetSelectedIndexOnNullRows()
		{
			var picker = new Picker();

			Assert.IsEmpty(picker.Items);
			Assert.AreEqual(-1, picker.SelectedIndex);

			picker.SelectedIndex = 2;

			Assert.AreEqual(-1, picker.SelectedIndex);
		}

		[Test]
		public void TestSelectedIndexInRange()
		{
			var picker = new Picker
			{
				Items = { "John", "Paul", "George", "Ringo" },
				SelectedIndex = 2
			};

			Assert.AreEqual(2, picker.SelectedIndex);

			picker.SelectedIndex = 42;
			Assert.AreEqual(3, picker.SelectedIndex);

			picker.SelectedIndex = -1;
			Assert.AreEqual(-1, picker.SelectedIndex);

			picker.SelectedIndex = -42;
			Assert.AreEqual(-1, picker.SelectedIndex);
		}

		[Test]
		public void TestSelectedIndexInRangeDefaultSelectedIndex()
		{
			var picker = new Picker
			{
				Items = { "John", "Paul", "George", "Ringo" }
			};

			Assert.AreEqual(-1, picker.SelectedIndex);

			picker.SelectedIndex = -5;
			Assert.AreEqual(-1, picker.SelectedIndex);

			picker.SelectedIndex = 2;
			Assert.AreEqual(2, picker.SelectedIndex);

			picker.SelectedIndex = 42;
			Assert.AreEqual(3, picker.SelectedIndex);

			picker.SelectedIndex = -1;
			Assert.AreEqual(-1, picker.SelectedIndex);

			picker.SelectedIndex = -42;
			Assert.AreEqual(-1, picker.SelectedIndex);
		}

		[Test]
		public void TestSelectedIndexChangedOnCollectionShrink()
		{
			var picker = new Picker { Items = { "John", "Paul", "George", "Ringo" }, SelectedIndex = 3 };

			Assert.AreEqual(3, picker.SelectedIndex);

			picker.Items.RemoveAt(3);
			picker.Items.RemoveAt(2);

			Assert.AreEqual(1, picker.SelectedIndex);

			picker.Items.Clear();
			Assert.AreEqual(-1, picker.SelectedIndex);
		}

		[Test]
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
			Assert.AreEqual("Monkey", picker.SelectedItem);
			picker.SelectedIndex = 42;
			Assert.AreEqual("Lemon", picker.SelectedItem);
			picker.SelectedIndex = -42;
			Assert.IsNull(picker.SelectedItem);
		}

		[Test]
		public void TestUnsubscribeINotifyCollectionChanged()
		{
			var list = new ObservableCollection<string>();
			var picker = new Picker
			{
				ItemsSource = list
			};
			Assert.AreEqual(0, picker.Items.Count);
			var newList = new ObservableCollection<string>();
			picker.ItemsSource = newList;
			list.Add("item");
			Assert.AreEqual(0, picker.Items.Count);
		}

		[Test]
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
			Assert.AreEqual(3, picker.Items.Count);
			picker.ItemsSource = new ObservableCollection<string>();
			Assert.AreEqual(0, picker.Items.Count);
		}

		[Test]
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
			Assert.AreEqual(5, picker.Items.Count);
			Assert.AreEqual("John", picker.Items[0]);
			Assert.AreEqual(null, picker.Items[3]);
		}

		[Test]
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
			Assert.IsTrue(converter.ConvertCalled);
			Assert.AreEqual("John", picker.Items[0]);
		}

		[Test]
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
			Assert.AreEqual(3, picker.Items.Count);
			Assert.AreEqual("John", picker.Items[0]);
			items.Add(new { Name = "George" });
			Assert.AreEqual(4, picker.Items.Count);
			Assert.AreEqual("George", picker.Items[picker.Items.Count - 1]);
		}

		[Test]
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
			Assert.AreEqual(3, picker.Items.Count);
			items.Clear();
			Assert.AreEqual(0, picker.Items.Count);
		}

		[Test]
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
			Assert.AreEqual(3, picker.Items.Count);
			Assert.AreEqual("John", picker.Items[0]);
			items.Insert(1, new { Name = "George" });
			Assert.AreEqual(4, picker.Items.Count);
			Assert.AreEqual("George", picker.Items[1]);
		}

		[Test]
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
			Assert.AreEqual(3, picker.Items.Count);
			items = new ObservableCollection<object>
			{
				"Peach",
				"Orange"
			};
			picker.BindingContext = new { Items = items };
			picker.ItemDisplayBinding = null;
			Assert.AreEqual(2, picker.Items.Count);
			Assert.AreEqual("Peach", picker.Items[0]);
		}

		[Test]
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
			Assert.AreEqual(3, picker.Items.Count);
			Assert.AreEqual("John", picker.Items[0]);
			items.RemoveAt(1);
			Assert.AreEqual(2, picker.Items.Count);
			Assert.AreEqual("Ringo", picker.Items[1]);
		}

		[Test]
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
			Assert.AreEqual(3, picker.Items.Count);
			Assert.AreEqual("John", picker.Items[0]);
		}

		[Test]
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
			Assert.AreEqual(1, picker.Items.Count);
			Assert.AreEqual(-1, picker.SelectedIndex);
			Assert.AreEqual(bindingContext.SelectedItem, picker.SelectedItem);
		}

		[Test]
		public void ThrowsWhenModifyingItemsIfItemsSourceIsSet()
		{
			var picker = new Picker
			{
				ItemsSource = new System.Collections.Generic.List<object>()
			};
			Assert.Throws<InvalidOperationException>(() => picker.Items.Add("foo"));
		}

		[Test]
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
			Assert.AreEqual("NestedProperty", picker.Items[0]);
		}

		[Test]
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
			Assert.AreEqual("Start", picker.Items[0]);
		}

		[Test]
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
			Assert.AreEqual(1, picker.Items.Count);
			Assert.AreEqual(0, picker.SelectedIndex);
			Assert.AreEqual(obj, picker.SelectedItem);
		}

		[Test]
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
			Assert.AreEqual(1, picker.Items.Count);
			Assert.AreEqual(-1, picker.SelectedIndex);
			Assert.AreEqual(null, picker.SelectedItem);
			picker.SelectedItem = obj;
			Assert.AreEqual(0, picker.SelectedIndex);
			Assert.AreEqual(obj, picker.SelectedItem);
			picker.SelectedIndex = -1;
			Assert.AreEqual(-1, picker.SelectedIndex);
			Assert.AreEqual(null, picker.SelectedItem);
		}
	}
}