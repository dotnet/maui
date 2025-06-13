#nullable  disable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.Controls.Performance;

/// <summary>
/// Controls whether performance monitoring features are enabled.
/// This class is used by the trimmer to remove performance monitoring code in production builds.
/// </summary>
internal static class PerformanceProfilerFeature
{
    /// <summary>
    /// Feature switch that controls performance monitoring availability.
    /// When false, all performance monitoring code will be trimmed from the final binary.
    /// </summary>
    public const string FeatureSwitchName = "Microsoft.Maui.Controls.Performance.EnableMonitoring";

    /// <summary>
    /// Indicates whether performance monitoring is enabled.
    /// This property is substituted by the linker based on the feature switch value.
    /// </summary>
    [FeatureSwitchDefinition(FeatureSwitchName)]
    public static bool IsEnabled => AppContext.TryGetSwitch(FeatureSwitchName, out bool isEnabled) && isEnabled;

    /// <summary>
    /// Guards execution of performance monitoring code.
    /// When IsEnabled is false, the linker will remove the guarded code entirely.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool Guard()
    {
        return IsEnabled;
    }

    /// <summary>
    /// Conditionally executes an action only when performance monitoring is enabled.
    /// The action and its closure will be trimmed when IsEnabled is false.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Execute(Action action)
    {
        if (IsEnabled)
        {
            action();
        }
    }

    /// <summary>
    /// Conditionally executes a function and returns its result when performance monitoring is enabled.
    /// Returns the default value when disabled. The function will be trimmed when IsEnabled is false.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T Execute<T>(Func<T> function, T defaultValue = default(T))
    {
        if (IsEnabled)
        {
            return function();
        }
        
        return defaultValue;
    }
}

/// <summary>
/// Attribute to mark types that should be trimmed when performance monitoring is disabled.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Struct)]
internal sealed class RequiresPerformanceMonitoringAttribute : Attribute
{
	public RequiresPerformanceMonitoringAttribute(string message = null)
	{
		Message = message;
	}

	public string Message { get; }
}

/// <summary>
/// Attribute to mark methods that should be trimmed when performance monitoring is disabled.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor)]
internal sealed class RequiresPerformanceMonitoringMethodAttribute : Attribute
{
	public RequiresPerformanceMonitoringMethodAttribute(string message = null)
	{
		Message = message;
	}

	public string Message { get; }
}


// FeatureSwitchDefinitionAttribute is not available for older target 
// frameworks like .NET Standard 2.0/2.1 and .NET Framework, we need to 
// define this attribute ourselves.
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETFRAMEWORK
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
internal sealed class FeatureSwitchDefinitionAttribute : Attribute
{
    public FeatureSwitchDefinitionAttribute(string switchName)
    {
        SwitchName = switchName;
    }

    public string SwitchName { get; }
}
#endif