using System;

namespace Xamarin.Forms.Xaml
{
	[AttributeUsage(AttributeTargets.Class)]
	internal sealed class RuntimeNamePropertyAttribute : Attribute
	{
		public RuntimeNamePropertyAttribute(string name)
		{
			Name = name;
		}

		public string Name { get; }
	}
}