using System;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public abstract class HandlerAttribute : Attribute
	{
		protected HandlerAttribute(Type handler, Type target, Type[] supportedVisuals = null)
		{
			SupportedVisuals = supportedVisuals ?? new[] { typeof(VisualRendererMarker.Default) };
			TargetType = target;
			HandlerType = handler;
		}

		internal Type[] SupportedVisuals { get; private set; }
		internal Type HandlerType { get; private set; }

		internal Type TargetType { get; private set; }

		public virtual bool ShouldRegister()
		{
			return true;
		}
	}
}