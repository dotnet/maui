#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ControlTemplate.xml" path="Type[@FullName='Microsoft.Maui.Controls.ControlTemplate']/Docs/*" />
	public class ControlTemplate : ElementTemplate
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ControlTemplate.xml" path="//Member[@MemberName='.ctor'][1]/Docs/*" />
		public ControlTemplate()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ControlTemplate.xml" path="//Member[@MemberName='.ctor'][3]/Docs/*" />
		public ControlTemplate(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type type)
			: base(type)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ControlTemplate.xml" path="//Member[@MemberName='.ctor'][2]/Docs/*" />
		public ControlTemplate(Func<object> createTemplate) : base(createTemplate)
		{
		}
	}
}