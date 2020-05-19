using System;

namespace System.Maui
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class ResolutionGroupNameAttribute : Attribute
	{
		public ResolutionGroupNameAttribute(string name)
		{
			ShortName = name;
		}

		internal string ShortName { get; private set; }
	}
}