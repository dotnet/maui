using System;
using CoreGraphics;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace System.Maui.Platform.iOS
{
	public class FormsUIImageView : UIImageView
	{
		bool _isDisposed;
		const string AnimationLayerName = "FormsUIImageViewAnimation";
		FormsCAKeyFrameAnimation _animation;
		public event EventHandler<CoreAnimation.CAAnimationStateEventArgs> AnimationStopped;
		public FormsUIImageView() : base(RectangleF.Empty)
		{
		}

		public override UIImage Image
		{
			get
			{
				return base.Image;
			}
			set
			{
				base.Image = value;
			}
		}

		public override CGSize SizeThatFits(CGSize size)
		{
			if (Image == null && Animation != null)
			{
				return new CoreGraphics.CGSize(Animation.Width, Animation.Height);
			}

			return base.SizeThatFits(size);
		}

		public FormsCAKeyFrameAnimation Animation
		{
			get { return _animation; }
			set
			{
				if (_animation != null)
				{
					_animation.AnimationStopped -= OnAnimationStopped;
					Layer.RemoveAnimation(AnimationLayerName);
					_animation.Dispose();
				}

				_animation = value;
				if (_animation != null)
				{
					_animation.AnimationStopped += OnAnimationStopped;
					Layer.AddAnimation(_animation, AnimationLayerName);
				}

				Layer.SetNeedsDisplay();
			}
		}

		void OnAnimationStopped(object sender, CoreAnimation.CAAnimationStateEventArgs e)
		{
			AnimationStopped?.Invoke(this, e);
		}

		public override bool IsAnimating
		{
			get
			{
				if (_animation != null)
					return Layer.Speed != 0.0f;
				else
					return base.IsAnimating;
			}
		}

		public override void StartAnimating()
		{
			if (_animation != null && Layer.Speed == 0.0f)
			{
				Layer.RemoveAnimation(AnimationLayerName);
				Layer.AddAnimation(_animation, AnimationLayerName);
				Layer.Speed = 1.0f;
			}
			else
			{
				base.StartAnimating();
			}
		}

		public override void StopAnimating()
		{
			if (_animation != null && Layer.Speed != 0.0f)
			{
				Layer.RemoveAnimation(AnimationLayerName);
				Layer.AddAnimation(_animation, AnimationLayerName);
				Layer.Speed = 0.0f;
			}
			else
			{
				base.StopAnimating();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			_isDisposed = true;

			if (disposing && _animation != null)
			{
				_animation.AnimationStopped -= OnAnimationStopped;
				Layer.RemoveAnimation(AnimationLayerName);
				_animation.Dispose();
				_animation = null;
			}

			base.Dispose(disposing);
		}
	}
}