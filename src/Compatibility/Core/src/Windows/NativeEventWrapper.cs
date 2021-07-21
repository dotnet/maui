using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using WinRT;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	class NativeEventWrapper : INotifyPropertyChanged
	{
		static string TargetProperty { get; set; }
		static readonly MethodInfo s_handlerinfo = typeof(NativeEventWrapper).GetRuntimeMethods().Single(mi => mi.Name == nameof(OnPropertyChanged) && mi.IsPublic == false);

		public NativeEventWrapper(object target, string targetProperty, string updateSourceEventName)
		{
			TargetProperty = targetProperty;
			try {
				var updateSourceEvent = target.GetType().GetRuntimeEvent(updateSourceEventName);
				MethodInfo addMethod = updateSourceEvent.AddMethod;
				MethodInfo removeMethod = updateSourceEvent.RemoveMethod;
				ParameterInfo[] addParameters = addMethod.GetParameters();
				Type delegateType = addParameters[0].ParameterType;
				var handlerDelegate = s_handlerinfo.CreateDelegate(delegateType, this);
				Func<object, EventRegistrationToken> add = a => (EventRegistrationToken)addMethod.Invoke(target, new object[] { handlerDelegate });
				Action<EventRegistrationToken> remove = t => removeMethod.Invoke(target, new object[] { t });

				// TODO WINUI3
				//WindowsRuntimeMarshal.AddEventHandler(add, remove, s_handlerinfo);
			}
			catch (Exception) {
				Log.Warning(nameof(NativeEventWrapper), "Can not attach NativeEventWrapper.");
			}
		}

		void OnPropertyChanged(object sender, RoutedEventArgs e)
		{
			PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(TargetProperty));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
