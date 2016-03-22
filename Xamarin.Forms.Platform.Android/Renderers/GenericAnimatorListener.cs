using System;
using Android.Animation;

namespace Xamarin.Forms.Platform.Android
{
	public class GenericAnimatorListener : AnimatorListenerAdapter
	{
		public Action<Animator> OnCancel { get; set; }

		public Action<Animator> OnEnd { get; set; }

		public Action<Animator> OnRepeat { get; set; }

		public override void OnAnimationCancel(Animator animation)
		{
			if (OnCancel != null)
				OnCancel(animation);
			base.OnAnimationCancel(animation);
		}

		public override void OnAnimationEnd(Animator animation)
		{
			if (OnEnd != null)
				OnEnd(animation);
			base.OnAnimationEnd(animation);
		}

		public override void OnAnimationRepeat(Animator animation)
		{
			if (OnRepeat != null)
				OnRepeat(animation);
			base.OnAnimationRepeat(animation);
		}

		protected override void JavaFinalize()
		{
			OnCancel = OnRepeat = OnEnd = null;
			base.JavaFinalize();
		}
	}
}