using System;
using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Performance
{
    /// <summary>
    /// Extension methods to register and configure performance monitoring services for a MAUI application.
    /// </summary>
    public static class PerformanceMonitoringExtensions
    {
        /// <summary>
        /// Adds .NET MAUI performance monitoring services to the application's dependency injection container.
        /// </summary>
        /// <param name="builder">The <see cref="MauiAppBuilder"/> to which performance monitoring is being added.</param>
        /// <param name="configure">
        /// An optional callback to configure <see cref="PerformanceMonitoringOptions"/> (e.g., enabling layout tracking).
        /// If null, default options are applied.
        /// </param>
        /// <returns>The same <see cref="MauiAppBuilder"/> instance, allowing for fluent chaining.</returns>
        [RequiresPerformanceMonitoringMethod]
        public static MauiAppBuilder AddPerformanceMonitoring(
            this MauiAppBuilder builder,
            Action<PerformanceMonitoringOptions>? configure = null)
        {
            var options = new PerformanceMonitoringOptions();
            configure?.Invoke(options);
            
            // Register the Meter
            var meter = new Meter("Microsoft.Maui");
            builder.Services.AddSingleton(meter);
            
            // Register core services
            builder.Services.AddSingleton<IPerformanceProfiler, PerformanceProfiler>();
            builder.Services.AddSingleton<IImagePerformanceTracker, ImagePerformanceTracker>();
            builder.Services.AddSingleton<ILayoutPerformanceTracker, LayoutPerformanceTracker>();
            builder.Services.AddSingleton<INavigationPerformanceTracker, NavigationPerformanceTracker>();
            
            // Register warning manager with configuration
            builder.Services.AddSingleton<IPerformanceWarningManager>(_ =>
            {
	            var warningManager = new PerformanceWarningManager();
                
	            // Configure warnings
	            warningManager.Configure(options.Warnings);
                
	            return warningManager;
            });
            
            // Configure options
            builder.Services.Configure<PerformanceMonitoringOptions>(opt =>
            {
				opt.Image = options.Image;
                opt.Layout = options.Layout;
                opt.Navigation = options.Navigation;
                opt.Warnings = options.Warnings;
            });

            return builder;
        }

        /// <summary>
        /// Configures image tracking options for performance monitoring.
        /// </summary>
        /// <param name="options">The performance monitoring options instance to modify.</param>
        /// <param name="configure">A delegate that applies configuration settings to image tracking.</param>
        /// <returns>The modified <see cref="PerformanceMonitoringOptions"/> instance.</returns>
        /// <remarks>
        /// This extension method allows customization of image tracking settings by applying user-defined configurations.
        /// It enables fine-tuned performance monitoring based on application needs.
        /// </remarks>
        [RequiresPerformanceMonitoringMethod]
        public static PerformanceMonitoringOptions ConfigureImage(
	        this PerformanceMonitoringOptions options,
	        Action<ImageTrackingOptions> configure)
        {
	        configure(options.Image);
	        return options;
        }  
        
        /// <summary>
        /// Configures layout‐tracking options within the <see cref="PerformanceMonitoringOptions"/>.
        /// </summary>
        /// <param name="options">The <see cref="PerformanceMonitoringOptions"/> instance to configure.</param>
        /// <param name="configure">
        /// A callback to configure the <see cref="LayoutTrackingOptions"/>, 
        /// such as enabling measure/arrange pass tracking or per‐element tags.
        /// </param>
        /// <returns>The same <see cref="PerformanceMonitoringOptions"/> instance, enabling fluent configuration chaining.</returns>
        [RequiresPerformanceMonitoringMethod]
        public static PerformanceMonitoringOptions ConfigureLayout(
            this PerformanceMonitoringOptions options,
            Action<LayoutTrackingOptions> configure)
        {
            configure(options.Layout);
            return options;
        }    
        
        /// <summary>
        /// Configures navigation-tracking options within the <see cref="PerformanceMonitoringOptions"/>.
        /// </summary>
        /// <param name="options">The <see cref="PerformanceMonitoringOptions"/> instance to configure.</param>
        /// <param name="configure">
        /// A callback to configure the <see cref="NavigationTrackingOptions"/>, 
        /// such as enabling timing thresholds or subscription behavior for navigation events.
        /// </param>
        /// <returns>The same <see cref="PerformanceMonitoringOptions"/> instance, enabling fluent configuration chaining.</returns>
        [RequiresPerformanceMonitoringMethod]
        public static PerformanceMonitoringOptions ConfigureNavigation(
	        this PerformanceMonitoringOptions options,
	        Action<NavigationTrackingOptions> configure)
        {
	        configure(options.Navigation);
	        return options;
        }  
        
        /// <summary>
        /// Configures performance warning settings by applying custom options.
        /// Allows modification of warning thresholds and severity levels.
        /// </summary>
        /// <param name="options">The <see cref="PerformanceMonitoringOptions"/> instance to configure.</param>
        /// <param name="configure">An action to modify <see cref="WarningOptions"/>.</param>
        /// <returns>The updated <see cref="PerformanceMonitoringOptions"/> instance.</returns>
        [RequiresPerformanceMonitoringMethod]
        public static PerformanceMonitoringOptions ConfigureWarnings(
	        this PerformanceMonitoringOptions options,
	        Action<WarningOptions> configure)
        {
	        configure?.Invoke(options.Warnings);
	        return options;
        }
    }
}
