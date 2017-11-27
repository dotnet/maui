using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Xamarin.Forms.Platform.UWP
{
	class NativePropertyListener : DependencyObject, INotifyPropertyChanged
	{
		readonly DependencyObject _target;
		readonly string _targetProperty;

		public static readonly DependencyProperty TargetPropertyValueProperty = DependencyProperty.Register(nameof(TargetPropertyValue), typeof(object), typeof(NativePropertyListener), new PropertyMetadata(null, OnNativePropertyChanged));

		public event PropertyChangedEventHandler PropertyChanged;

		public NativePropertyListener(DependencyObject target, string propertyName)
		{
			_target = target;
			_targetProperty = propertyName;
			BindingOperations.SetBinding(this, TargetPropertyValueProperty, new Windows.UI.Xaml.Data.Binding() { Source = _target, Path = new PropertyPath(_targetProperty), Mode = Windows.UI.Xaml.Data.BindingMode.OneWay });
		}

		public void Dispose()
		{
			ClearValue(TargetPropertyValueProperty);
		}

		public object TargetPropertyValue
		{
			get
			{
				return GetValue(TargetPropertyValueProperty);
			}
		}

		static void OnNativePropertyChanged(object sender, DependencyPropertyChangedEventArgs args)
		{
			NativePropertyListener source = (NativePropertyListener)sender;
			source?.PropertyChanged?.Invoke(source._target, new PropertyChangedEventArgs(source._targetProperty));
		}		
	}
}