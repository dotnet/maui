using System;

#if __MOBILE__
using UIKit;
#endif

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportRendererAttribute : HandlerAttribute
	{
#if __MOBILE__
		public ExportRendererAttribute(Type handler, Type target, UIUserInterfaceIdiom idiom, Type[] supportedVisuals) : base(handler, target, supportedVisuals)
		{
			Idiomatic = true;
			Idiom = idiom;
		}


		public ExportRendererAttribute(Type handler, Type target, UIUserInterfaceIdiom idiom) : this(handler, target, idiom, null)
		{
		}

		internal UIUserInterfaceIdiom Idiom { get; }
#endif

		public ExportRendererAttribute(Type handler, Type target, Type[] supportedVisuals) : base(handler, target, supportedVisuals)
		{
			Idiomatic = false;
		}

		public ExportRendererAttribute(Type handler, Type target) : this(handler, target, null)
		{
		}

		internal bool Idiomatic { get; }

		public override bool ShouldRegister()
		{
#if __MOBILE__
			return !Idiomatic || Idiom == UIDevice.CurrentDevice.UserInterfaceIdiom;
#else
			return !Idiomatic;
#endif
		}
	}
}