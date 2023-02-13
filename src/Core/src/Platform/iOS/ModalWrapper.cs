#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	internal class ModalWrapper : UIViewController
	{
		internal ModalWrapper()
		{
		}


		public override void WillMoveToParentViewController(UIViewController parent)
		{
			base.WillMoveToParentViewController(parent);
		}
	}
}