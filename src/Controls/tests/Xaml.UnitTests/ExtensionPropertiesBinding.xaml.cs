// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Domain models
public class OrderItem
{
	public string Name { get; set; } = string.Empty;
	public decimal Price { get; set; }
	public int Quantity { get; set; }
}

public class Order
{
	public List<OrderItem> Items { get; set; } = new();
	public decimal DiscountPercent { get; set; }
}

// C# 14 extension members for OrderItem - adds computed properties
public static class OrderItemExtensions
{
	extension(OrderItem item)
	{
		public decimal LineTotal => item.Price * item.Quantity;
		public string FormattedLineTotal => item.LineTotal.ToString("F2", CultureInfo.InvariantCulture);
	}
}

// C# 14 extension members for Order - adds computed properties that use other extension properties
public static class OrderExtensions
{
	extension(Order order)
	{
		public decimal SubTotal => order.Items.Sum(i => i.LineTotal);
		public decimal DiscountAmount => order.SubTotal * (order.DiscountPercent / 100m);
		public decimal Total => order.SubTotal - order.DiscountAmount;
		public int TotalItemCount => order.Items.Sum(i => i.Quantity);

		public string FormattedTotal => order.Total.ToString("F2", CultureInfo.InvariantCulture);
		public string FormattedSubTotal => order.SubTotal.ToString("F2", CultureInfo.InvariantCulture);
		public string FormattedDiscount => $"{order.DiscountAmount.ToString("F2", CultureInfo.InvariantCulture)} ({order.DiscountPercent}% off)";

		public string Summary => $"Order: {order.TotalItemCount} items, Total: {order.FormattedTotal}";
	}
}

// C# 14 extension members for IEnumerable<OrderItem>
public static class OrderItemCollectionExtensions
{
	extension(IEnumerable<OrderItem> items)
	{
		public decimal TotalValue => items.Sum(i => i.LineTotal);
		public bool HasExpensiveItems => items.Any(i => i.Price > 100);
	}
}

// ViewModel that exposes C# 14 extension properties through regular properties for XAML binding
public class OrderViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private Order _order = new();
	public Order Order
	{
		get => _order;
		set
		{
			_order = value;
			OnPropertyChanged();
			NotifyOrderPropertiesChanged();
		}
	}

	// Expose C# 14 extension properties through regular properties for XAML binding
	public string FormattedTotal => _order.FormattedTotal;
	public string ItemCountDisplay => $"{_order.TotalItemCount} items";
	public string DiscountedTotalDisplay => _order.DiscountPercent > 0
		? $"{_order.FormattedSubTotal} → {_order.FormattedTotal} ({_order.FormattedDiscount})"
		: _order.FormattedTotal;
	public string OrderSummary => _order.Summary;

	// Method to update items and notify property changes
	public void AddItem(OrderItem item)
	{
		_order.Items.Add(item);
		NotifyOrderPropertiesChanged();
	}

	public void SetDiscount(decimal percent)
	{
		_order.DiscountPercent = percent;
		NotifyOrderPropertiesChanged();
	}

	private void NotifyOrderPropertiesChanged()
	{
		OnPropertyChanged(nameof(FormattedTotal));
		OnPropertyChanged(nameof(ItemCountDisplay));
		OnPropertyChanged(nameof(DiscountedTotalDisplay));
		OnPropertyChanged(nameof(OrderSummary));
	}

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public partial class ExtensionPropertiesBinding : ContentPage
{
	public ExtensionPropertiesBinding() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyBindingWithComputedValues(XamlInflator inflator)
		{
			var vm = new OrderViewModel
			{
				Order = new Order
				{
					Items = new List<OrderItem>
					{
						new() { Name = "Widget", Price = 10.00m, Quantity = 2 },
						new() { Name = "Gadget", Price = 25.00m, Quantity = 1 }
					},
					DiscountPercent = 0
				}
			};

			var page = new ExtensionPropertiesBinding(inflator)
			{
				BindingContext = vm
			};

			// Total: (10 * 2) + (25 * 1) = 45.00
			Assert.Equal("45.00", page.totalLabel.Text);
			Assert.Equal("3 items", page.itemCountLabel.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyBindingWithDiscount(XamlInflator inflator)
		{
			var vm = new OrderViewModel
			{
				Order = new Order
				{
					Items = new List<OrderItem>
					{
						new() { Name = "Product", Price = 100.00m, Quantity = 1 }
					},
					DiscountPercent = 10
				}
			};

			var page = new ExtensionPropertiesBinding(inflator)
			{
				BindingContext = vm
			};

			// With 10% discount: $100 - $10 = $90
			Assert.Equal("90.00", page.totalLabel.Text);
			Assert.Contains("100.00", page.discountLabel.Text, StringComparison.Ordinal);
			Assert.Contains("90.00", page.discountLabel.Text, StringComparison.Ordinal);
			Assert.Contains("10%", page.discountLabel.Text, StringComparison.Ordinal);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyBindingUpdatesOnChange(XamlInflator inflator)
		{
			var vm = new OrderViewModel
			{
				Order = new Order
				{
					Items = new List<OrderItem>
					{
						new() { Name = "Item1", Price = 50.00m, Quantity = 1 }
					}
				}
			};

			var page = new ExtensionPropertiesBinding(inflator)
			{
				BindingContext = vm
			};

			Assert.Equal("50.00", page.totalLabel.Text);
			Assert.Equal("1 items", page.itemCountLabel.Text);

			// Add another item and verify binding updates
			vm.AddItem(new OrderItem { Name = "Item2", Price = 30.00m, Quantity = 2 });

			Assert.Equal("110.00", page.totalLabel.Text);
			Assert.Equal("3 items", page.itemCountLabel.Text);
		}

		[Fact]
		internal void ExtensionPropertyOnEnumerableWorks()
		{
			// Test C# 14 extension properties on IEnumerable<T>
			var items = new List<OrderItem>
			{
				new() { Name = "Cheap", Price = 10.00m, Quantity = 5 },
				new() { Name = "Expensive", Price = 150.00m, Quantity = 1 }
			};

			// Extension property: TotalValue
			Assert.Equal(200.00m, items.TotalValue); // (10*5) + (150*1)

			// Extension property: HasExpensiveItems
			Assert.True(items.HasExpensiveItems);

			var cheapItems = new List<OrderItem>
			{
				new() { Name = "Budget1", Price = 5.00m, Quantity = 1 },
				new() { Name = "Budget2", Price = 10.00m, Quantity = 1 }
			};
			Assert.False(cheapItems.HasExpensiveItems);
		}

		[Fact]
		internal void ExtensionPropertyChainedUsage()
		{
			// Test using extension properties that depend on other extension properties
			var item = new OrderItem { Name = "Test", Price = 19.99m, Quantity = 3 };

			// LineTotal is an extension property
			Assert.Equal(59.97m, item.LineTotal);
			Assert.Equal("59.97", item.FormattedLineTotal);

			var order = new Order
			{
				Items = new List<OrderItem> { item },
				DiscountPercent = 20
			};

			// These extension properties use other extension properties internally
			Assert.Equal(59.97m, order.SubTotal);
			Assert.Equal(11.994m, order.DiscountAmount);
			Assert.Equal(47.976m, order.Total);
			Assert.Contains("3 items", order.Summary, StringComparison.Ordinal);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertySummaryBinding(XamlInflator inflator)
		{
			var vm = new OrderViewModel
			{
				Order = new Order
				{
					Items = new List<OrderItem>
					{
						new() { Name = "A", Price = 10.00m, Quantity = 1 },
						new() { Name = "B", Price = 20.00m, Quantity = 2 }
					}
				}
			};

			var page = new ExtensionPropertiesBinding(inflator)
			{
				BindingContext = vm
			};

			// Summary uses the extension property Order.Summary
			Assert.Contains("3 items", page.summaryLabel.Text, StringComparison.Ordinal);
			Assert.Contains("50.00", page.summaryLabel.Text, StringComparison.Ordinal);
		}
	}
}
