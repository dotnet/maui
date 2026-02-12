using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Internal ViewModel for the custom control (matches the bug report pattern)
public class Maui29459ControlViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	private int _value;
	public int Value
	{
		get => _value;
		set
		{
			if (_value != value)
			{
				_value = value;
				OnPropertyChanged();
			}
		}
	}

	public void Increase() => Value++;

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

// Custom control with internal ViewModel that syncs with bindable property
// This matches the exact pattern from the bug report
public class Maui29459CustomControl : ContentView
{
	public Maui29459ControlViewModel ViewModel { get; } = new();

	public Maui29459CustomControl()
	{
		// Sync ViewModel.Value -> BindableProperty Value (like the bug report)
		ViewModel.PropertyChanged += (s, e) =>
		{
			if (e.PropertyName == nameof(Maui29459ControlViewModel.Value))
			{
				if (Value != ViewModel.Value)
				{
					Value = ViewModel.Value;
				}
			}
		};
	}

	public static readonly BindableProperty ValueProperty = BindableProperty.Create(
		propertyName: nameof(Value),
		returnType: typeof(int),
		declaringType: typeof(Maui29459CustomControl),
		defaultValue: 0,
		defaultBindingMode: BindingMode.TwoWay,
		propertyChanged: OnValuePropertyChanged);

	private static void OnValuePropertyChanged(BindableObject bindable, object oldValue, object newValue)
	{
		// Sync BindableProperty Value -> ViewModel.Value (like the bug report)
		if (bindable is Maui29459CustomControl control && newValue is int newVal)
		{
			control.PropertyChangedCount++;
			control.LastOldValue = oldValue;
			control.LastNewValue = newValue;
			if (control.ViewModel.Value != newVal)
			{
				control.ViewModel.Value = newVal;
			}
		}
	}

	public int Value
	{
		get => (int)GetValue(ValueProperty);
		set => SetValue(ValueProperty, value);
	}

	public int PropertyChangedCount { get; private set; }
	public object LastOldValue { get; private set; }
	public object LastNewValue { get; private set; }
}

// ViewModel for MainPage testing
public class Maui29459ViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler PropertyChanged;

	private int _a;
	public int A
	{
		get => _a;
		set
		{
			if (_a != value)
			{
				_a = value;
				OnPropertyChanged();
			}
		}
	}

	private int _b;
	public int B
	{
		get => _b;
		set
		{
			if (_b != value)
			{
				_b = value;
				OnPropertyChanged();
			}
		}
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public partial class Maui29459 : ContentPage
{
	public Maui29459() => InitializeComponent();

	[Collection("Binding")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		public void SwitchingBindingTriggersPropertyChanged(XamlInflator inflator)
		{
			// Arrange: Create page and set up view model
			var page = new Maui29459(inflator);
			var viewModel = new Maui29459ViewModel { A = 0, B = 100 };
			page.BindingContext = viewModel;

			// Act: Bind to property A
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.A));

			// Assert: Initial binding should trigger property changed
			Assert.Equal(0, page.MyControl.Value);

			// Reset counter for clean measurement
			int initialCount = page.MyControl.PropertyChangedCount;

			// Act: Switch to property B
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.B));

			// Assert: Switching binding should trigger property changed since value is different
			Assert.Equal(100, page.MyControl.Value);
			Assert.True(page.MyControl.PropertyChangedCount > initialCount,
				"PropertyChanged should fire when switching to a binding with a different value");
		}

		[Theory]
		[XamlInflatorData]
		public void SwitchingBindingAfterModifyingValueTriggersPropertyChanged(XamlInflator inflator)
		{
			// This test reproduces the exact scenario from issue #29459:
			// A --> B (press Increase button on control) --> A (no changes) --> B (should show updated B value)
			// The key is that the Increase button modifies the INTERNAL ViewModel, not the external one directly

			var page = new Maui29459(inflator);
			var viewModel = new Maui29459ViewModel { A = 0, B = 100 };
			page.BindingContext = viewModel;

			// Step 1: Bind to A
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.A));
			Assert.Equal(0, page.MyControl.Value);
			Assert.Equal(0, page.MyControl.ViewModel.Value);

			// Step 2: Switch to B
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.B));
			Assert.Equal(100, page.MyControl.Value);
			Assert.Equal(100, page.MyControl.ViewModel.Value);

			// Step 3: Press Increase button (this modifies the INTERNAL ViewModel, which syncs to bindable property, which syncs to external viewModel.B)
			page.MyControl.ViewModel.Increase();
			Assert.Equal(101, page.MyControl.ViewModel.Value);
			Assert.Equal(101, page.MyControl.Value);
			Assert.Equal(101, viewModel.B);

			// Step 4: Switch back to A (without pressing Increase)
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.A));
			Assert.Equal(0, page.MyControl.Value);
			Assert.Equal(0, page.MyControl.ViewModel.Value);

			// Record PropertyChangedCount before Step 5
			int countBeforeStep5 = page.MyControl.PropertyChangedCount;

			// Step 5: Switch back to B - THIS IS WHERE THE BUG MANIFESTS
			// The issue reports that the control's Label shows 0 instead of 101
			// This happens because PropertyChanged doesn't fire when switching bindings
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.B));

			// The external viewModel.B should still be 101 (we didn't change it)
			Assert.Equal(101, viewModel.B);
			
			// The control's Value (bindable property) should be 101
			Assert.Equal(101, page.MyControl.Value);

			// Check if propertyChanged was called
			Assert.True(page.MyControl.PropertyChangedCount > countBeforeStep5,
				$"PropertyChanged callback should have been called. Count before: {countBeforeStep5}, after: {page.MyControl.PropertyChangedCount}");
			
			// The internal ViewModel should also be synced to 101
			Assert.Equal(101, page.MyControl.ViewModel.Value);
		}

		[Theory]
		[XamlInflatorData]
		public void SwitchingBindingToSameValueMaintainsCorrectValue(XamlInflator inflator)
		{
			// The value should be correct regardless of PropertyChanged firing behavior

			var page = new Maui29459(inflator);
			var viewModel = new Maui29459ViewModel { A = 50, B = 50 }; // Same values
			page.BindingContext = viewModel;

			// Bind to A
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.A));

			// Switch to B (same value)
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.B));

			Assert.Equal(50, page.MyControl.Value);
			// PropertyChangedCount behavior is implementation-defined when values are equal; no assertion is made.
		}
	}
}
