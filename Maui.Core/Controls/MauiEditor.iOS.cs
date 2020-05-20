using CoreGraphics;
using UIKit;

namespace System.Maui.Core.Controls
{
	internal interface IMauiEditor
	{
		event EventHandler FrameChanged;
	}

	public class MauiEditor : UITextView, IMauiEditor
	{
		public event EventHandler FrameChanged;

		public MauiEditor(CGRect frame) : base(frame)
		{

		}

		public override CGRect Frame
		{
			get => base.Frame;
			set
			{
				base.Frame = value;
				FrameChanged?.Invoke(this, EventArgs.Empty);
			}
		}
	}
}