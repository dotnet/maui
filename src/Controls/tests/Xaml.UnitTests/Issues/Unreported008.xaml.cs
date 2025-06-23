using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public class Unreported008View : ContentView
	{
		public static readonly BindableProperty DateProperty = BindableProperty.Create(nameof(Date), typeof(DateTime?), typeof(Unreported008View), null);

		public DateTime? Date
		{
			get { return (DateTime?)GetValue(DateProperty); }
			set { SetValue(DateProperty, value); }
		}
	}

	public partial class Unreported008 : ContentPage
	{
		public Unreported008()
		{
			InitializeComponent();
		}

		public Unreported008(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		class Tests
		{
			[TestCase(true), TestCase(false)]
			public void PickerDateTimesAndXamlC(bool useCompiledXaml)
			{
				var page = new Unreported008(useCompiledXaml);
				var picker = page.picker0;
				Assert.AreEqual(DateTime.Today, picker.Date.Date);
				Assert.AreEqual(new DateTime(2000, 1, 1), picker.MinimumDate);
				Assert.AreEqual(new DateTime(2050, 12, 31), picker.MaximumDate);

				Assert.AreEqual(DateTime.Today, page.view0.Date.Value.Date);
			}
		}
	}
}