using System;

namespace Xamarin.Forms.Xaml
{
	[AttributeUsage(AttributeTargets.Assembly, Inherited = false, AllowMultiple = true)]
	public sealed class XamlResourceIdAttribute : Attribute
	{
		public string ResourceId { get; set; }
		public string Path { get; set; }
		public Type Type { get; set; }

		public XamlResourceIdAttribute(string resourceId, string path, Type type)
		{
			ResourceId = resourceId;
			Path = path;
			Type = type;
		}
	}
}