using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui.Hosting.Internal
{
	internal interface IConfigureContainerAdapter
	{
		void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder);
	}
}
