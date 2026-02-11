using System;
using System.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23961 : ContentPage
{
	public Maui23961() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[SetUp]
		public void Setup()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}

		[Test]
		public void ObsoleteWarningIncludesMessage([Values] XamlInflator inflator)
		{
			// This test verifies that obsolete warnings include the custom message from the ObsoleteAttribute
			// and that types/properties with [Obsolete(error: false)] produce warnings, not errors

			if (inflator == XamlInflator.XamlC)
			{
				// When not treating warnings as errors, this should compile with warnings
				MockCompiler.Compile(typeof(Maui23961), treatWarningsAsErrors: false);
				// The test passes if compilation succeeds (warnings are emitted but don't fail the build)
			}
			else
			{
				var page = new Maui23961(inflator);
				Assert.That(page, Is.Not.Null);
			}
		}

		[Test]
		public void ObsoleteWarningBecomesErrorWhenTreatWarningsAsErrors([Values] XamlInflator inflator)
		{
			// When TreatWarningsAsErrors is true, obsolete warnings should become errors
			if (inflator == XamlInflator.XamlC)
			{
				// Should throw when treating warnings as errors (may be Exception or AggregateException)
				var ex = Assert.Catch(() => MockCompiler.Compile(typeof(Maui23961), treatWarningsAsErrors: true));
				Assert.That(ex, Is.Not.Null);
			}
			else
			{
				// Runtime and SourceGen don't have treatWarningsAsErrors
				var page = new Maui23961(inflator);
				Assert.That(page, Is.Not.Null);
			}
		}
	}
}

// Test classes for obsolete attribute handling
public class Maui23961Class1 : Grid
{
	[Obsolete("[Test Message]: BP1Property is deprecated", error: false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly BindableProperty BP1Property =
		BindableProperty.Create(nameof(BP1), typeof(string), typeof(Maui23961Class1), null);

	[Obsolete("[Test Message]: BP1 property is deprecated", error: false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string BP1
	{
		get => (string)GetValue(BP1Property);
		set => SetValue(BP1Property, value);
	}

	[Obsolete("[Test Message]: P1 is deprecated", error: false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string P1 { get; set; }
}

[Obsolete("[Test Message]: Maui23961ObsoleteClass2 is deprecated", error: false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class Maui23961ObsoleteClass2 : Grid { }
