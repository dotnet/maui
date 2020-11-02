using Xamarin.Platform.Layouts;

namespace Xamarin.Platform
{
	public class VerticalStackLayout : StackLayout
	{
		protected override ILayoutManager CreateLayoutManager() => new VerticalStackLayoutManager(this);
	}
}
