#nullable enable
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// ViewModel classes for TypedBinding tests
public class UserInfo : INotifyPropertyChanged
{
	private string _displayName = "John Doe";
	public string DisplayName
	{
		get => _displayName;
		set { _displayName = value; OnPropertyChanged(); }
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class SimpleViewModel : INotifyPropertyChanged
{
	private string _name = "From ViewModel";
	public string Name
	{
		get => _name;
		set { _name = value; OnPropertyChanged(); }
	}

	private UserInfo _user = new UserInfo();
	public UserInfo User
	{
		get => _user;
		set { _user = value; OnPropertyChanged(); }
	}

	private decimal _price = 100m;
	public decimal Price
	{
		get => _price;
		set { _price = value; OnPropertyChanged(); }
	}

	private int _quantity = 2;
	public int Quantity
	{
		get => _quantity;
		set { _quantity = value; OnPropertyChanged(); }
	}

	public string GetDisplayName() => $"Display: {Name}";

	public event PropertyChangedEventHandler? PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string? name = null)
		=> PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public partial class CSharpExpressions : ContentPage
{
	// Properties for expression testing
	public string PageTitle => "Test Page";
	public string ExplicitProperty => "Explicit Value";
	public string FirstName => "John";
	public string LastName => "Doe";
	public bool IsHidden => false;
	public bool IsActive => true;
	public string? NullableValue => null;
	public int Count => 0;
	public string? value => null;
	public string BareProperty => "Bare Value";
	public new bool IsVisible => true;
	public bool IsVisibleProp => true;
	public bool IsDataLoaded => true;
	public bool HasProducts => true;
	public bool IsEmpty => false;
	public bool HasError => false;

	// Properties for TypedBinding tests
	public string LocalProperty => "From Local";
	public decimal TaxRate => 0.1m;  // 10% tax rate

	// Nested this. test - method that takes a local value
	public decimal CalculateWithRate(decimal rate) => 100m * rate;

	// Bindable properties for Binding tests
	public string BindableText { get; set; } = "Bindable Value";
	public string MixedText { get; set; } = "Mixed Value";

	// Lambda event properties
	public int ClickCount { get; set; }
	public bool ButtonClicked { get; private set; }
	public object? LastSender { get; private set; }
	public string? LastNewText { get; private set; }

	// Local method call tracking
	public int GetTextCallCount { get; private set; }
	public string GetText()
	{
		GetTextCallCount++;
		return "Method Result";
	}
	public char GetChar(char c) => c;

	// Capture test method - only used by captureTestLabel
	public int GetMultiplierCallCount { get; private set; }
	public decimal GetMultiplier()
	{
		GetMultiplierCallCount++;
		return 1.5m;
	}

	// Multi-arg capture test - same method called with different args
	public string GetFormattedValue(int value) => $"Value{value}";

	// Lambda event handlers
	public void OnButtonClicked() => ButtonClicked = true;
	public void OnButtonClickedWithSender(object? sender) => LastSender = sender;
	public void OnTextChanged(string? newValue) => LastNewText = newValue;

	public CSharpExpressions() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests
	{
		[Fact]
		public void ExplicitPropertyExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Explicit Value", page.explicitPropertyLabel.Text);
		}

		[Fact]
		public void MethodCallExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Method Result", page.methodCallLabel.Text);
		}

		[Fact]
		public void LocalMethodCalledOnceAtInit()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			try
			{
				var page = new CSharpExpressions(XamlInflator.SourceGen);
				var vm = new SimpleViewModel { Price = 100m };
				page.BindingContext = vm;
				
				// Method should have been called exactly once during initialization
				Assert.Equal(1, page.GetMultiplierCallCount);
				
				// Verify initial value contains "100" and "1" and "5"
				Assert.StartsWith("100 x 1", page.captureTestLabel.Text, StringComparison.Ordinal);
				
				// Change the binding source - binding updates but local method NOT called again
				vm.Price = 200m;
				Assert.StartsWith("200 x 1", page.captureTestLabel.Text, StringComparison.Ordinal);
				Assert.Equal(1, page.GetMultiplierCallCount); // Still 1!
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public void SameMethodDifferentArgs_EachCapturedSeparately()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			try
			{
				var page = new CSharpExpressions(XamlInflator.SourceGen);
				var vm = new SimpleViewModel { Price = 99m };
				page.BindingContext = vm;
				
				// GetFormattedValue(1) and GetFormattedValue(2) should both be captured
				Assert.Equal("99: Value1 and Value2", page.multiMethodArgsLabel.Text);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public void StringConcatenationExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("John Doe", page.additionLabel.Text);
		}

		[Fact]
		public void NegationExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.True(page.negationLabel.IsVisible);
		}

		[Fact]
		public void TernaryExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Active", page.ternaryLabel.Text);
		}

		[Fact]
		public void NullCoalescingExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Default", page.nullCoalescingLabel.Text);
		}

		[Fact]
		public void ComparisonExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.True(page.comparisonLabel.IsVisible);
		}

		[Fact]
		public void BooleanAndExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			// IsDataLoaded (true) && HasProducts (true) = true
			Assert.True(page.booleanAndLabel.IsVisible);
		}

		[Fact]
		public void BooleanOrExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			// IsEmpty (false) || HasError (false) = false
			Assert.False(page.booleanOrLabel.IsVisible);
		}

		[Fact]
		public void CharLiteralExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("x", page.charLabel.Text);
		}

		[Fact]
		public void EscapedSingleQuotesExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("it's working", page.escapedSingleQuotesLabel.Text);
		}

		[Fact]
		public void StaticMethodCallExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Count: 0", page.staticMethodLabel.Text);
		}

		[Fact]
		public void TitlePropertyExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Test Page", page.Title);
		}

		[Fact]
		public void BareIdentifierExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Bare Value", page.bareIdentifierLabel.Text);
		}

		// DISAMBIGUATION TESTS

		[Fact]
		public void BindingMarkupExtension_NotTreatedAsExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			// Binding should work - the text shouldn't be the literal markup string
			// If it was treated as expression, it would fail to compile or have wrong value
			Assert.NotEqual("{Binding BindableText}", page.bindingLabel.Text);
		}

		[Fact]
		public void StaticResourceMarkupExtension_NotTreatedAsExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Static Resource Value", page.staticResourceLabel.Text);
		}

		[Fact]
		public void OnPlatformMarkupExtension_NotTreatedAsExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			// OnPlatform should resolve to a value, not be treated as expression
			Assert.NotNull(page.onPlatformLabel.Text);
			Assert.NotEqual("{OnPlatform Default=PlatformDefault, iOS=iOS Value}", page.onPlatformLabel.Text);
		}

		[Fact]
		public void OnIdiomMarkupExtension_NotTreatedAsExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			// OnIdiom should resolve to a value
			Assert.NotNull(page.onIdiomLabel.Text);
			Assert.NotEqual("{OnIdiom Default=IdiomDefault, Phone=Phone Value}", page.onIdiomLabel.Text);
		}

		[Fact]
		public void XStaticMarkupExtension_NotTreatedAsExpression()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			// x:Static should resolve to String.Empty
			Assert.Equal(string.Empty, page.xStaticLabel.Text);
		}

		[Fact]
		public void MixedBindingAndExpression_BothWork()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			// Expression should work for IsVisible
			Assert.True(page.mixedMarkupLabel.IsVisible);
			// Binding should set text - not the literal markup
			Assert.NotEqual("{Binding MixedText}", page.mixedMarkupLabel.Text);
		}

		// LAMBDA EVENT TESTS

		[Fact]
		public void InlineLambdaEvent()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal(0, page.ClickCount);
			page.inlineLambdaButton.SendClicked();
			Assert.Equal(1, page.ClickCount);
		}

		[Fact]
		public void MethodLambdaEvent()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.False(page.ButtonClicked);
			page.methodLambdaButton.SendClicked();
			Assert.True(page.ButtonClicked);
		}

		[Fact]
		public void SenderLambdaEvent()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Null(page.LastSender);
			page.senderLambdaButton.SendClicked();
			Assert.Same(page.senderLambdaButton, page.LastSender);
		}

		[Fact]
		public void ExplicitLambdaEvent()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal(0, page.ClickCount);
			page.explicitLambdaButton.SendClicked();
			Assert.Equal(1, page.ClickCount);
		}

		[Fact]
		public void TextChangedLambdaEvent()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Null(page.LastNewText);
			page.lambdaEntry.Text = "New Value";
			Assert.Equal("New Value", page.LastNewText);
		}

		// STRING INTERPOLATION TESTS

		[Fact]
		public void BasicStringInterpolation()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Hello John", page.interpolationLabel.Text);
		}

		[Fact]
		public void StringInterpolationWithMultipleExpressions()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("John Doe", page.interpolationExprLabel.Text);
		}

		[Fact]
		public void StringInterpolationWithMethodCall()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Result: Method Result", page.interpolationMethodLabel.Text);
		}

		// TYPED BINDING TESTS

		[Fact]
		public void TypedBinding_SimpleProperty()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			page.BindingContext = new SimpleViewModel();
			Assert.Equal("From ViewModel", page.vmLabel.Text);
		}

		[Fact]
		public void TypedBinding_DotPrefix()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			page.BindingContext = new SimpleViewModel();
			Assert.Equal("From ViewModel", page.dotPrefixLabel.Text);
		}

		[Fact]
		public void TypedBinding_BindingContextPrefix()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			page.BindingContext = new SimpleViewModel();
			Assert.Equal("From ViewModel", page.bcPrefixLabel.Text);
		}

		[Fact]
		public void TypedBinding_ThisPrefix()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			// this.LocalProperty should NOT be a binding, just a SetValue at init time
			Assert.Equal("From Local", page.thisPrefixLabel.Text);
		}

		[Fact]
		public void TypedBinding_NestedProperty()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			page.BindingContext = new SimpleViewModel();
			Assert.Equal("John Doe", page.nestedLabel.Text);
		}

		[Fact]
		public void TypedBinding_INPC_Updates()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			try
			{
				var page = new CSharpExpressions(XamlInflator.SourceGen);
				var vm = new SimpleViewModel();
				page.BindingContext = vm;
				
				Assert.Equal("From ViewModel", page.vmLabel.Text);
				
				// Change the property - binding should update
				vm.Name = "Updated Name";
				Assert.Equal("Updated Name", page.vmLabel.Text);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public void TypedBinding_NestedProperty_INPC()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			try
			{
				var page = new CSharpExpressions(XamlInflator.SourceGen);
				var vm = new SimpleViewModel();
				page.BindingContext = vm;
				
				Assert.Equal("John Doe", page.nestedLabel.Text);
				
				// Change the nested property - binding should update
				vm.User.DisplayName = "Jane Smith";
				Assert.Equal("Jane Smith", page.nestedLabel.Text);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public void TypedBinding_NestedProperty_ParentChange()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			try
			{
				var page = new CSharpExpressions(XamlInflator.SourceGen);
				var vm = new SimpleViewModel();
				page.BindingContext = vm;
				
				Assert.Equal("John Doe", page.nestedLabel.Text);
				
				// Change the parent property - binding should update
				vm.User = new UserInfo { DisplayName = "New User" };
				Assert.Equal("New User", page.nestedLabel.Text);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public void TypedBinding_MethodCall()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			var vm = new SimpleViewModel();
			page.BindingContext = vm;
			
			Assert.Equal("Display: From ViewModel", page.vmMethodLabel.Text);
		}

		[Fact]
		public void TypedBinding_BareMethodCall()
		{
			// Test that {GetDisplayName()} without prefix resolves to DataType method
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			var vm = new SimpleViewModel();
			page.BindingContext = vm;
			
			Assert.Equal("Display: From ViewModel", page.vmMethodBareLabel.Text);
		}

		[Fact]
		public void TypedBinding_MixedLocalAndBinding()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			var vm = new SimpleViewModel { Price = 100m };
			page.BindingContext = vm;
			
			// Price (100) * TaxRate (0.1) = 10.00 (culture-dependent decimal separator)
			Assert.StartsWith("10", page.mixedExprLabel.Text, StringComparison.Ordinal);
			Assert.Equal(5, page.mixedExprLabel.Text!.Length); // "10.00" or "10,00"
		}

		[Fact]
		public void TypedBinding_DuplicateCapture()
		{
			// Tests that two bindings using same local member (this.TaxRate) don't cause CS0128
			// The scoped block wrapping ensures each capture is in its own scope
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			var vm = new SimpleViewModel { Price = 100m, Quantity = 5 };
			page.BindingContext = vm;
			
			// First binding with explicit this.TaxRate: Quantity (5) * TaxRate (0.1) = 0.50
			Assert.StartsWith("0", page.mixedExprLabel2.Text, StringComparison.Ordinal);
			
			// Second binding with explicit this.TaxRate: Price (100) * TaxRate (0.1) = 10.00
			Assert.StartsWith("10", page.mixedExprLabel3.Text, StringComparison.Ordinal);
		}

		[Fact]
		public void TypedBinding_MixedLocalAndBinding_INPC()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			try
			{
				var page = new CSharpExpressions(XamlInflator.SourceGen);
				var vm = new SimpleViewModel { Price = 100m };
				page.BindingContext = vm;
				
				Assert.StartsWith("10", page.mixedExprLabel.Text, StringComparison.Ordinal);
				
				// Change Price - binding should update (TaxRate was captured at init)
				vm.Price = 200m;
				Assert.StartsWith("20", page.mixedExprLabel.Text, StringComparison.Ordinal);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public void TypedBinding_MultiRoot()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			var vm = new SimpleViewModel { Price = 50m, Quantity = 3 };
			page.BindingContext = vm;
			
			// Price (50) * Quantity (3) = 150
			Assert.Equal("150", page.multiRootLabel.Text);
		}

		[Fact]
		public void TypedBinding_MultiRoot_INPC_FirstProperty()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			try
			{
				var page = new CSharpExpressions(XamlInflator.SourceGen);
				var vm = new SimpleViewModel { Price = 50m, Quantity = 3 };
				page.BindingContext = vm;
				
				Assert.Equal("150", page.multiRootLabel.Text);
				
				// Change Price - binding should update
				vm.Price = 100m;
				Assert.Equal("300", page.multiRootLabel.Text);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public void TypedBinding_MultiRoot_INPC_SecondProperty()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			try
			{
				var page = new CSharpExpressions(XamlInflator.SourceGen);
				var vm = new SimpleViewModel { Price = 50m, Quantity = 3 };
				page.BindingContext = vm;
				
				Assert.Equal("150", page.multiRootLabel.Text);
				
				// Change Quantity - binding should update
				vm.Quantity = 4;
				Assert.Equal("200", page.multiRootLabel.Text);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public void TypedBinding_StringInterpolation()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			var vm = new SimpleViewModel { Name = "World" };
			page.BindingContext = vm;
			
			Assert.Equal("Hello World!", page.vmInterpolationLabel.Text);
		}

		[Fact]
		public void TypedBinding_StringInterpolation_INPC()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			try
			{
				var page = new CSharpExpressions(XamlInflator.SourceGen);
				var vm = new SimpleViewModel { Name = "World" };
				page.BindingContext = vm;
				
				Assert.Equal("Hello World!", page.vmInterpolationLabel.Text);
				
				// Change Name - binding should update
				vm.Name = "MAUI";
				Assert.Equal("Hello MAUI!", page.vmInterpolationLabel.Text);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		[Fact]
		public void TypedBinding_StringInterpolation_Multiple()
		{
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			var vm = new SimpleViewModel { Price = 9.99m, Quantity = 5 };
			page.BindingContext = vm;
			
			// Format is culture-dependent, just check structure
			Assert.StartsWith("5x at", page.vmInterpolationMultiLabel.Text, StringComparison.Ordinal);
		}

		[Fact]
		public void TypedBinding_StringInterpolation_Multiple_INPC()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			try
			{
				var page = new CSharpExpressions(XamlInflator.SourceGen);
				var vm = new SimpleViewModel { Price = 9.99m, Quantity = 5 };
				page.BindingContext = vm;
				
				Assert.StartsWith("5x at", page.vmInterpolationMultiLabel.Text, StringComparison.Ordinal);
				
				// Change Quantity - should update
				vm.Quantity = 10;
				Assert.StartsWith("10x at", page.vmInterpolationMultiLabel.Text, StringComparison.Ordinal);
			}
			finally
			{
				DispatcherProvider.SetCurrent(null);
			}
		}

		// CDATA EXPRESSION TESTS

		[Fact]
		public void CdataBooleanAndExpression()
		{
			// Tests CDATA with natural && syntax (no XML escaping required)
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.True(page.cdataBooleanAndLabel.IsVisible); // IsDataLoaded && HasProducts = true && true
		}

		[Fact]
		public void CdataComparisonExpression()
		{
			// Tests CDATA with natural < > syntax (no XML escaping required)
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.False(page.cdataComparisonLabel.IsVisible); // Count (0) > 0 && Count < 100 = false
		}

		[Fact]
		public void CdataDoubleQuotesExpression()
		{
			// Tests CDATA with natural double quotes (no single-quote transformation)
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Default Text", page.cdataDoubleQuotesLabel.Text); // value ?? "Default Text"
		}

		[Fact]
		public void CdataStringInterpolationExpression()
		{
			// Tests CDATA with string interpolation using double quotes
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("Hello John!", page.cdataInterpolationLabel.Text); // $"Hello {FirstName}!"
		}

		[Fact]
		public void NestedThisExpressions()
		{
			// Tests that nested this. expressions work: this.Method(this.Property)
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			// CalculateWithRate(TaxRate) = 100 * 0.1 = 10
			Assert.StartsWith("10", page.nestedThisLabel.Text, StringComparison.Ordinal);
		}

		[Fact]
		public void EscapedDoubleQuotesInSingleQuotedString()
		{
			// Tests that \" inside single quotes becomes " in the output (not \\")
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("he said \"hi\"", page.escapedDoubleQuotesLabel.Text);
		}

		[Fact]
		public void EscapedCharLiterals_Newline()
		{
			// Tests that '\n' stays as char literal, not converted to string
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("\n", page.escapedNewlineLabel.Text);
		}

		[Fact]
		public void EscapedCharLiterals_Tab()
		{
			// Tests that '\t' stays as char literal, not converted to string
			var page = new CSharpExpressions(XamlInflator.SourceGen);
			Assert.Equal("\t", page.escapedTabLabel.Text);
		}
	}
}
