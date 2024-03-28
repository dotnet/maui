#nullable enable
using System;

namespace Microsoft.Maui
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	public class PortTestAttribute : Attribute
	{
		public PortTestAttribute()
		{

		}

		public PortTestAttribute(string description)
		{
			Description = description;
		}

		public string? Description { get; set; }
	}
}