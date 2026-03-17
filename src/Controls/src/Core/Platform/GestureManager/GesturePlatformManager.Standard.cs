using System;

namespace Microsoft.Maui.Controls.Platform
{
	class GesturePlatformManager : IGesturePlatformManager
	{
		public GesturePlatformManager(IViewHandler handler)
		{
		}

		public void SetupHandler(IViewHandler handler)
		{
			// Handler is already set via constructor on locally-created instances
		}

		public void Dispose()
		{
		}
	}
}
