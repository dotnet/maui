using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

#if __ANDROID__
using Xamarin.Forms.Platform.Android;
#elif TIZEN4_0
using Xamarin.Forms.Platform.Tizen;
#elif __IOS__
using Xamarin.Forms.Platform.iOS;
#endif

namespace Xamarin.Forms.Platform
{
	internal static class Loader
	{
		internal static void Load ()
		{
		}
	}

#if !WINDOWS_PHONE && !WINDOWS_PHONE_APP && !TIZEN4_0
	[RenderWith(typeof(BoxRenderer))]
#else
	[RenderWith (typeof(BoxViewRenderer))]
#endif
	internal class _BoxViewRenderer { }

	[RenderWith(typeof(EntryRenderer))]
	internal class _EntryRenderer { }

	[RenderWith (typeof (EditorRenderer))]
	internal class _EditorRenderer { }
#if __ANDROID__
	[RenderWith(typeof(Xamarin.Forms.Platform.Android.LabelRenderer))]
#else
	[RenderWith (typeof (LabelRenderer))]
#endif
	internal class _LabelRenderer { }

#if __ANDROID__
	[RenderWith(typeof(Xamarin.Forms.Platform.Android.ImageRenderer))]
#else
	[RenderWith (typeof (ImageRenderer))]
#endif
	internal class _ImageRenderer { }

	[RenderWith (typeof (ButtonRenderer))]
	internal class _ButtonRenderer { }

	[RenderWith(typeof(ImageButtonRenderer))]
	internal class _ImageButtonRenderer { }

#if __ANDROID__
	[RenderWith(typeof(RadioButtonRenderer))]
#elif !TIZEN4_0
	[RenderWith(typeof(RadioButtonRenderer))]
#endif
	internal class _RadioButtonRenderer { }

	[RenderWith (typeof (TableViewRenderer))]
	internal class _TableViewRenderer { }

	[RenderWith (typeof (ListViewRenderer))]
	internal class _ListViewRenderer { }
#if !TIZEN4_0
	[RenderWith (typeof (CollectionViewRenderer))]
#else
	[RenderWith (typeof (StructuredItemsViewRenderer))]
#endif
	internal class _CollectionViewRenderer { }

	[RenderWith (typeof (CarouselViewRenderer))]
	internal class _CarouselViewRenderer { }

	[RenderWith (typeof (SliderRenderer))]
	internal class _SliderRenderer { }

#if __IOS__
	[RenderWith (typeof (WkWebViewRenderer))]
	internal class _WebViewRenderer { }
#else
	[RenderWith(typeof(WebViewRenderer))]
	internal class _WebViewRenderer { }
#endif

	[RenderWith (typeof (SearchBarRenderer))]
	internal class _SearchBarRenderer { }

	[RenderWith (typeof (SwitchRenderer))]
	internal class _SwitchRenderer { }

	[RenderWith (typeof (DatePickerRenderer))]
	internal class _DatePickerRenderer { }

	[RenderWith (typeof (TimePickerRenderer))]
	internal class _TimePickerRenderer { }

	[RenderWith (typeof (PickerRenderer))]
	internal class _PickerRenderer { }

	[RenderWith (typeof (StepperRenderer))]
	internal class _StepperRenderer { }

	[RenderWith (typeof (ProgressBarRenderer))]
	internal class _ProgressBarRenderer { }

	[RenderWith (typeof (ScrollViewRenderer))]
	internal class _ScrollViewRenderer { }

	[RenderWith (typeof (ActivityIndicatorRenderer))]
	internal class _ActivityIndicatorRenderer { }

	[RenderWith (typeof (FrameRenderer))]
	internal class _FrameRenderer { }

#if __ANDROID__
	// current previewer doesn't work with appcompat so this renderer is here for the previewer only
	// once previewer switches to appcompat then we can remove this
	[RenderWith(typeof(CheckBoxDesignerRenderer))]
	internal class _CheckBoxRenderer { }
#endif

#if !TIZEN4_0
	[RenderWith(typeof(IndicatorViewRenderer))]
#endif
	internal class _IndicatorViewRenderer { }

#if __IOS__
	// current previewer doesn't work with appcompat so this renderer is here for the previewer only
	// once previewer switches to appcompat then we can remove this
	[RenderWith(typeof(CheckBoxRenderer))]
	internal class _CheckBoxRenderer { }
#endif

#if !WINDOWS_PHONE && !WINDOWS_PHONE_APP && !TIZEN4_0
	[RenderWith (typeof (OpenGLViewRenderer))]
#else
	[RenderWith (null)]
#endif
	internal class _OpenGLViewRenderer { }

#if !WINDOWS_PHONE && !WINDOWS_PHONE_APP && !TIZEN4_0
	[RenderWith (typeof (TabbedRenderer))]
#else
	[RenderWith (typeof (TabbedPageRenderer))]
#endif
	internal class _TabbedPageRenderer { }

#if !WINDOWS_PHONE && !WINDOWS_PHONE_APP && !TIZEN4_0
	[RenderWith (typeof (NavigationRenderer))]
#else
	[RenderWith (typeof (NavigationPageRenderer))]
#endif
	internal class _NavigationPageRenderer { }

	[RenderWith (typeof (CarouselPageRenderer))]
	internal class _CarouselPageRenderer { }

	[RenderWith (typeof (PageRenderer))]
	internal class _PageRenderer { }

#if !__IOS__ && !TIZEN4_0
	[RenderWith (typeof (MasterDetailRenderer))]
#elif TIZEN4_0
	[RenderWith (typeof(MasterDetailPageRenderer))]
#else
	[RenderWith (typeof (PhoneMasterDetailRenderer))]
#endif
	internal class _MasterDetailPageRenderer { }

	[RenderWith (typeof(MediaElementRenderer))]
	internal class _MediaElementRenderer { }

	[RenderWith(typeof(RefreshViewRenderer))]
	internal class _RefreshViewRenderer { }

	[RenderWith(typeof(SwipeViewRenderer))]
	internal class _SwipeViewRenderer { }
}





