using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace Xamarin.Forms.Platform.WinPhone
{
	public class FormsListPicker : ListPicker
	{
		internal static readonly DependencyProperty ListPickerModeChangedProperty = DependencyProperty.Register("ListPickerMode", typeof(ListPickerMode), typeof(FormsListPicker),
			new PropertyMetadata(ModeChanged));

		protected virtual void OnListPickerModeChanged(DependencyPropertyChangedEventArgs args)
		{
			ListPickerModeChanged?.Invoke(this, args);
		}

		internal event EventHandler<DependencyPropertyChangedEventArgs> ListPickerModeChanged;

		static void ModeChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
		{
			var listPicker = dependencyObject as FormsListPicker;
			listPicker?.OnListPickerModeChanged(dependencyPropertyChangedEventArgs);
		}
	}
}