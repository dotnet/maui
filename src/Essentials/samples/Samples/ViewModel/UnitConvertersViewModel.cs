using System;
using System.Windows.Input;
using Microsoft.Maui.Essentials;
using Xamarin.Forms;

namespace Samples.ViewModel
{
	public class UnitConvertersViewModel : BaseViewModel
	{
		double fahrenheit;
		double celsius;
		double miles;
		double kilometers;

		public UnitConvertersViewModel()
		{
		}

		public double Fahrenheit
		{
			get => fahrenheit;
			set
			{
				SetProperty(ref fahrenheit, value);
				Celsius = UnitConverters.FahrenheitToCelsius(fahrenheit);
			}
		}

		public double Celsius
		{
			get => celsius;
			set => SetProperty(ref celsius, value);
		}

		public double Miles
		{
			get => miles;
			set
			{
				SetProperty(ref miles, value);
				Kilometers = UnitConverters.MilesToKilometers(miles);
			}
		}

		public double Kilometers
		{
			get => kilometers;
			set => SetProperty(ref kilometers, value);
		}
	}
}
