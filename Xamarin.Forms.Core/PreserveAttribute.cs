using System;

namespace Xamarin.Forms
{
	[AttributeUsage(
		AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field |
		AttributeTargets.Event | AttributeTargets.Interface | AttributeTargets.Delegate | AttributeTargets.All)]
	internal class PreserveAttribute : Attribute
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