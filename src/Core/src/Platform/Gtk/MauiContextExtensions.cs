using System.Collections.Generic;
using Gtk;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Platform;

internal static partial class MauiContextExtensions
{

	public static Window GetPlatformWindow(this IMauiContext mauiContext) =>
		mauiContext.Services.GetRequiredService<Window>();

	public static IToolbarContainer? GetToolBarContainer(this IMauiContext mauiContext)
	{
		var queue = new Queue<Widget>(mauiContext.GetPlatformWindow().Children);
		while (queue.Count != 0)
		{
			var curr = queue.Dequeue();
			if (curr is IToolbarContainer result)
			{
				return result;
			}

			if (curr is not Container container)
				continue;

			foreach (var child in container.Children)
			{
				queue.Enqueue(child);
			}
		}

		return null;
	}


}