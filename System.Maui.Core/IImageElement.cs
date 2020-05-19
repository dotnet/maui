using System;
using System.ComponentModel;

namespace System.Maui
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
		void OnImageSourceSourceChanged(object sender, EventArgs e);
		bool IsLoading { get; }
		bool IsAnimationPlaying { get; }
	}
}
