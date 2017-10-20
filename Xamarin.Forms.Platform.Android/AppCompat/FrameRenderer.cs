using System;
using Android.Content;

namespace Xamarin.Forms.Platform.Android.AppCompat
{
    // This version of FrameRenderer is here for backward compatibility with anyone referencing 
    // FrameRenderer from this namespace
    public class FrameRenderer : FastRenderers.FrameRenderer
    {
	    public FrameRenderer(Context context) : base(context)
	    {
		    
	    }

	    [Obsolete("This constructor is obsolete as of version 2.5. Please use FrameRenderer(Context) instead.")]
	    public FrameRenderer()
	    {
		   
	    }
    }
}