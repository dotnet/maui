#nullable disable
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Controls.StyleSheets
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
	sealed class StylePropertyAttribute : Attribute
	{
		public string CssPropertyName { get; }
		public string BindablePropertyName { get; }
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)]
		public Type TargetType { get; }
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)]
		public Type PropertyOwnerType { get; set; }
		public BindableProperty BindableProperty { get; set; }
		public bool Inherited { get; set; } = false;


		public StylePropertyAttribute(
			string cssPropertyName,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.NonPublicFields)]
			Type targetType,
			string bindablePropertyName)
		{
			CssPropertyName = cssPropertyName;
			BindablePropertyName = bindablePropertyName;
			TargetType = targetType;
		}
	}
}