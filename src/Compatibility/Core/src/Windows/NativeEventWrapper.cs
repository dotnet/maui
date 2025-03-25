using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Internals;
using Microsoft.UI.Xaml;
using WinRT;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	class NativeEventWrapper : INotifyPropertyChanged
	{
		static string TargetProperty { get; set; }
		static readonly MethodInfo s_handlerinfo = typeof(NativeEventWrapper).GetRuntimeMethods().Single(mi => mi.Name == nameof(OnPropertyChanged) && mi.IsPublic == false);

		public NativeEventWrapper(object target, string targetProperty, string updateSourceEventName)
		{
			TargetProperty = targetProperty;
			try
			{

#pragma warning disable IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
				var updateSourceEvent = target.GetType().GetRuntimeEvent(updateSourceEventName);
#pragma warning restore IL2072 // Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.
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
			catch (Exception)
			{
				Application.Current?.FindMauiContext()?.CreateLogger<NativeEventWrapper>()?.LogWarning("Cannot attach NativeEventWrapper.");
			}
		}

		void OnPropertyChanged(object sender, RoutedEventArgs e)
		{
			PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(TargetProperty));
		}

		public event PropertyChangedEventHandler PropertyChanged;
	}
}
