using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Animations
{
	/// <summary>
	/// Represents a linear interpolation animation.
	/// </summary>
	public class LerpingAnimation : Animation
	{
		/// <summary>
		/// Instantiate a new <see cref="LerpingAnimation"/> object.
		/// </summary>
		public LerpingAnimation()
		{
		}

		/// <summary>
		/// Instantiate a new <see cref="LerpingAnimation"/> object with the given parameters.
		/// </summary>
		/// <param name="callback">The <see cref="Action{T}"/> that is invoked after each tick of this animation.</param>
		/// <param name="start">Specifies a delay (in seconds) taken into account before the animation starts.</param>
		/// <param name="end">Specifies the duration that this animation should take in seconds.</param>
		/// <param name="easing">The easing function to apply to this animation.</param>
		/// <param name="finished">A callback <see cref="Action{T}"/> that is invoked after the animation has finished.</param>
		public LerpingAnimation(Action<double> callback, double start = 0, double end = 1, Easing? easing = null, Action? finished = null)
			: base(callback, start, end, easing, finished)
		{
		}

		/// <summary>
		/// Instantiate a new <see cref="LerpingAnimation"/> that consists of the given list of child animations.
		/// </summary>
		/// <param name="animations">A <see cref="List{T}"/> that contains <see cref="Animation"/> objects that will be children of the newly instantiated animation.</param>
		public LerpingAnimation(List<Animation> animations)
			: base(animations)
		{
		}

		/// <summary>
		/// Gets or sets a callback that is invoked when <see cref="CurrentValue"/> is changed.
		/// </summary>
		public Action? ValueChanged { get; set; }

		/// <summary>
		/// Gets or sets the start value of this animation.
		/// </summary>
		public object? StartValue { get; set; }

		/// <summary>
		/// Gets or sets the end value of this animation.
		/// </summary>
		public object? EndValue { get; set; }

		/// <summary>
		/// Gets the current value for this animation.
		/// </summary>
		public object? CurrentValue
		{
			get => _currentValue;
			protected set
			{
				if (_currentValue == value)
					return;
				_currentValue = value;
				ValueChanged?.Invoke();
			}
		}
		Lerp? _lerp;
		private object? _currentValue;

		/// <summary>
		/// Gets or sets the linear interpolation for this animation.
		/// </summary>
		public Lerp? Lerp
		{
			get
			{
				if (_lerp != null)
					return _lerp;

				//TODO: later we should find the first matching types of the subclasses
				var type = StartValue?.GetType() ?? EndValue?.GetType();
				if (type == null)
					return null;
				return _lerp = Lerp.GetLerp(type);
			}
			set => _lerp = value;
		}

		/// <inheritdoc/>
		public override void Update(double percent)
		{
			try
			{
				base.Update(percent);
				if (Lerp != null! && StartValue != null && EndValue != null)
					CurrentValue = Lerp.Calculate?.Invoke(StartValue, EndValue, Progress);
			}
			catch (Exception ex)
			{
				//TODO log exception
				Console.WriteLine(ex);
				HasFinished = true;
			}
		}
	}
}