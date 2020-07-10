
#if __ANDROID__
using System.Maui.Platform.Android;
#elif TIZEN4_0
using System.Maui.Platform.Tizen;
#elif __IOS__
using System.Maui.Platform.iOS;
#endif

namespace System.Maui.Platform
{
	internal static class Loader
	{
		internal static void Load()
		{
		}
	}

#if !WINDOWS_PHONE && !WINDOWS_PHONE_APP && !TIZEN4_0
	[RenderWith(typeof(BoxRenderer))]
#else
	[RenderWith(typeof(BoxViewRenderer))]
#endif
	internal class _BoxViewRenderer { }

	[RenderWith(typeof(EntryRenderer))]
	internal class _EntryRenderer { }

	[RenderWith(typeof(EditorRenderer))]
	internal class _EditorRenderer { }
#if __ANDROID__
	[RenderWith(typeof(System.Maui.Platform.Android.LabelRenderer))]
#else
	[RenderWith(typeof(LabelRenderer))]
#endif
	internal class _LabelRenderer { }

#if __ANDROID__
	[RenderWith(typeof(System.Maui.Platform.Android.ImageRenderer))]
#else
	[RenderWith(typeof(ImageRenderer))]
#endif
	internal class _ImageRenderer { }

#if __ANDROID__
	[RenderWith(typeof(System.Maui.Platform.Android.AppCompat.ButtonRenderer))]
#else
	[RenderWith(typeof(ButtonRenderer))]
#endif
	internal class _ButtonRenderer { }

	[RenderWith(typeof(ImageButtonRenderer))]
	internal class _ImageButtonRenderer { }

	[RenderWith(typeof(RadioButtonRenderer))]
	internal class _RadioButtonRenderer { }

	[RenderWith(typeof(TableViewRenderer))]
	internal class _TableViewRenderer { }

	[RenderWith(typeof(ListViewRenderer))]
	internal class _ListViewRenderer { }
#if !TIZEN4_0
	[RenderWith(typeof(CollectionViewRenderer))]
#else
	[RenderWith(typeof(StructuredItemsViewRenderer))]
#endif
	internal class _CollectionViewRenderer { }

	[RenderWith(typeof(CarouselViewRenderer))]
	internal class _CarouselViewRenderer { }

	[RenderWith(typeof(SliderRenderer))]
	internal class _SliderRenderer { }

#if __IOS__
	[RenderWith(typeof(WkWebViewRenderer))]
	internal class _WebViewRenderer { }
#else
	[RenderWith(typeof(WebViewRenderer))]
	internal class _WebViewRenderer { }
#endif

	[RenderWith(typeof(SearchBarRenderer))]
	internal class _SearchBarRenderer { }

#if __ANDROID__
	[RenderWith(typeof(System.Maui.Platform.Android.AppCompat.SwitchRenderer))]
#else
	[RenderWith(typeof(SwitchRenderer))]
#endif
	internal class _SwitchRenderer { }

	[RenderWith(typeof(DatePickerRenderer))]
	internal class _DatePickerRenderer { }

	[RenderWith(typeof(TimePickerRenderer))]
	internal class _TimePickerRenderer { }

#if __ANDROID__
	[RenderWith(typeof(System.Maui.Platform.Android.AppCompat.PickerRenderer))]
#else
	[RenderWith(typeof(PickerRenderer))]
#endif
	internal class _PickerRenderer { }

	[RenderWith(typeof(StepperRenderer))]
	internal class _StepperRenderer { }

	[RenderWith(typeof(ProgressBarRenderer))]
	internal class _ProgressBarRenderer { }

	[RenderWith(typeof(ScrollViewRenderer))]
	internal class _ScrollViewRenderer { }

	[RenderWith(typeof(ActivityIndicatorRenderer))]
	internal class _ActivityIndicatorRenderer { }

#if __ANDROID__
	[RenderWith(typeof(System.Maui.Platform.Android.AppCompat.FrameRenderer))]
#else
	[RenderWith(typeof(FrameRenderer))]
#endif
	internal class _FrameRenderer { }

#if __ANDROID__
	// current previewer doesn't work with appcompat so this renderer is here for the previewer only
	// once previewer switches to appcompat then we can remove this
	[RenderWith(typeof(CheckBoxDesignerRenderer))]
	internal class _CheckBoxRenderer { }
#endif

	[RenderWith(typeof(IndicatorViewRenderer))]
	internal class _IndicatorViewRenderer { }

#if __IOS__
	// current previewer doesn't work with appcompat so this renderer is here for the previewer only
	// once previewer switches to appcompat then we can remove this
	[RenderWith(typeof(CheckBoxRenderer))]
	internal class _CheckBoxRenderer { }
#endif

#if !WINDOWS_PHONE && !WINDOWS_PHONE_APP && !TIZEN4_0
	[RenderWith(typeof(OpenGLViewRenderer))]
#else
	[RenderWith(null)]
#endif
	internal class _OpenGLViewRenderer { }
#if __ANDROID__
	[RenderWith(typeof(System.Maui.Platform.Android.AppCompat.TabbedPageRenderer))]
#elif !WINDOWS_PHONE && !WINDOWS_PHONE_APP && !TIZEN4_0
	[RenderWith(typeof(TabbedRenderer))]
#else
	[RenderWith(typeof(TabbedPageRenderer))]
#endif
	internal class _TabbedPageRenderer { }
#if __ANDROID__
	[RenderWith(typeof(System.Maui.Platform.Android.AppCompat.NavigationPageRenderer))]
#elif !WINDOWS_PHONE && !WINDOWS_PHONE_APP && !TIZEN4_0
	[RenderWith(typeof(NavigationRenderer))]
#else
	[RenderWith(typeof(NavigationPageRenderer))]
#endif
	internal class _NavigationPageRenderer { }

#if __ANDROID__
	[RenderWith(typeof(System.Maui.Platform.Android.AppCompat.CarouselPageRenderer))]
#else
	[RenderWith(typeof(CarouselPageRenderer))]
#endif
	internal class _CarouselPageRenderer { }

	[RenderWith(typeof(PageRenderer))]
	internal class _PageRenderer { }

#if !__IOS__ && !TIZEN4_0
	[RenderWith(typeof(System.Maui.Platform.Android.AppCompat.MasterDetailPageRenderer))]
#elif TIZEN4_0
	[RenderWith(typeof(MasterDetailPageRenderer))]
#else
	[RenderWith(typeof(PhoneMasterDetailRenderer))]
#endif
	internal class _MasterDetailPageRenderer { }

	[RenderWith(typeof(MediaElementRenderer))]
	internal class _MediaElementRenderer { }

	[RenderWith(typeof(RefreshViewRenderer))]
	internal class _RefreshViewRenderer { }

	[RenderWith(typeof(SwipeViewRenderer))]
	internal class _SwipeViewRenderer { }
}





