using System;

namespace Microsoft.Maui.Handlers
{
	public partial class PageHandler : ViewHandler<IPage, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapTitle(PageHandler handler, IPage page)
		{
		}
		public static void MapContent(PageHandler handler, IPage page)
		{
		}
	}
}
