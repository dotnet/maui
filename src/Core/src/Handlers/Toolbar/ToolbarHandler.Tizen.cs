using ElmSharp;

namespace Microsoft.Maui.Handlers
{
	public partial class ToolbarHandler : ElementHandler<IToolbar, Toolbar>
	{
		protected override Toolbar CreatePlatformElement()
		{
			throw new System.NotImplementedException();
		}

		public static void MapTitle(IToolbarHandler arg1, IToolbar arg2)
		{
		}
	}
}
