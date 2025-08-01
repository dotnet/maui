using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Performance
{
    /// <summary>
    /// Extension methods to register and configure performance monitoring services for a MAUI application.
    /// </summary>
    internal static class PerformanceMauiAppBuilderExtensions
    {
        /// <summary>
        /// Adds .NET MAUI performance monitoring services to the application's dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="MauiAppBuilder"/> to which performance monitoring is being added.</param>
        /// <returns>The same <see cref="MauiAppBuilder"/> instance.</returns>
        public static MauiAppBuilder ConfigurePerformance(
			this MauiAppBuilder builder)
        {
            // Check if the performance profiling feature is enabled.
            if (RuntimeFeature.IsMetricsSupported)
            {
	            // Register the Meter wrapper
	            builder.Services.AddSingleton<MauiPerformanceMeter>();

	            // Register core services
	            builder.Services.AddSingleton<ILayoutPerformanceTracker, LayoutPerformanceTracker>();

	            // Register initializer service to set up PerformanceProfiler
	            builder.Services.AddTransient<IMauiInitializeService, PerformanceProfilerInitializer>();
            }

            return builder;
        }
        
        /// <summary>
        /// Service to initialize PerformanceProfiler at application startup.
        /// </summary>
        internal class PerformanceProfilerInitializer : IMauiInitializeService
        {
	        private readonly ILayoutPerformanceTracker _layoutTracker;

	        public PerformanceProfilerInitializer(ILayoutPerformanceTracker layoutTracker)
	        {
		        _layoutTracker = layoutTracker ?? throw new ArgumentNullException(nameof(layoutTracker));
	        }

	        public void Initialize(IServiceProvider services)
	        {
		        PerformanceProfiler.Initialize(_layoutTracker);
	        }
        }
    }
}