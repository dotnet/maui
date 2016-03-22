using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public static class NameScopeExtensions
	{
		public static T FindByName<T>(this Element element, string name)
		{
			return ((INameScope)element).FindByName<T>(name);
		}

		internal static T FindByName<T>(this INameScope namescope, string name)
		{
			return (T)namescope.FindByName(name);
		}
	}
}