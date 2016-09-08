using System;
using System.ComponentModel;
#if __UNIFIED__
using Foundation;

#else
using MonoTouch.Foundation;
#endif

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
			if (keyPath == TargetProperty)
				PropertyChanged?.Invoke(ofObject, new PropertyChangedEventArgs(TargetProperty));
		}
	}
}
