#nullable disable
using System;
using System.ComponentModel;
using System.Reflection;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
	/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static class ResourceLoader
	{
		static Func<ResourceLoadingQuery, ResourceLoadingResponse> _resourceProvider2;
		/// <summary>Internal API for Microsoft.Maui.Controls platform use.</summary>
		/// <remarks>For internal use only. This API can be changed or removed without notice at any time.</remarks>
		public static Func<ResourceLoadingQuery, ResourceLoadingResponse> ResourceProvider2
		{
			get => _resourceProvider2;
			internal set
			{
				DesignMode.IsDesignModeEnabled = value != null;
				_resourceProvider2 = value;
			}
		}

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

		internal static Action<(Exception exception, string filepath)> ExceptionHandler2 { get; set; }
	}
}
