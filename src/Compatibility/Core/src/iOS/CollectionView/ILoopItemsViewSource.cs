namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
    public interface ILoopItemsViewSource : IItemsViewSource
    {
        bool Loop { get; set; }

        int LoopCount { get; }
    }
}