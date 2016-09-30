using System;
using System.ComponentModel;
using Foundation;

namespace Xamarin.Forms.Platform.iOS
{
	class NativeViewPropertyListener : NSObject, INotifyPropertyChanged
	{
		string TargetProperty { get; set; }

		public NativeViewPropertyListener(string targetProperty)
		{
			TargetProperty = targetProperty;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			if (keyPath.ToString().Equals(TargetProperty, StringComparison.InvariantCultureIgnoreCase))
				PropertyChanged?.Invoke(ofObject, new PropertyChangedEventArgs(TargetProperty));
			else
				base.ObserveValue(keyPath, ofObject, change, context);
		}
	}
}
