using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	public static class NameScopeExtensions
	{
		public static T FindByName<T>(this Element element, string name)
		=> (T)element.FindByName(name);

		internal static T FindByName<T>(this INameScope namescope, string name)
			=> (T)namescope.FindByName(name);
	}
}