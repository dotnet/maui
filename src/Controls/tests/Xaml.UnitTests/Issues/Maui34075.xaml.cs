// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable
using System;
using System.ComponentModel;
using System.Windows.Input;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// ViewModel with read-only properties to reproduce issue #34075.
/// C# expression source gen should not generate setters for these.
/// </summary>
public class Maui34075ViewModel : INotifyPropertyChanged
{
	// Getter-only property (expression-bodied) - common for commands
	public ICommand MyCommand => new Command(() => { });

	// Getter-only property - common for computed/display values
	public string ReadOnlyText => "Hello";

#pragma warning disable CS0067 // Event is never used
	public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore CS0067
}

public partial class Maui34075 : ContentPage
{
	public Maui34075() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		/// <summary>
		/// Verifies that C# expression bindings to getter-only properties
		/// compile and inflate without errors (no setter generated for read-only props).
		/// See: https://github.com/dotnet/maui/issues/34075
		/// </summary>
		[Fact]
		public void ReadOnlyPropertyExpressionBindingCompilesSuccessfully()
		{
			var page = new Maui34075(XamlInflator.SourceGen);
			page.BindingContext = new Maui34075ViewModel();

			Assert.NotNull(page);
			Assert.Equal("Hello", page.readOnlyLabel.Text);
		}
	}
}
