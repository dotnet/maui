using System;

namespace Microsoft.Maui.Handlers
{
	public partial class BorderHandler : ViewHandler<IBorder, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapContent(BorderHandler handler, IBorder border)
		{
		}
	}
}
