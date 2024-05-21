#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls
{
#pragma warning disable CS1734 // XML comment on 'ExportEffectAttribute' has a paramref tag for 'effectType', but there is no parameter by that name
	/// <include file="../../docs/Microsoft.Maui.Controls/ExportEffectAttribute.xml" path="Type[@FullName='Microsoft.Maui.Controls.ExportEffectAttribute']/Docs/*" />
#pragma warning restore CS1734
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportEffectAttribute : Attribute
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/ExportEffectAttribute.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public ExportEffectAttribute(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)] Type effectType,
			string uniqueName)
		{
			if (uniqueName.IndexOf(".", StringComparison.Ordinal) != -1)
				throw new ArgumentException("uniqueName must not contain a .");
			Type = effectType;
			Id = uniqueName;
		}

		internal string Id { get; private set; }

		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
		internal Type Type { get; private set; }
	}
}