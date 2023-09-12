using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentViewHandler : ViewHandler<IContentView, object>
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static partial void MapContent(IContentViewHandler handler, IContentView page)
		{
		}
	}
}
