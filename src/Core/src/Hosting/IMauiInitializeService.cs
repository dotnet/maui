using System;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Maui.Hosting
{
	public interface IMauiInitializeService
	{
		void Initialize(IServiceProvider services);
	}
}