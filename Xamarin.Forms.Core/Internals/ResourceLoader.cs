using System;
using System.Reflection;

namespace Xamarin.Forms.Internals
{
	public static class ResourceLoader
	{
		//takes a resource path, returns string content
		public static Func<AssemblyName, string, string> ResourceProvider { get; internal set; }
		internal static Action<Exception> ExceptionHandler { get; set; }
	}
}