using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentViewHandler : ViewHandler<IContentView, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapContent(ContentViewHandler handler, IContentView page)
		{
		}
	}
}
