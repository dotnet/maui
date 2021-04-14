using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ScrollViewHandler : ViewHandler<IScroll, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapContent(IViewHandler handler, IScroll scrollView) { }
	}
}