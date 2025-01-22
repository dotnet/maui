using System;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

[XamlProcessing(XamlInflator.Default, true)]
public partial class Unreported008 : ContentPage
{
	public Unreported008() => InitializeComponent();

	class Tests
	{
		[Test]
		public void PickerDateTimesAndXamlC([Values] XamlInflator inflator)
		{
			var page = new Unreported008(inflator);
			var picker = page.picker0;
			Assert.AreEqual(DateTime.Today, picker.Date.Date);
			Assert.AreEqual(new DateTime(2000, 1, 1), picker.MinimumDate);
			Assert.AreEqual(new DateTime(2050, 12, 31), picker.MaximumDate);
		}
	}
}