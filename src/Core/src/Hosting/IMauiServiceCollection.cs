using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Hosting
{
	public interface IMauiServiceCollection
		: IServiceCollection, IDictionary<Type, Func<IServiceProvider, object?>?>
	{
	}
}
