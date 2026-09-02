#if HANDLER_INSTRUMENTATION
using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

/// <summary>
/// Compile-time opt-in <see cref="ActivitySource"/> for handler lifecycle and mapper attribution
/// (<c>ElementHandler.SetVirtualView</c>, <c>PropertyMapper.UpdateProperties</c>/<c>UpdateProperty</c>).
/// </summary>
/// <remarks>
/// This whole file only exists when <c>HANDLER_INSTRUMENTATION</c> is defined (via the
/// <c>MauiEnableHandlerInstrumentation</c> MSBuild property), so neither this type nor its
/// <see cref="ActivitySource"/>/<see cref="System.Diagnostics.DiagnosticSource"/> dependency are compiled
/// into a default build.
///
/// It intentionally keeps its own static <see cref="ActivitySource"/> instance rather than resolving the
/// shared, DI-registered <see cref="IDiagnosticsManager"/> used by layout diagnostics
/// (<c>DiagnosticInstrumentation</c>): <c>SetVirtualView</c> and property updates are invoked for every
/// handler/property, so the check has to stay a single static-field read (<see cref="HasListeners"/>)
/// with no service-provider lookup or <c>RuntimeFeature</c> gate on the hot path. It shares the same
/// <see cref="DiagnosticsIdentity.Namespace"/>/<see cref="DiagnosticsIdentity.Version"/> as
/// <see cref="DiagnosticsManager"/> so a single <see cref="ActivityListener"/> subscription (matched by
/// source name) observes both handler and layout spans, instead of requiring two subscriptions.
/// </remarks>
internal static class HandlerInstrumentation
{
	static readonly ActivitySource s_activitySource = new(DiagnosticsIdentity.Namespace, DiagnosticsIdentity.Version);

	public static bool HasListeners => s_activitySource.HasListeners();

	public static Activity? Start(string name, IElementHandler? handler, IElement? element, string? property = null)
	{
		if (!HasListeners)
		{
			return null;
		}

		var activity = s_activitySource.StartActivity(name, ActivityKind.Internal);
		if (activity is not null)
		{
			// Handler/element are nullable here (and in practice can be null, e.g. some PropertyMapper
			// call sites/tests pass a null handler) - never let instrumentation throw for callers that
			// tolerate that, so every tag lookup is null-safe.
			activity.SetTag("element.type", element?.GetType().FullName);
			activity.SetTag("handler.type", handler?.GetType().FullName);

			if (property is not null)
			{
				activity.SetTag("mapper.property", property);
			}
		}

		return activity;
	}
}
#endif
