using System;

namespace System.Maui.Platform.iOS
{
	public class IOSAppIndexingProvider : IAppIndexingProvider
	{
		public IAppLinks AppLinks => new IOSAppLinks();
	}
}