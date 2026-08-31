// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// C# 14 extension members for Label - adds properties directly to Label
// These should be usable in XAML like regular properties: <Label MyTag="value" />
public static class LabelExtensions
{
	private static readonly Dictionary<Label, string> _myTags = new();
	private static readonly Dictionary<Label, int> _myPriorities = new();

	// C# 14 extension properties - adds properties directly to Label type
	extension(Label label)
	{
		public string MyTag
		{
			get => _myTags.TryGetValue(label, out var tag) ? tag : string.Empty;
			set => _myTags[label] = value;
		}

		public int MyPriority
		{
			get => _myPriorities.TryGetValue(label, out var priority) ? priority : 0;
			set => _myPriorities[label] = value;
		}
	}
}

// ViewModel with C# 14 extension properties
public class PersonModel
{
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public int Age { get; set; }
}

// C# 14 extension members that add computed properties to PersonModel
public static class PersonModelExtensions
{
	extension(PersonModel person)
	{
		public string FullName => $"{person.FirstName} {person.LastName}";
		public string DisplayInfo => $"{person.FirstName} {person.LastName} (Age: {person.Age})";
	}
}

// C# 14 extension members that add properties to collections
public static class CollectionExtensions
{
	extension<T>(ICollection<T> collection)
	{
		public bool IsEmpty => collection.Count == 0;
	}
}

// ViewModel that exposes C# 14 extension properties through regular properties (for XAML binding)
public class ExtensionPropertiesViewModel : INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	private PersonModel _person = new() { FirstName = "John", LastName = "Doe", Age = 30 };
	public PersonModel Person
	{
		get => _person;
		set
		{
			_person = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(FullName));
			OnPropertyChanged(nameof(DisplayInfo));
		}
	}

	// Expose C# 14 extension property through a regular property for XAML binding
	public string FullName => _person.FullName;
	public string DisplayInfo => _person.DisplayInfo;

	private List<string> _items = new();
	public List<string> Items
	{
		get => _items;
		set
		{
			_items = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(IsCollectionEmpty));
		}
	}

	// Expose C# 14 extension property on ICollection<T> through a regular property for XAML binding
	public bool IsCollectionEmpty => _items.IsEmpty;

	protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

// All XAML inflators (Runtime, XamlC, SourceGen) now support C# 14 extension properties.
// Extension properties appear as IPropertySymbol on static classes with get_X/set_X accessors
// that take the target type as the first parameter.
public partial class ExtensionProperties : ContentPage
{
	public ExtensionProperties() => InitializeComponent();

	[Collection("Xaml Inflation")]
	public class Tests : IDisposable
	{
		public Tests() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		public void Dispose() => DispatcherProvider.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyCanBeSetFromXaml(XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			// Verify the C# 14 extension property was set from XAML (like a regular property)
			Assert.Equal("Hello from extension property", page.labelWithExtProp.MyTag);
			Assert.Equal("Regular text", page.labelWithExtProp.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void MultipleExtensionPropertiesCanBeSetFromXaml(XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			// Verify multiple C# 14 extension properties were set from XAML
			Assert.Equal("Tag value", page.labelWithMultipleExtProps.MyTag);
			Assert.Equal(42, page.labelWithMultipleExtProps.MyPriority);
		}

		[Fact]
		internal void ExtensionPropertyCanBeSetAndReadInCode()
		{
			var label = new Label();

			// Set via C# 14 extension property syntax
			label.MyTag = "Test value";
			label.MyPriority = 123;

			// Read via C# 14 extension property syntax
			Assert.Equal("Test value", label.MyTag);
			Assert.Equal(123, label.MyPriority);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyOnViewModelCanBeBoundTo(XamlInflator inflator)
		{
			var vm = new ExtensionPropertiesViewModel
			{
				Person = new PersonModel { FirstName = "Jane", LastName = "Smith", Age = 25 }
			};

			var page = new ExtensionProperties(inflator)
			{
				BindingContext = vm
			};

			// Verify binding to FullName (which uses C# 14 extension property internally)
			Assert.Equal("Jane Smith", page.labelWithBinding.Text);

			// Verify binding to DisplayInfo (computed C# 14 extension property)
			Assert.Equal("Jane Smith (Age: 25)", page.labelWithComputedBinding.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyOnCollectionWorks(XamlInflator inflator)
		{
			var vm = new ExtensionPropertiesViewModel
			{
				Items = new List<string>() // Empty list
			};

			var page = new ExtensionProperties(inflator)
			{
				BindingContext = vm
			};

			// Verify binding to IsCollectionEmpty (which uses C# 14 extension property on ICollection<T>)
			Assert.Equal("True", page.labelWithIsEmptyBinding.Text);

			// Update collection and verify change
			vm.Items = new List<string> { "item1", "item2" };
			Assert.Equal("False", page.labelWithIsEmptyBinding.Text);
		}

		[Theory]
		[XamlInflatorData]
		internal void ExtensionPropertyUpdatesProperly(XamlInflator inflator)
		{
			var vm = new ExtensionPropertiesViewModel();
			var page = new ExtensionProperties(inflator)
			{
				BindingContext = vm
			};

			// Initial value
			Assert.Equal("John Doe", page.labelWithBinding.Text);

			// Update the person and verify binding updates
			vm.Person = new PersonModel { FirstName = "Alice", LastName = "Wonder", Age = 28 };
			Assert.Equal("Alice Wonder", page.labelWithBinding.Text);
			Assert.Equal("Alice Wonder (Age: 28)", page.labelWithComputedBinding.Text);
		}

		[Fact]
		internal void ExtensionPropertyDirectUsageInCode()
		{
			// Test C# 14 extension properties can be used directly in C# code
			var person = new PersonModel { FirstName = "Test", LastName = "User", Age = 42 };

			// Using the C# 14 extension property syntax
			Assert.Equal("Test User", person.FullName);
			Assert.Equal("Test User (Age: 42)", person.DisplayInfo);

			// Test C# 14 collection extension property
			var emptyList = new List<int>();
			var nonEmptyList = new List<int> { 1, 2, 3 };

			Assert.True(emptyList.IsEmpty);
			Assert.False(nonEmptyList.IsEmpty);
		}
	}
}
