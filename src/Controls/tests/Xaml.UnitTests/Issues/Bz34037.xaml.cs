using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[AcceptEmptyServiceProvider]
	public class Bz34037Converter0 : IValueConverter
	{
		public static int Invoked { get; set; }
		public static object Parameter { get; set; }
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Invoked++;
			Parameter = parameter;
			return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return new Bz34037Converter0();
		}
	}

	[AcceptEmptyServiceProvider]
	public class Bz34037Converter1 : IValueConverter, IMarkupExtension
	{
		public static int Invoked { get; set; }
		public static object Parameter { get; set; }
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Invoked++;
			Parameter = parameter;
			return true;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return new Bz34037Converter1();
		}
	}

	public partial class Bz34037 : ContentPage
	{
		public Bz34037()
		{
			InitializeComponent();
		}

		public string Property
		{
			get { return "FooBar"; }
		}

		[TestFixture]
		class Tests
		{
			[SetUp]
			public void Setup()
			{
				Bz34037Converter0.Invoked = 0;
				Bz34037Converter1.Invoked = 0;
			}

			[TearDown]
			public void TearDown()
			{
				Application.Current = null;
			}


			[Test]
			public void ConverterParameterOrderDoesNotMatters([Values] XamlInflator inflator)
			{
				var layout = new Bz34037(inflator);
				Assert.AreEqual(1, Bz34037Converter0.Invoked);
				Assert.AreEqual(1, Bz34037Converter1.Invoked);
				Assert.AreEqual(typeof(string), Bz34037Converter0.Parameter);
				Assert.AreEqual(typeof(string), Bz34037Converter1.Parameter);
				Assert.That(layout.s0.IsToggled, Is.True);
				Assert.That(layout.s1.IsToggled, Is.True);
			}
		}
	}
}