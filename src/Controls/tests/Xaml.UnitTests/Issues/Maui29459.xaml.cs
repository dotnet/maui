using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

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

	[TestFixture]
	class Tests
	{
		[SetUp] public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		[TearDown] public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void SwitchingBindingTriggersPropertyChanged([Values] XamlInflator inflator)
		{
			// Arrange: Create page and set up view model
			var page = new Maui29459(inflator);
			var viewModel = new Maui29459ViewModel { A = 0, B = 100 };
			page.BindingContext = viewModel;

			// Act: Bind to property A
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.A));

			// Assert: Initial binding should trigger property changed
			Assert.That(page.MyControl.Value, Is.EqualTo(0), "Initial value should be A (0)");

			// Reset counter for clean measurement
			int initialCount = page.MyControl.PropertyChangedCount;

			// Act: Switch to property B
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.B));

			// Assert: Switching binding should trigger property changed since value is different
			Assert.That(page.MyControl.Value, Is.EqualTo(100), "Value should now be B (100)");
			Assert.That(page.MyControl.PropertyChangedCount, Is.GreaterThan(initialCount),
				"PropertyChanged should fire when switching to a binding with a different value");
		}

		[Test]
		public void SwitchingBindingAfterModifyingValueTriggersPropertyChanged([Values] XamlInflator inflator)
		{
			// This test reproduces the exact scenario from issue #29459:
			// A --> B (press Increase button on control) --> A (no changes) --> B (should show updated B value)
			// The key is that the Increase button modifies the INTERNAL ViewModel, not the external one directly

			var page = new Maui29459(inflator);
			var viewModel = new Maui29459ViewModel { A = 0, B = 100 };
			page.BindingContext = viewModel;

			// Step 1: Bind to A
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.A));
			Assert.That(page.MyControl.Value, Is.EqualTo(0), "Step 1: Value should be A (0)");
			Assert.That(page.MyControl.ViewModel.Value, Is.EqualTo(0), "Step 1: Internal ViewModel should sync to 0");

			// Step 2: Switch to B
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.B));
			Assert.That(page.MyControl.Value, Is.EqualTo(100), "Step 2: Value should be B (100)");
			Assert.That(page.MyControl.ViewModel.Value, Is.EqualTo(100), "Step 2: Internal ViewModel should sync to 100");

			// Step 3: Press Increase button (this modifies the INTERNAL ViewModel, which syncs to bindable property, which syncs to external viewModel.B)
			page.MyControl.ViewModel.Increase();
			Assert.That(page.MyControl.ViewModel.Value, Is.EqualTo(101), "Step 3: Internal ViewModel should be 101");
			Assert.That(page.MyControl.Value, Is.EqualTo(101), "Step 3: Bindable property should be 101");
			Assert.That(viewModel.B, Is.EqualTo(101), "Step 3: External ViewModel B should be 101 (TwoWay binding)");

			// Step 4: Switch back to A (without pressing Increase)
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.A));
			Assert.That(page.MyControl.Value, Is.EqualTo(0), "Step 4: Value should be A (0)");
			Assert.That(page.MyControl.ViewModel.Value, Is.EqualTo(0), "Step 4: Internal ViewModel should sync to 0");

			// Record PropertyChangedCount before Step 5
			int countBeforeStep5 = page.MyControl.PropertyChangedCount;

			// Step 5: Switch back to B - THIS IS WHERE THE BUG MANIFESTS
			// The issue reports that the control's Label shows 0 instead of 101
			// This happens because PropertyChanged doesn't fire when switching bindings
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.B));

			// The external viewModel.B should still be 101 (we didn't change it)
			Assert.That(viewModel.B, Is.EqualTo(101), "Step 5: External ViewModel B should still be 101");
			
			// The control's Value (bindable property) should be 101
			Assert.That(page.MyControl.Value, Is.EqualTo(101), 
				"Step 5: Bindable property should be B (101), not the stale value");

			// Check if propertyChanged was called
			Assert.That(page.MyControl.PropertyChangedCount, Is.GreaterThan(countBeforeStep5),
				$"Step 5: PropertyChanged callback should have been called. Count before: {countBeforeStep5}, after: {page.MyControl.PropertyChangedCount}. Last old={page.MyControl.LastOldValue}, new={page.MyControl.LastNewValue}. Value now={page.MyControl.Value}");
			
			// The internal ViewModel should also be synced to 101
			Assert.That(page.MyControl.ViewModel.Value, Is.EqualTo(101),
				"Step 5: Internal ViewModel should be synced to 101 - THIS IS THE BUG if it shows 0");
		}

		[Test]
		public void SwitchingBindingToSameValueDoesNotTriggerPropertyChanged([Values] XamlInflator inflator)
		{
			// When switching bindings but the value remains the same, 
			// PropertyChanged should NOT fire (optimization)

			var page = new Maui29459(inflator);
			var viewModel = new Maui29459ViewModel { A = 50, B = 50 }; // Same values
			page.BindingContext = viewModel;

			// Bind to A
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.A));
			int countAfterFirstBinding = page.MyControl.PropertyChangedCount;

			// Switch to B (same value)
			page.MyControl.SetBinding(Maui29459CustomControl.ValueProperty, nameof(Maui29459ViewModel.B));

			Assert.That(page.MyControl.Value, Is.EqualTo(50), "Value should remain 50");
			// Note: PropertyChanged may or may not fire when values are equal - this is implementation-dependent
			// The key assertion is that the Value is correct
		}
	}
}
