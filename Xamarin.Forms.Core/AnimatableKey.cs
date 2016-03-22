using System;

namespace Xamarin.Forms
{
	internal class AnimatableKey
	{
		public AnimatableKey(IAnimatable animatable, string handle)
		{
			if (animatable == null)
			{
				throw new ArgumentNullException(nameof(animatable));
			}

			if (string.IsNullOrEmpty(handle))
			{
				throw new ArgumentException("Argument is null or empty", nameof(handle));
			}

			Animatable = new WeakReference<IAnimatable>(animatable);
			Handle = handle;
		}

		public WeakReference<IAnimatable> Animatable { get; }

		public string Handle { get; }

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (ReferenceEquals(this, obj))
			{
				return true;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((AnimatableKey)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				IAnimatable target;
				if (!Animatable.TryGetTarget(out target))
				{
					return Handle?.GetHashCode() ?? 0;
				}

				return ((target?.GetHashCode() ?? 0) * 397) ^ (Handle?.GetHashCode() ?? 0);
			}
		}

		protected bool Equals(AnimatableKey other)
		{
			if (!string.Equals(Handle, other.Handle))
			{
				return false;
			}

			IAnimatable thisAnimatable;

			if (!Animatable.TryGetTarget(out thisAnimatable))
			{
				return false;
			}

			IAnimatable thatAnimatable;

			if (!other.Animatable.TryGetTarget(out thatAnimatable))
			{
				return false;
			}

			return Equals(thisAnimatable, thatAnimatable);
		}
	}
}