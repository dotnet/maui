#nullable enable
using System;

namespace Microsoft.Maui
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public class MissingMapperAttribute : Attribute
	{
		public MissingMapperAttribute()
		{

		}

		public MissingMapperAttribute(string description)
		{
			Description = description;
		}

		public string? Description { get; set; }
	}
}