using static Xamarin.Forms.Core.Markup.Markup;

namespace Xamarin.Forms.Markup
{
	public static class ViewExtensions
	{
		public static TView Start<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.Start; return view; }

		public static TView CenterHorizontal<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.Center; return view; }

		public static TView FillHorizontal<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.Fill; return view; }

		public static TView End<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.End; return view; }

		public static TView StartExpand<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.StartAndExpand; return view; }

		public static TView CenterExpandHorizontal<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.CenterAndExpand; return view; }

		public static TView FillExpandHorizontal<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.FillAndExpand; return view; }

		public static TView EndExpand<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.EndAndExpand; return view; }

		public static TView Top<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.VerticalOptions = LayoutOptions.Start; return view; }

		public static TView Bottom<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.VerticalOptions = LayoutOptions.End; return view; }

		public static TView CenterVertical<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.VerticalOptions = LayoutOptions.Center; return view; }

		public static TView FillVertical<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.VerticalOptions = LayoutOptions.Fill; return view; }

		public static TView TopExpand<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.VerticalOptions = LayoutOptions.StartAndExpand; return view; }

		public static TView BottomExpand<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.VerticalOptions = LayoutOptions.EndAndExpand; return view; }

		public static TView CenterExpandVertical<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.VerticalOptions = LayoutOptions.CenterAndExpand; return view; }

		public static TView FillExpandVertical<TView>(this TView view) where TView : View
		{ VerifyExperimental(); view.VerticalOptions = LayoutOptions.FillAndExpand; return view; }

		public static TView Center<TView>(this TView view) where TView : View
			=> view.CenterHorizontal().CenterVertical();

		public static TView Fill<TView>(this TView view) where TView : View
			=> view.FillHorizontal().FillVertical();

		public static TView CenterExpand<TView>(this TView view) where TView : View
			=> view.CenterExpandHorizontal().CenterExpandVertical();

		public static TView FillExpand<TView>(this TView view) where TView : View
			=> view.FillExpandHorizontal().FillExpandVertical();

		public static TView Margin<TView>(this TView view, Thickness margin) where TView : View
		{ VerifyExperimental(); view.Margin = margin; return view; }

		public static TView Margin<TView>(this TView view, double horizontal, double vertical) where TView : View
		{ VerifyExperimental(); view.Margin = new Thickness(horizontal, vertical); return view; }

		public static TView Margins<TView>(this TView view, double left = 0, double top = 0, double right = 0, double bottom = 0) where TView : View
		{ VerifyExperimental(); view.Margin = new Thickness(left, top, right, bottom); return view; }
	}

	namespace LeftToRight
	{
		public static class ViewExtensions
		{
			public static TView Left<TView>(this TView view) where TView : View
			{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.Start; return view; }

			public static TView Right<TView>(this TView view) where TView : View
			{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.End; return view; }

			public static TView LeftExpand<TView>(this TView view) where TView : View
			{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.StartAndExpand; return view; }

			public static TView RightExpand<TView>(this TView view) where TView : View
			{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.EndAndExpand; return view; }
		}
	}

	namespace RightToLeft
	{
		public static class ViewExtensions
		{
			public static TView Left<TView>(this TView view) where TView : View
			{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.End; return view; }

			public static TView Right<TView>(this TView view) where TView : View
			{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.Start; return view; }

			public static TView LeftExpand<TView>(this TView view) where TView : View
			{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.EndAndExpand; return view; }

			public static TView RightExpand<TView>(this TView view) where TView : View
			{ VerifyExperimental(); view.HorizontalOptions = LayoutOptions.StartAndExpand; return view; }
		}
	}
}