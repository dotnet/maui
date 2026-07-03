#nullable disable
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Foundation;

#if __MOBILE__
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	class NativeViewPropertyListener : NSObject, INotifyPropertyChanged
	{
		string TargetProperty { get; set; }

		public NativeViewPropertyListener(string targetProperty)
		{
			TargetProperty = targetProperty;
		}

		[UnconditionalSuppressMessage("Memory", "MEM0001", Justification = "PropertyChanged must be public to implement INotifyPropertyChanged for the native property listener.")]
		public event PropertyChangedEventHandler PropertyChanged;

		public override void ObserveValue(NSString keyPath, NSObject ofObject, NSDictionary change, IntPtr context)
		{
			if (keyPath.ToString().Equals(TargetProperty, StringComparison.OrdinalIgnoreCase))
				PropertyChanged?.Invoke(ofObject, new PropertyChangedEventArgs(TargetProperty));
			else
				base.ObserveValue(keyPath, ofObject, change, context);
		}
	}
}