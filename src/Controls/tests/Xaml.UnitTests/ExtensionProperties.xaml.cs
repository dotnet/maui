// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
#nullable enable
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

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

// Runtime and XamlC now support C# 14 extension properties
// SourceGen: C# 14 extension members require Roslyn to fully lower the extension blocks
// before the semantic model exposes get_/set_ accessor methods. Since SourceGen runs
// during compilation (before full lowering), it cannot yet see the generated accessors.
// This limitation may be resolved in future Roslyn versions with better C# 14 support.
// Using .rtxc.xaml extension to restrict to Runtime and XamlC only for now.
public partial class ExtensionProperties : ContentPage
{
	public ExtensionProperties() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup() => DispatcherProvider.SetCurrent(new DispatcherProviderStub());

		[TearDown]
		public void TearDown() => DispatcherProvider.SetCurrent(null);

		[Test]
		public void ExtensionPropertyCanBeSetFromXaml([Values(XamlInflator.Runtime, XamlInflator.XamlC)] XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			// Verify the C# 14 extension property was set from XAML (like a regular property)
			Assert.That(page.labelWithExtProp.MyTag, Is.EqualTo("Hello from extension property"));
			Assert.That(page.labelWithExtProp.Text, Is.EqualTo("Regular text"));
		}

		[Test]
		public void MultipleExtensionPropertiesCanBeSetFromXaml([Values(XamlInflator.Runtime, XamlInflator.XamlC)] XamlInflator inflator)
		{
			var page = new ExtensionProperties(inflator);

			// Verify multiple C# 14 extension properties were set from XAML
			Assert.That(page.labelWithMultipleExtProps.MyTag, Is.EqualTo("Tag value"));
			Assert.That(page.labelWithMultipleExtProps.MyPriority, Is.EqualTo(42));
		}

		[Test]
		public void ExtensionPropertyCanBeSetAndReadInCode()
		{
			var label = new Label();

			// Set via C# 14 extension property syntax
			label.MyTag = "Test value";
			label.MyPriority = 123;

			// Read via C# 14 extension property syntax
			Assert.That(label.MyTag, Is.EqualTo("Test value"));
			Assert.That(label.MyPriority, Is.EqualTo(123));
		}

		[Test]
		public void ExtensionPropertyOnViewModelCanBeBoundTo([Values(XamlInflator.Runtime, XamlInflator.XamlC)] XamlInflator inflator)
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
			Assert.That(page.labelWithBinding.Text, Is.EqualTo("Jane Smith"));

			// Verify binding to DisplayInfo (computed C# 14 extension property)
			Assert.That(page.labelWithComputedBinding.Text, Is.EqualTo("Jane Smith (Age: 25)"));
		}

		[Test]
		public void ExtensionPropertyOnCollectionWorks([Values(XamlInflator.Runtime, XamlInflator.XamlC)] XamlInflator inflator)
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
			Assert.That(page.labelWithIsEmptyBinding.Text, Is.EqualTo("True"));

			// Update collection and verify change
			vm.Items = new List<string> { "item1", "item2" };
			Assert.That(page.labelWithIsEmptyBinding.Text, Is.EqualTo("False"));
		}

		[Test]
		public void ExtensionPropertyUpdatesProperly([Values(XamlInflator.Runtime, XamlInflator.XamlC)] XamlInflator inflator)
		{
			var vm = new ExtensionPropertiesViewModel();
			var page = new ExtensionProperties(inflator)
			{
				BindingContext = vm
			};

			// Initial value
			Assert.That(page.labelWithBinding.Text, Is.EqualTo("John Doe"));

			// Update the person and verify binding updates
			vm.Person = new PersonModel { FirstName = "Alice", LastName = "Wonder", Age = 28 };
			Assert.That(page.labelWithBinding.Text, Is.EqualTo("Alice Wonder"));
			Assert.That(page.labelWithComputedBinding.Text, Is.EqualTo("Alice Wonder (Age: 28)"));
		}

		[Test]
		public void ExtensionPropertyDirectUsageInCode()
		{
			// Test C# 14 extension properties can be used directly in C# code
			var person = new PersonModel { FirstName = "Test", LastName = "User", Age = 42 };

			// Using the C# 14 extension property syntax
			Assert.That(person.FullName, Is.EqualTo("Test User"));
			Assert.That(person.DisplayInfo, Is.EqualTo("Test User (Age: 42)"));

			// Test C# 14 collection extension property
			var emptyList = new List<int>();
			var nonEmptyList = new List<int> { 1, 2, 3 };

			Assert.That(emptyList.IsEmpty, Is.True);
			Assert.That(nonEmptyList.IsEmpty, Is.False);
		}
	}
}
