using System;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public class DependencyAttribute : Attribute
	{
		public DependencyAttribute(Type implementorType)
		{
			Implementor = implementorType;
		}

		internal Type Implementor { get; private set; }
	}
}