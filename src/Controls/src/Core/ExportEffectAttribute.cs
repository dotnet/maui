using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/ExportEffectAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.ExportEffectAttribute']/Docs" />
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportEffectAttribute : Attribute
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ExportEffectAttribute.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ExportEffectAttribute(Type effectType, string uniqueName)
		{
			if (uniqueName.Contains("."))
				throw new ArgumentException("uniqueName must not contain a .");
			Type = effectType;
			Id = uniqueName;
		}

		internal string Id { get; private set; }

		internal Type Type { get; private set; }
	}
}