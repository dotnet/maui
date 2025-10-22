using System;
using System.Diagnostics.CodeAnalysis;

namespace Samples.Model
{
	public class SampleItem
	{
		public SampleItem(string icon, string name, Type pageType, string description, params string[] tags)
		{
			Icon = icon;
			Name = name;
			Description = description;
			PageType = pageType;
			Tags = tags ?? Array.Empty<string>();
		}

		public string Icon { get; }

		public string Name { get; }

		public string Description { get; }

		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		public Type PageType { get; }

		public string[] Tags { get; }
	}
}
