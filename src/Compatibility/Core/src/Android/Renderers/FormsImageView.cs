//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Android.Content;
using Android.Runtime;
using Android.Widget;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	internal class FormsImageView : ImageView, IImageRendererController
	{
		bool _skipInvalidate;

		public FormsImageView(Context context) : base(context)
		{
		}

		protected FormsImageView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public override void Invalidate()
		{
			if (_skipInvalidate)
			{
				_skipInvalidate = false;
				return;
			}

			base.Invalidate();
		}

		public void SkipInvalidate()
		{
			_skipInvalidate = true;
		}

		void IImageRendererController.SetFormsAnimationDrawable(IFormsAnimationDrawable formsAnimationDrawable)
		{
		}


		bool IImageRendererController.IsDisposed => false || !this.IsAlive();
	}
}