using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

/// <summary>
/// Collection definition for xUnit tests that test XAML inflation features.
/// Used for tests in the root directory.
/// DisableParallelization is required because tests share static state (Application.Current, etc.)
/// </summary>
[CollectionDefinition("Xaml Inflation", DisableParallelization = true)]
public class XamlInflationCollection : ICollectionFixture<MauiCompatibilityFixture>
{
}

/// <summary>
/// Collection definition for xUnit issue tests.
/// Used for tests in the Issues/ directory.
/// DisableParallelization is required because tests share static state (Application.Current, etc.)
/// </summary>
[CollectionDefinition("Issue", DisableParallelization = true)]
public class IssueCollection : ICollectionFixture<MauiCompatibilityFixture>
{
}

/// <summary>
/// Fixture that initializes MAUI compatibility mode once per test collection.
/// Equivalent to NUnit's [SetUpFixture] with [OneTimeSetUp].
/// </summary>
public class MauiCompatibilityFixture : IDisposable
{
	public MauiCompatibilityFixture()
	{
		Microsoft.Maui.Controls.Hosting.CompatibilityCheck.UseCompatibility();
	}

	public void Dispose()
	{
		// Cleanup if needed
	}
}

/// <summary>
/// Fixture that sets up Application.Current for tests that require it.
/// </summary>
public class ApplicationFixture : IDisposable
{
	public ApplicationFixture()
	{
		Application.Current = new MockApplication();
	}

	public void Dispose()
	{
		Application.Current = null;
	}
}

/// <summary>
/// Fixture that sets up DispatcherProvider for tests that require it.
/// </summary>
public class DispatcherProviderFixture : IDisposable
{
	public DispatcherProviderFixture()
	{
		DispatcherProvider.SetCurrent(new DispatcherProviderStub());
	}

	public void Dispose()
	{
		DispatcherProvider.SetCurrent(null);
	}
}
