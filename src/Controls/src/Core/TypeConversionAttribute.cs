using System;

namespace Microsoft.Maui.Controls.Xaml
{
	[global::System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class TypeConversionAttribute : Attribute
	{
		public Type TargetType { get; private set; }

		public TypeConversionAttribute(Type targetType)
		{
			TargetType = targetType;
		}
	}
}