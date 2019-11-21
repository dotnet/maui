using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Xamarin.Forms
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IImageElement
	{
		//note to implementor: implement this property publicly
		Aspect Aspect { get; }
		ImageSource Source { get; }
		bool IsOpaque { get; }


		//note to implementor: but implement these methods explicitly
		void RaiseImageSourcePropertyChanged();
		void OnImageSourcesSourceChanged(object sender, EventArgs e);
		bool IsLoading { get; }
		bool IsAnimationPlaying { get; }
	}
}
