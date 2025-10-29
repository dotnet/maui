using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;
public partial class Maui25309 : ContentPage
{
	public Maui25309()
	{
		InitializeComponent();
	}

	public Maui25309(bool useCompiledXaml)
	{
		//this stub will be replaced at compile time
	}	class Test
	{
		// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
		}

		[Theory]
			public void Method(bool useCompiledXaml)
		{
			if (useCompiledXaml)
				Assert.DoesNotThrow(() => MockCompiler.Compile(typeof(Maui25309)));

			var page = new Maui25309(useCompiledXaml) { BindingContext = new { IsValid = true } };
			var converter = page.Resources["IsValidConverter"] as Maui25309BoolToObjectConverter;
			Assert.NotNull(converter);
			Assert.Equal(Color.Parse("#140F4B", page.label.BackgroundColor));
		}
	}
}

#nullable enable
class Maui25309BoolToObjectConverter : Maui25309BoolToObjectConverter<object>
{
}

public class Maui25309BoolToObjectConverter<TObject> : IValueConverter
{
	public TObject? TrueObject { get; set; }

	public TObject? FalseObject { get; set; }

	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		bool boolValue = false;
		if (value is bool bv)
			boolValue = bv;

		return boolValue ? TrueObject : FalseObject;
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}