namespace Microsoft.Maui.Controls.Handlers
{
	public partial class BoxViewHandler : ShapeViewHandler
	{
		public override bool NeedsContainer => 
			VirtualView?.Clip != null ||
			VirtualView?.Shadow != null;
	}
}