#nullable disable
using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <summary>Extension methods for <see cref="Microsoft.Maui.Controls.Element"/>  that adds a strongly-typed FindByName method.</summary>
	public static class NameScopeExtensions
	{
		public static T FindByName<T>(this Element element, string name)
		{
			if (!RuntimeFeature.AreNamescopesSupported)
				throw new NotSupportedException("Namescopes are not supported. Please enable the feature switch 'Microsoft.Maui.RuntimeFeature.AreNamescopesSupported' to keep using namescopes.");

			try
			{
				return (T)element.FindByName(name);
			}
			catch (InvalidCastException ice) when (ResourceLoader.ExceptionHandler2 != null)
			{
				ResourceLoader.ExceptionHandler2((ice, null));
				return default(T);
			}
		}

		internal static T FindByName<T>(this INameScope namescope, string name)
		{
			if (!RuntimeFeature.AreNamescopesSupported)
				throw new NotSupportedException("Namescopes are not supported. Please enable the feature switch 'Microsoft.Maui.RuntimeFeature.AreNamescopesSupported' to keep using namescopes.");

			return (T)namescope.FindByName(name);
		}
	}
}
