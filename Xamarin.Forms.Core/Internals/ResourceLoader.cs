using System;
using System.ComponentModel;
using System.Reflection;

namespace Xamarin.Forms.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class ResourceLoader
	{
		static Func<AssemblyName, string, string> resourceProvider;

		[Obsolete("You shouldn't have used this one to begin with, don't use the other one either")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		//takes a resource path, returns string content
		public static Func<AssemblyName, string, string> ResourceProvider
		{
			get => resourceProvider;
			internal set
			{
				resourceProvider = value;
				if (value != null)
					ResourceProvider2 = rlq => new ResourceLoadingResponse { ResourceContent = value(rlq.AssemblyName, rlq.ResourcePath) };
				else
					ResourceProvider2 = null;
			}
		}

		static Func<ResourceLoadingQuery, ResourceLoadingResponse> _resourceProvider2;
		public static Func<ResourceLoadingQuery, ResourceLoadingResponse> ResourceProvider2
		{
			get => _resourceProvider2;
			internal set
			{
				DesignMode.IsDesignModeEnabled = value != null;
				_resourceProvider2 = value;
			}
		}

		[Obsolete("Can't touch this")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsEnabled => _resourceProvider2 != null;

		[Obsolete("Can't touch this")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool CanProvideContentFor(ResourceLoadingQuery rlq) => _resourceProvider2?.Invoke(rlq) != null;

		public class ResourceLoadingQuery
		{
			public AssemblyName AssemblyName { get; set; }
			public string ResourcePath { get; set; }
			public object Instance { get; set; }
		}

		public class ResourceLoadingResponse
		{
			public string ResourceContent { get; set; }
			public bool UseDesignProperties { get; set; }
		}

		static Action<Exception> exceptionHandler1;

		[Obsolete("2 is better than 1")]
		internal static Action<Exception> ExceptionHandler
		{
			get => exceptionHandler1;
			set
			{
				exceptionHandler1 = value;
				ExceptionHandler2 = value != null ? ((Exception exception, string filepath) err) => exceptionHandler1(err.exception) : (Action<(Exception, string)>)null;
			}
		}

		internal static Action<(Exception exception, string filepath)> ExceptionHandler2 { get; set; }
	}
}