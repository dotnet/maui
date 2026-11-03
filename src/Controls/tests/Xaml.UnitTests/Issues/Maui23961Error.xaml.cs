using System;
using System.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23961Error : ContentPage
{
	public Maui23961Error() => InitializeComponent();

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
		internal void ObsoleteErrorProducesCompilationError(XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				// [Obsolete("msg", error: true)] must always produce a compilation error,
				// even without TreatWarningsAsErrors
				// Multiple [Obsolete(error:true)] members produce multiple LoggedErrors wrapped in AggregateException,
				// so use ThrowsAny rather than Throws (which requires an exact type match).
				var ex = Assert.ThrowsAny<Exception>(() => MockCompiler.Compile(typeof(Maui23961Error), treatWarningsAsErrors: false));
				Assert.Contains("XC0619", ex.Message, StringComparison.Ordinal);
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				// SourceGen does not process .rt.xaml files
				return;
			}
			else
			{
				// .rt.xaml files only support runtime inflation (no XamlInflator ctor generated)
				var page = new Maui23961Error();
				Assert.NotNull(page);
			}
		}
	}
}

// Test classes for obsolete attribute error handling
public class Maui23961ErrorClass1 : Grid
{
	[Obsolete("[Error Message]: BP2Property is obsolete", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static readonly BindableProperty BP2Property =
		BindableProperty.Create(nameof(BP2), typeof(string), typeof(Maui23961ErrorClass1), null);

	[Obsolete("[Error Message]: BP2 property is obsolete", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string BP2
	{
		get => (string)GetValue(BP2Property);
		set => SetValue(BP2Property, value);
	}

	[Obsolete("[Error Message]: P2 is obsolete", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string P2 { get; set; }
}

[Obsolete("[Error Message]: Maui23961ObsoleteClass3 is obsolete", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class Maui23961ObsoleteClass3 : Grid { }
