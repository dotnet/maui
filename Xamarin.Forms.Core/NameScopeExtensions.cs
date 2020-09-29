using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public static class NameScopeExtensions
	{
		public static T FindByName<T>(this Element element, string name)
		{
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
			=> (T)namescope.FindByName(name);
	}
}