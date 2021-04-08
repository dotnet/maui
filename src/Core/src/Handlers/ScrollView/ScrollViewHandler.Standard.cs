using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScroll, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapContentSize(IViewHandler handler, IScroll scrollView) { }
	}
}