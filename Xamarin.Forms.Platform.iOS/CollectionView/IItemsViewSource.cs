using System;

namespace Xamarin.Forms.Platform.iOS
{
	internal interface IItemsViewSource : IDisposable
	{
		int Count { get; }
		object this[int index] { get; }
	}
}