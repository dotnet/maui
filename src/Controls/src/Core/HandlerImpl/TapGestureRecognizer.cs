namespace Microsoft.Maui.Controls
{
	public partial class TapGestureRecognizer : ITapGestureRecognizer
	{
		void ITapGestureRecognizer.Tapped(IView view) =>
			SendTapped((View)view);
	}
}
