using System;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class GrialIssue02Converter : IValueConverter
	{
		public object FalseValue
		{
			get;
			set;
		}

		public object TrueValue
		{
			get;
			set;
		}

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool))
			{
				return null;
			}
			bool flag = (bool)value;
			return (!flag) ? FalseValue : TrueValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}

	public partial class GrialIssue02 : ContentPage
	{
		public GrialIssue02()
		{
			InitializeComponent();
		}
		public GrialIssue02(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true)]
			[TestCase(false)]
			public void BoxValueTypes(bool useCompiledXaml)
			{
				var layout = new GrialIssue02(useCompiledXaml);
				var res = (GrialIssue02Converter)layout.Resources["converter"];

				Assert.AreEqual(FontAttributes.None, res.TrueValue);
				Assert.AreEqual(FontAttributes.Bold, res.FalseValue);
			}
		}
	}
}