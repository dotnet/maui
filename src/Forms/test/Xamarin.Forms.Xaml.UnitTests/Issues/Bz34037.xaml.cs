using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xamarin.Forms;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class Bz34037Converter0 : IValueConverter, IMarkupExtension
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

		public Bz34037(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
				Bz34037Converter0.Invoked = 0;
				Bz34037Converter1.Invoked = 0;
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
				Application.Current = null;
			}


			[TestCase(true)]
			[TestCase(false)]
			public void ConverterParameterOrderDoesNotMatters(bool useCompiledXaml)
			{
				var layout = new Bz34037(useCompiledXaml);
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