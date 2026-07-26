using System;
using System.ComponentModel;
using Microsoft.Build.Framework;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23961 : ContentPage
{
	public Maui23961() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			AppInfo.SetCurrent(new MockAppInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void ObsoleteWarningIncludesMessage(XamlInflator inflator)
		{
			// This test verifies that obsolete warnings include the custom message from the ObsoleteAttribute
			// and that types/properties with [Obsolete(error: false)] produce warnings, not errors

			if (inflator == XamlInflator.XamlC)
			{
				// Compile with warnings allowed and capture the emitted warnings
				MockCompiler.Compile(typeof(Maui23961), out _, out _, out var warnings, treatWarningsAsErrors: false);
				// Verify the diagnostic messages include the custom [Obsolete] message text
				Assert.Contains(warnings, w => w.Message.Contains("[Test Message]", StringComparison.Ordinal));
			}
			else
			{
				var page = new Maui23961(inflator);
				Assert.NotNull(page);
			}
		}

		[Theory]
		[XamlInflatorData]
		internal void ObsoleteWarningBecomesErrorWhenTreatWarningsAsErrors(XamlInflator inflator)
		{
			// When TreatWarningsAsErrors is true, obsolete warnings should become errors
			if (inflator == XamlInflator.XamlC)
			{
				// Should throw when treating warnings as errors
				var ex = Assert.ThrowsAny<Exception>(() => MockCompiler.Compile(typeof(Maui23961), treatWarningsAsErrors: true));
				Assert.NotNull(ex);
			}
			else
			{
				// Runtime and SourceGen don't have treatWarningsAsErrors
				var page = new Maui23961(inflator);
				Assert.NotNull(page);
			}
		}
	}
}

// Test classes for obsolete attribute handling
[Obsolete("[Test Message]: Maui23961ObsoleteClass2 is deprecated", error: false)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class Maui23961ObsoleteClass2 : Grid { }

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
