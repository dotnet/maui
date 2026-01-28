#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <summary>Specifies the resolution group name for effects in the assembly.</summary>
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class ResolutionGroupNameAttribute : Attribute
	{
		/// <summary>Creates a new <see cref="ResolutionGroupNameAttribute"/> with the specified name.</summary>
		public ResolutionGroupNameAttribute(string name)
		{
			ShortName = name;
		}

		internal string ShortName { get; private set; }
	}
}