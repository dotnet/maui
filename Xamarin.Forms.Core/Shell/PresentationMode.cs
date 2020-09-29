using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms
{
	[Flags]
	public enum PresentationMode
	{
		NotAnimated = 1,
		Animated = 1 << 1,
		Modal = 1 << 2,
		ModalAnimated = PresentationMode.Animated | PresentationMode.Modal,
		ModalNotAnimated = PresentationMode.NotAnimated | PresentationMode.Modal
	}
}