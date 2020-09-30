using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	internal interface IElement
	{
		Element Parent { get; set; }

		//Use these 2 instead of an event to avoid cloning way too much multicastdelegates on mono
		void AddResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged);
		void RemoveResourcesChangedListener(Action<object, ResourcesChangedEventArgs> onchanged);
	}
}