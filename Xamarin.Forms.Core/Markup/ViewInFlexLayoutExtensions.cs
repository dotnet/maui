using static Xamarin.Forms.Core.Markup.Markup;

namespace Xamarin.Forms.Markup
{
	public static class ViewInFlexLayoutExtensions
	{
		public static TView AlignSelf<TView>(this TView view, FlexAlignSelf value) where TView : View
		{
			VerifyExperimental();
			FlexLayout.SetAlignSelf(view, value);
			return view;
		}

		public static TView Basis<TView>(this TView view, FlexBasis value) where TView : View
		{
			VerifyExperimental();
			FlexLayout.SetBasis(view, value);
			return view;
		}

		public static TView Grow<TView>(this TView view, float value) where TView : View
		{
			VerifyExperimental();
			FlexLayout.SetGrow(view, value);
			return view;
		}

		public static TView Order<TView>(this TView view, int value) where TView : View
		{
			VerifyExperimental();
			FlexLayout.SetOrder(view, value);
			return view;
		}

		public static TView Shrink<TView>(this TView view, float value) where TView : View
		{
			VerifyExperimental();
			FlexLayout.SetShrink(view, value);
			return view;
		}
	}
}