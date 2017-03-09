using System;
using System.ComponentModel;

namespace Xamarin.Forms.Internals
{
	[AttributeUsage(AttributeTargets.All)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public class PreserveAttribute : Attribute
	{
		public bool AllMembers;
		public bool Conditional;

		public PreserveAttribute(bool allMembers, bool conditional)
		{
			AllMembers = allMembers;
			Conditional = conditional;
		}

		public PreserveAttribute()
		{
		}
	}
}