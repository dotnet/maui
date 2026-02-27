using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;

namespace Microsoft.Maui.Hosting;

internal class MauiMetricsBuilder(IServiceCollection services) : IMetricsBuilder
{
	readonly IServiceCollection _services = services;

	public IServiceCollection Services => _services;
}
