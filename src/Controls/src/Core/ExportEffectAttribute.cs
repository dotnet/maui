using System;

namespace Microsoft.Maui.Controls
{
#pragma warning disable CS1734 // XML comment on 'ExportEffectAttribute' has a paramref tag for 'effectType', but there is no parameter by that name
	/// <include file="../../docs/Microsoft.Maui.Controls/ExportEffectAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.ExportEffectAttribute']/Docs" />
#pragma warning restore CS1734
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportEffectAttribute : Attribute
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ExportEffectAttribute.xml" path="//Member[@MemberName='.ctor']/Docs" />
		public ExportEffectAttribute(Type effectType, string uniqueName)
		{
			if (uniqueName.IndexOf(".", StringComparison.Ordinal) != -1)
				throw new ArgumentException("uniqueName must not contain a .");
			Type = effectType;
			Id = uniqueName;
		}

		internal string Id { get; private set; }

		internal Type Type { get; private set; }
	}
}