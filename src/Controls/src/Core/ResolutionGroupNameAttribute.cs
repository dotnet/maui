#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ResolutionGroupNameAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.ResolutionGroupNameAttribute']/Docs/*" />
	[AttributeUsage(AttributeTargets.Assembly)]
	public sealed class ResolutionGroupNameAttribute : Attribute
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ResolutionGroupNameAttribute.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ResolutionGroupNameAttribute(string name)
		{
			ShortName = name;
		}

		internal string ShortName { get; private set; }
	}
}