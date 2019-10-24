using System;
namespace Xamarin.Forms.Internals
{
	public static class ResourceLoader
	{
		public static Func<string, string> ResourceProvider { get; internal set; }
		internal static Action<Exception> ExceptionHandler { get; set; }
	}
}