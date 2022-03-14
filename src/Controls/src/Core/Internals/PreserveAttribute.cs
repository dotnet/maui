using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls.Internals
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/PreserveAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.Internals.PreserveAttribute']/Docs" />
	[AttributeUsage(AttributeTargets.All)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public sealed class PreserveAttribute : Attribute
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/PreserveAttribute.xml" path="//Member[@MemberName='AllMembers']/Docs" />
		public bool AllMembers;
		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/PreserveAttribute.xml" path="//Member[@MemberName='Conditional']/Docs" />
		public bool Conditional;

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/PreserveAttribute.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public PreserveAttribute(bool allMembers, bool conditional)
		{
			AllMembers = allMembers;
			Conditional = conditional;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Internals/PreserveAttribute.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public PreserveAttribute()
		{
		}
	}
}