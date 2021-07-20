using Microsoft.Extensions.DependencyInjection;
using System;

namespace Microsoft.Maui
{
	public partial class ViewExtensions
	{
		public static ISemanticService? GetSemanticService(this IElement element)
		{
			if (element.Handler == null)
				throw new ArgumentException($"Unable to find {nameof(ISemanticService)} for '{element.GetType().FullName}'.", nameof(element));

			return element?.Handler.MauiContext?.Services.GetService<ISemanticService>();
		}
	}
}
