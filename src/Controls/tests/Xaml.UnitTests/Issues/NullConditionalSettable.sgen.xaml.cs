// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

#nullable enable
using System.ComponentModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public class NullConditionalUser
{
	public string Name { get; set; } = "Test";
}

public class NullConditionalViewModel : INotifyPropertyChanged
{
	// Use non-null User for simpler test - the null-conditional still applies
	// but the actual object will be non-null in our test
	private NullConditionalUser _user = new();

	public NullConditionalUser User
	{
		get => _user;
		set
		{
			_user = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(User)));
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;
}

public partial class NullConditionalSettable : ContentPage
{
	public NullConditionalSettable() => InitializeComponent();

	[Collection("Issue")]
	public class Tests
	{
		[Fact]
		internal void NullConditionalAccessIsSettable()
		{
			// This test verifies that null-conditional access (?.) works in two-way bindings.
			// In C# 14+ (default for .NET 10+), expressions like "user?.Name = value" are valid.
			// C# Expressions only work with SourceGen, not Runtime or XamlC.
			var viewModel = new NullConditionalViewModel
			{
				User = new NullConditionalUser { Name = "Initial" }
			};

			var page = new NullConditionalSettable(XamlInflator.SourceGen);
			page.BindingContext = viewModel;

			Assert.NotNull(page);
			Assert.NotNull(page.testEntry);

			// Verify the getter works (reads from User?.Name)
			Assert.Equal("Initial", page.testEntry.Text);

			// Verify the setter works (writes to User?.Name)
			page.testEntry.Text = "Updated";
			Assert.Equal("Updated", viewModel.User.Name);
		}
	}
}
