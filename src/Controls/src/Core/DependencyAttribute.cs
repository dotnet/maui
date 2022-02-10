using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/DependencyAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.DependencyAttribute']/Docs" />
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class DependencyAttribute : Attribute
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/DependencyAttribute.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public DependencyAttribute(Type implementorType)
		{
			Implementor = implementorType;
		}

		internal Type Implementor { get; private set; }
	}
}