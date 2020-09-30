using System;

namespace Xamarin.Forms.Platform.iOS
{
	public class IOSAppIndexingProvider : IAppIndexingProvider
	{
		public IAppLinks AppLinks => new IOSAppLinks();
	}
}