// This is a temporary namespace until we rename everything and move the legacy layouts
namespace Microsoft.Maui.Controls.Layout2
{
	public abstract class StackLayout : Layout, IStackLayout
	{
		public int Spacing { get; set; }
	}
}
