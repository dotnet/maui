using System;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui.Hosting.Internal
{
	internal class ConfigureContainerAdapter<TContainerBuilder> : IConfigureContainerAdapter
	{
		readonly Action<HostBuilderContext, TContainerBuilder> _action;

		public ConfigureContainerAdapter(Action<HostBuilderContext, TContainerBuilder> action) =>
			_action = action ?? throw new ArgumentNullException(nameof(action));

		public void ConfigureContainer(HostBuilderContext hostContext, object containerBuilder) =>
			_action(hostContext, (TContainerBuilder)containerBuilder);

	}
}
