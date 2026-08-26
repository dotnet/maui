using System.Diagnostics;

namespace Microsoft.Maui.Diagnostics;

internal static class HandlerInstrumentation
{
	internal const string ActivitySourceName = "Microsoft.Maui.Handlers";

	static readonly ActivitySource s_activitySource = new(ActivitySourceName);

	public static bool HasListeners => s_activitySource.HasListeners();

	public static Activity? Start(string name, IElementHandler handler, IElement element, string? property = null)
	{
		if (!HasListeners)
		{
			return null;
		}

		var activity = s_activitySource.StartActivity(name, ActivityKind.Internal);
		if (activity is not null)
		{
			activity.SetTag("element.type", element.GetType().FullName);
			activity.SetTag("handler.type", handler.GetType().FullName);

			if (property is not null)
			{
				activity.SetTag("mapper.property", property);
			}
		}

		return activity;
	}
}
