using System;
#if __UNIFIED__
using UIKit;
#else
using MonoTouch.UIKit;
#endif

namespace Xamarin.Forms
{
	[AttributeUsage (AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportRendererAttribute : HandlerAttribute {
		internal bool Idiomatic { get; private set; }
		internal UIUserInterfaceIdiom Idiom { get; private set; }

		public ExportRendererAttribute (Type handler, Type target, UIUserInterfaceIdiom idiom)
			: base (handler, target) {
			Idiomatic = true;
			Idiom = idiom;
			}

		public ExportRendererAttribute (Type handler, Type target)
			: base (handler, target) {
			Idiomatic = false;
			}

		public override bool ShouldRegister () {
			return !Idiomatic || Idiom == UIDevice.CurrentDevice.UserInterfaceIdiom;
		}
	}
}