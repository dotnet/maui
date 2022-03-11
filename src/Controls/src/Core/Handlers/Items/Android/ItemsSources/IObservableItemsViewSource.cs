namespace Microsoft.Maui.Controls.Handlers.Items
{
	public interface IObservableItemsViewSource : IItemsViewSource
	{
		bool ObserveChanges { get; set; }
	}
}
