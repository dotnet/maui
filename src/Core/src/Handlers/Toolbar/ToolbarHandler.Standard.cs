using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, object>
	{
		protected override object CreatePlatformElement() => throw new NotImplementedException();

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
		}
	}
}