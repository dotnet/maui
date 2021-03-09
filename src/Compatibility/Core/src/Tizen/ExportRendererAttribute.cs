using System;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;

namespace Microsoft.Maui.Controls.Compatibility
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportRendererAttribute : HandlerAttribute
	{
		TargetIdiom Target { get; set; }

		public ExportRendererAttribute(Type handler, Type target) : this(handler, target, null)
		{
		}

		public ExportRendererAttribute(Type handler, Type target, Type[] supportedVisuals) : base(handler, target, supportedVisuals)
		{
		}

		public ExportRendererAttribute(Type handler, Type target, TargetIdiom targetIdiom) : this(handler, target, null, targetIdiom)
		{
		}

		public ExportRendererAttribute(Type handler, Type target, Type[] supportedVisuals, TargetIdiom targetIdiom) : base(handler, target, supportedVisuals)
		{
			Target = targetIdiom;
		}

		public override bool ShouldRegister()
		{
			if (Target == TargetIdiom.Unsupported)
				return true;

			return (Target == Device.Idiom);
		}
	}
}
