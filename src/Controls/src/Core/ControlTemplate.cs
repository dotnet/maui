using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ControlTemplate.xml" path="Type[@FullName='Microsoft.Maui.Controls.ControlTemplate']/Docs" />
	public class ControlTemplate : ElementTemplate
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ControlTemplate.xml" path="//Member[@MemberName='.ctor'][0]/Docs" />
		public ControlTemplate()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ControlTemplate.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public ControlTemplate(Type type) : base(type)
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/ControlTemplate.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ControlTemplate(Func<object> createTemplate) : base(createTemplate)
		{
		}
	}
}