namespace Microsoft.Maui.Controls
{
	public partial class Frame : IFrame
	{
		IView IFrame.Content => Content;
	}
}