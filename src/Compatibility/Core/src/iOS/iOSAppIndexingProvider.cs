using System;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	public class IOSAppIndexingProvider : IAppIndexingProvider
	{
		public IAppLinks AppLinks => new IOSAppLinks();
	}
}