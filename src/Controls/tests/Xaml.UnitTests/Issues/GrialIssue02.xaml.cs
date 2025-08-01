using System;
using System.Globalization;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

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
		if (value is not bool)
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
	public GrialIssue02() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		[Test]
		public void BoxValueTypes([Values] XamlInflator inflator)
		{
			var layout = new GrialIssue02(inflator);
			var res = (GrialIssue02Converter)layout.Resources["converter"];

			Assert.AreEqual(FontAttributes.None, res.TrueValue);
			Assert.AreEqual(FontAttributes.Bold, res.FalseValue);
		}
	}
}