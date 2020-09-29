using System;
namespace Xamarin.Forms.Xaml.Diagnostics
{
	internal class StaticResourceResolvedEventArgs : EventArgs
	{
		internal StaticResourceResolvedEventArgs(ResourceDictionary resourceDictionary, string key, object targetObject, object targetProperty)
		{
			ResourceDictionary = resourceDictionary;
			Key = key;
			TargetObject = targetObject;
			TargetProperty = targetProperty;
		}

		public ResourceDictionary ResourceDictionary { get; }
		public string Key { get; }
		public object TargetObject { get; }
		public object TargetProperty { get; }
	}
}