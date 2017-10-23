using System;

namespace Xamarin.Forms.Xaml
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class XamlResourceIdAttribute : Attribute
	{
		public string RootNamespace { get; set; }
		public string ResourceId { get; set; }

		public XamlResourceIdAttribute(string rootNamespace, string resourceId)
		{
			RootNamespace = rootNamespace;
			ResourceId = resourceId;
		}
	}
}