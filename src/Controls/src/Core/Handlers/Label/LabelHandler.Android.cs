namespace Microsoft.Maui.Controls.Handlers
{
	public partial class LabelHandler : Maui.Handlers.LabelHandler
	{
		public static void MapTextType(LabelHandler handler, Label label)
		{
			handler.NativeView?.UpdateText(label);
		}
	}
}
