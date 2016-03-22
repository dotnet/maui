using System;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class RenderWithAttribute : Attribute
	{
		public RenderWithAttribute(Type type)
		{
			Type = type;
		}

		public Type Type { get; }
	}
}