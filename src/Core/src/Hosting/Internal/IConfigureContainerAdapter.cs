using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui.Hosting.Internal
{
	interface IConfigureContainerAdapter
	{
		void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder);
	}
}