using System;

namespace Xamarin.Forms.Xaml
{
	[System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
	public sealed class TypeConversionAttribute : Attribute
	{
		public Type TargetType { get; private set; }

		public TypeConversionAttribute(Type targetType)
		{
			TargetType = targetType;
		}
	}
}