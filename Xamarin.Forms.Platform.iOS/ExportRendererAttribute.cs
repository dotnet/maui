using System;
using UIKit;

namespace Xamarin.Forms
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
	public sealed class ExportRendererAttribute : HandlerAttribute
	{
		public ExportRendererAttribute(Type handler, Type target, UIUserInterfaceIdiom idiom) : base(handler, target)
		{
			Idiomatic = true;
			Idiom = idiom;
		}

		public ExportRendererAttribute(Type handler, Type target) : base(handler, target)
		{
			Idiomatic = false;
		}

		internal UIUserInterfaceIdiom Idiom { get; }

		internal bool Idiomatic { get; }

		public override bool ShouldRegister()
		{
			return !Idiomatic || Idiom == UIDevice.CurrentDevice.UserInterfaceIdiom;
		}
	}
}