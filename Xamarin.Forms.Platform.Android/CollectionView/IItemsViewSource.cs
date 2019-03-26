using System;

namespace Xamarin.Forms.Platform.Android
{
	internal interface IItemsViewSource : IDisposable
	{
		int Count { get; }
		object this[int index] { get; }
	}
}