#nullable enable

using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	public class PrivateTypeBindingTests : IDisposable
	{
		public PrivateTypeBindingTests()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			ApplicationExtensions.CreateAndSetMockApplication();
		}

		public void Dispose()
		{
			Application.ClearCurrent();
			DispatcherProvider.SetCurrent(null);
		}

		[Fact]
		public void ComplexBindingWithInaccessibleTypeInMiddle()
		{
			// This test validates that a complex binding path works correctly at runtime
			// using the source generated code. The types are internal (not private) so that
			// the test code can access them, but the SetBinding lambda is still source generated
			
			var label = new Label();
			var viewModel = new TestPage.ViewModel
			{
				Contact = new TestPage.Contact
				{
					FullName = new TestPage.FullName
					{
						FirstName = "John",
						LastName = "Doe"
					}
				}
			};

			// Use the source generated SetBinding method with null-conditional operators
			label.SetBinding(Label.TextProperty, static (TestPage.ViewModel vm) => vm.Contact?.FullName?.FirstName);
			label.BindingContext = viewModel;

			// Verify the binding works
			Assert.Equal("John", label.Text);

			// Verify updates propagate
			viewModel.Contact.FullName.FirstName = "Jane";
			Assert.Equal("Jane", label.Text);
		}

		[Fact]
		public void BindingWithUpdatedNestedProperties()
		{
			// Test that property updates in nested objects propagate correctly
			
			var label = new Label();
			var viewModel = new TestPage.ViewModel();

			label.SetBinding(Label.TextProperty, static (TestPage.ViewModel vm) => vm.Contact?.FullName?.FirstName);
			label.BindingContext = viewModel;

			// Initial value should be empty
			Assert.Equal("", label.Text);

			// Update nested property
			viewModel.Contact.FullName.FirstName = "Alice";
			Assert.Equal("Alice", label.Text);

			// Change the entire Contact object
			viewModel.Contact = new TestPage.Contact
			{
				FullName = new TestPage.FullName
				{
					FirstName = "Bob"
				}
			};

			Assert.Equal("Bob", label.Text);
		}

		[Fact]
		public void TwoWayBindingWithInternalType()
		{
			// Test two-way binding with source generated code
			
			var entry = new Entry();
			var viewModel = new TestPage.SimpleViewModel
			{
				Name = "Initial"
			};

			entry.SetBinding(Entry.TextProperty, static (TestPage.SimpleViewModel vm) => vm.Name, BindingMode.TwoWay);
			entry.BindingContext = viewModel;

			// Verify initial value
			Assert.Equal("Initial", entry.Text);

			// Change view model - should update entry
			viewModel.Name = "Updated";
			Assert.Equal("Updated", entry.Text);

			// Change entry - should update view model
			entry.Text = "FromEntry";
			Assert.Equal("FromEntry", viewModel.Name);
		}

		// Test page class with internal nested types
		// Note: These must be internal (not private) since we access them from test code
		// The SetBinding source generator will treat them as accessible and not use UnsafeAccessor
		// For testing actual private types, see the integration tests in BindingSourceGen.UnitTests
		public class TestPage
		{
			// Internal view model with nested internal types
			// Using simple properties without BindableObject to avoid dispatcher requirements
			internal class ViewModel
			{
				public Contact Contact { get; set; } = new Contact();
			}

			internal class Contact
			{
				public FullName FullName { get; set; } = new FullName();
			}

			internal class FullName
			{
				public string FirstName { get; set; } = "";
				public string LastName { get; set; } = "";
			}

			internal class SimpleViewModel
			{
				public string Name { get; set; } = "";
			}
		}
	}
}
