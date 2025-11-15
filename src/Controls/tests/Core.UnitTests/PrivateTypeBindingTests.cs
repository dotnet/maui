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
			// using the source generated code. The types are private nested classes so that
			// the SetBinding source generator uses UnsafeAccessorType.
			
			var label = new Label();
			var viewModel = new ViewModel
			{
				Contact = new Contact
				{
					FullName = new FullName
					{
						FirstName = "John",
						LastName = "Doe"
					}
				}
			};

			// Use the source generated SetBinding method with null-conditional operators
			label.SetBinding(Label.TextProperty, static (ViewModel vm) => vm.Contact?.FullName?.FirstName);
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
			var viewModel = new ViewModel();

			label.SetBinding(Label.TextProperty, static (ViewModel vm) => vm.Contact?.FullName?.FirstName);
			label.BindingContext = viewModel;

			// Initial value should be empty
			Assert.Equal("", label.Text);

			// Update nested property
			viewModel.Contact.FullName.FirstName = "Alice";
			Assert.Equal("Alice", label.Text);

			// Change the entire Contact object
			viewModel.Contact = new Contact
			{
				FullName = new FullName
				{
					FirstName = "Bob"
				}
			};

			Assert.Equal("Bob", label.Text);
		}

		[Fact]
		public void TwoWayBindingWithPrivateType()
		{
			// Test two-way binding with source generated code
			
			var entry = new Entry();
			var viewModel = new SimpleViewModel
			{
				Name = "Initial"
			};

			entry.SetBinding(Entry.TextProperty, static (SimpleViewModel vm) => vm.Name, BindingMode.TwoWay);
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

		// Private nested view model classes that implement INotifyPropertyChanged
		// The SetBinding source generator will use UnsafeAccessorType to access these
		private class ViewModel : System.ComponentModel.INotifyPropertyChanged
		{
			private Contact _contact = new Contact();
			public Contact Contact
			{
				get => _contact;
				set
				{
					_contact = value;
					PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Contact)));
				}
			}

			public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
		}

		private class Contact : System.ComponentModel.INotifyPropertyChanged
			{
				private FullName _fullName = new FullName();
				public FullName FullName
				{
					get => _fullName;
					set
					{
						_fullName = value;
						PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(FullName)));
					}
				}

				public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
			}

		private class FullName : System.ComponentModel.INotifyPropertyChanged
			{
				private string _firstName = "";
				private string _lastName = "";

				public string FirstName
				{
					get => _firstName;
					set
					{
						_firstName = value;
						PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(FirstName)));
					}
				}

				public string LastName
				{
					get => _lastName;
					set
					{
						_lastName = value;
						PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(LastName)));
					}
				}

				public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
			}

		private class SimpleViewModel : System.ComponentModel.INotifyPropertyChanged
			{
				private string _name = "";
				public string Name
				{
					get => _name;
					set
					{
						_name = value;
						PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Name)));
					}
				}

				public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
			}
	}
}
