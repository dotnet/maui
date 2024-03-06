namespace Microsoft.Maui.Controls.Handlers.Items.Platform;

public class RealizedItem
{

	public RealizedItem(ViewHolder holder, int index)
	{
		Holder = holder;
		Index = index;
	}

	public ViewHolder Holder { get; set; }

	public int Index { get; set; }

}