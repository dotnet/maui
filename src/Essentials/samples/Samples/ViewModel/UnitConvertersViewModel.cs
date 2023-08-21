// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Media;

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
			set => SetProperty(ref fahrenheit, value, onChanged: () => Celsius = UnitConverters.FahrenheitToCelsius(fahrenheit));
		}

		public double Celsius
		{
			get => celsius;
			set => SetProperty(ref celsius, value);
		}

		public double Miles
		{
			get => miles;
			set => SetProperty(ref miles, value, onChanged: () => Kilometers = UnitConverters.MilesToKilometers(miles));
		}

		public double Kilometers
		{
			get => kilometers;
			set => SetProperty(ref kilometers, value);
		}
	}
}
