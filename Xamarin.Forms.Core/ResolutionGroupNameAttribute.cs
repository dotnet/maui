using System;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class ResolutionGroupNameAttribute : Attribute
	{
		public ResolutionGroupNameAttribute(string name)
		{
			ShortName = name;
		}

		internal string ShortName { get; private set; }
	}
}