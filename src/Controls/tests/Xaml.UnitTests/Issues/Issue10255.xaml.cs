using System;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Issue10255 : ContentPage
{
	public Issue10255() => InitializeComponent();

	class Test
	{
		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		[TearDown]
		public void TearDown() => AppInfo.SetCurrent(null);

		[Test]
		public void GenericConverterWithGridLengthWorks([Values] XamlInflator inflator)
		{
#if !NET11_0_OR_GREATER
			if (inflator == XamlInflator.XamlC)
				Assert.Ignore("XamlC compilation of implicit string to GridLength cast requires .NET 11 or greater");
#endif
			var page = new Issue10255(inflator);
			
			// Test that the converters were created successfully
			var failedConverter = page.Resources["FailedConverter"] as Issue10255BoolToObjectConverter<GridLength>;
			Assert.IsNotNull(failedConverter, "FailedConverter should not be null");
			Assert.That(failedConverter.TrueObject.IsStar, Is.True, "TrueObject should be Star");
			Assert.That(failedConverter.TrueObject.Value, Is.EqualTo(1.0), "Star value should be 1.0");
			Assert.That(failedConverter.FalseObject.IsAbsolute, Is.True, "FalseObject should be Absolute");
			Assert.That(failedConverter.FalseObject.Value, Is.EqualTo(80.0), "Absolute value should be 80.0");

			var starConverter = page.Resources["StarConverter"] as Issue10255BoolToObjectConverter<GridLength>;
			Assert.IsNotNull(starConverter, "StarConverter should not be null");
			Assert.That(starConverter.TrueObject.IsStar, Is.True, "TrueObject should be Star");
			Assert.That(starConverter.TrueObject.Value, Is.EqualTo(2.0), "Star value should be 2.0");
			Assert.That(starConverter.FalseObject.IsAuto, Is.True, "FalseObject should be Auto");

			var workingConverter = page.Resources["WorkingConverter"] as Issue10255BoolToObjectConverter<int>;
			Assert.IsNotNull(workingConverter, "WorkingConverter should not be null");
			Assert.That(workingConverter.TrueObject, Is.EqualTo(10));
			Assert.That(workingConverter.FalseObject, Is.EqualTo(80));
		}

#if NET11_0_OR_GREATER
		[Test]
		public void ImplicitStringCastWorks()
		{
			// Test implicit cast from string to GridLength
			GridLength star = "*";
			Assert.That(star.IsStar, Is.True);
			Assert.That(star.Value, Is.EqualTo(1.0));

			GridLength twoStar = "2*";
			Assert.That(twoStar.IsStar, Is.True);
			Assert.That(twoStar.Value, Is.EqualTo(2.0));

			GridLength auto = "auto";
			Assert.That(auto.IsAuto, Is.True);

			GridLength absolute = "80";
			Assert.That(absolute.IsAbsolute, Is.True);
			Assert.That(absolute.Value, Is.EqualTo(80.0));

			GridLength absoluteWithDecimal = "45.5";
			Assert.That(absoluteWithDecimal.IsAbsolute, Is.True);
			Assert.That(absoluteWithDecimal.Value, Is.EqualTo(45.5));
		}

		[Test]
		public void ImplicitStringCastThrowsOnNull()
		{
			Assert.Throws<ArgumentNullException>(() => { GridLength gl = (string)null; });
		}

		[Test]
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
