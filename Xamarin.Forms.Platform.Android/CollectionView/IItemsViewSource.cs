namespace Xamarin.Forms.Platform.Android
{
	internal interface IItemsViewSource
	{
		int Count { get; }
		object this[int index] { get; }
	}
}