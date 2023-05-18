#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/VisualAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.VisualAttribute']/Docs/*" />
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class VisualAttribute : Attribute
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/VisualAttribute.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public VisualAttribute(
			string key,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type visual)
		{
			this.Key = key;
			this.Visual = visual;
		}

		internal string Key { get; }

		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		internal Type Visual { get; }
	}
}
