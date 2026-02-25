using System;
using System.ComponentModel;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Build.Tasks;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui23961Error : ContentPage
{
	public Maui23961Error() => InitializeComponent();

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
		public void ObsoleteErrorProducesCompilationError([Values] XamlInflator inflator)
		{
			if (inflator == XamlInflator.XamlC)
			{
				// [Obsolete("msg", error: true)] must always produce a compilation error,
				// even without TreatWarningsAsErrors
				Assert.Throws<Exception>(() => MockCompiler.Compile(typeof(Maui23961Error), treatWarningsAsErrors: false));
			}
			else if (inflator == XamlInflator.SourceGen)
			{
				Assert.Ignore("SourceGen does not process .rt.xaml files");
			}
			else
			{
				var page = new Maui23961Error(inflator);
				Assert.That(page, Is.Not.Null);
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
