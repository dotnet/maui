#nullable disable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <summary>For internal use by platform renderers.</summary>
	[AttributeUsage(AttributeTargets.All)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class PreserveAttribute : Attribute
	{
		/// <summary>For internal use by platform renderers.</summary>
		public bool AllMembers;
		/// <summary>For internal use by platform renderers.</summary>
		public bool Conditional;

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/PreserveAttribute.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public PreserveAttribute(bool allMembers, bool conditional)
		{
			AllMembers = allMembers;
			Conditional = conditional;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/PreserveAttribute.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public PreserveAttribute()
		{
		}
	}
}