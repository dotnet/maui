using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/NameScopeExtensions.xml" path="Type[@FullName='Microsoft.Maui.Controls.NameScopeExtensions']/Docs/*" />
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
