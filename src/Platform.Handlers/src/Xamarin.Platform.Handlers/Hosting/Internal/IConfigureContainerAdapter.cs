using Microsoft.Extensions.Hosting;

namespace Xamarin.Platform.Hosting.Internal
{
	internal interface IConfigureContainerAdapter
	{
		void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder);
	}
}
