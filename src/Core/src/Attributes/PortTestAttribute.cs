#nullable enable
using System;

namespace Microsoft.Maui
{
	// TODO: Consider making this public for .NET 9
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	internal class PortTestAttribute : Attribute
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