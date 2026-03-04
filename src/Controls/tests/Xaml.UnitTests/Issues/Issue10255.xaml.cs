using System;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue10255 : ContentPage
{
	public Issue10255() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose() => AppInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void GenericConverterWithGridLengthWorks(XamlInflator inflator)
		{
#if !NET11_0_OR_GREATER
			if (inflator == XamlInflator.XamlC)
				return; // Skip: XamlC compilation of implicit string to GridLength cast requires .NET 11 or greater
#endif
			var page = new Issue10255(inflator);
			
			// Test that the converters were created successfully
			var failedConverter = page.Resources["FailedConverter"] as Issue10255BoolToObjectConverter<GridLength>;
			Assert.NotNull(failedConverter);
			Assert.True(failedConverter.TrueObject.IsStar, "TrueObject should be Star");
			Assert.Equal(1.0, failedConverter.TrueObject.Value);
			Assert.True(failedConverter.FalseObject.IsAbsolute, "FalseObject should be Absolute");
			Assert.Equal(80.0, failedConverter.FalseObject.Value);

			var starConverter = page.Resources["StarConverter"] as Issue10255BoolToObjectConverter<GridLength>;
			Assert.NotNull(starConverter);
			Assert.True(starConverter.TrueObject.IsStar, "TrueObject should be Star");
			Assert.Equal(2.0, starConverter.TrueObject.Value);
			Assert.True(starConverter.FalseObject.IsAuto, "FalseObject should be Auto");

			var workingConverter = page.Resources["WorkingConverter"] as Issue10255BoolToObjectConverter<int>;
			Assert.NotNull(workingConverter);
			Assert.Equal(10, workingConverter.TrueObject);
			Assert.Equal(80, workingConverter.FalseObject);
		}

#if NET11_0_OR_GREATER
		[Fact]
		public void ImplicitStringCastWorks()
		{
			// Test implicit cast from string to GridLength
			GridLength star = "*";
			Assert.True(star.IsStar);
			Assert.Equal(1.0, star.Value);

			GridLength twoStar = "2*";
			Assert.True(twoStar.IsStar);
			Assert.Equal(2.0, twoStar.Value);

			GridLength auto = "auto";
			Assert.True(auto.IsAuto);

			GridLength absolute = "80";
			Assert.True(absolute.IsAbsolute);
			Assert.Equal(80.0, absolute.Value);

			GridLength absoluteWithDecimal = "45.5";
			Assert.True(absoluteWithDecimal.IsAbsolute);
			Assert.Equal(45.5, absoluteWithDecimal.Value);
		}

		[Fact]
		public void ImplicitStringCastThrowsOnNull()
		{
			Assert.Throws<ArgumentNullException>(() => { GridLength gl = (string)null; });
		}

		[Fact]
		public void ImplicitStringCastThrowsOnInvalidFormat()
		{
			Assert.Throws<FormatException>(() => { GridLength gl = "invalid"; });
		}
#endif
	}
}

#nullable enable
public class Issue10255BoolToObjectConverter<TObject> : IValueConverter
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
