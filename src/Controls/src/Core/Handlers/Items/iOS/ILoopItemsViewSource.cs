namespace Microsoft.Maui.Controls.Handlers.Items
{
	public interface ILoopItemsViewSource : IItemsViewSource
	{
		bool Loop { get; set; }

		int LoopCount { get; }
	}
}