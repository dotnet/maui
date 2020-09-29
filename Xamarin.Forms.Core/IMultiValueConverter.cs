using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Xamarin.Forms
{
	//
	// (For Documentation Authors)
	// Results of different possible return values from Convert and ConvertBack:
	// 
	//			     |	Binding.DoNothing		Binding.UnsetValue					null
	// ----------------------------------------------------------------------------------------------------------
	// Convert:		 |	No update				MultiBinding.FallbackValue			MultiBinding.TargetNullValue
	// ConvertBack:	 |	No update to source[i]	No update at all					No update at all
	//	 (at position i)
	//
	public interface IMultiValueConverter
	{
		object Convert(object[] values, Type targetType, object parameter, CultureInfo culture);

		object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture);
	}
}