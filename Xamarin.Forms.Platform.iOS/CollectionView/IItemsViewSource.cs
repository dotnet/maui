namespace Xamarin.Forms.Platform.iOS
{
	internal interface IItemsViewSource
	{
		int Count { get; }
		object this[int index] { get; }
	}
}