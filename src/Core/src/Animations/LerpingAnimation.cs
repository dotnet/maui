using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Animations
{
	public class LerpingAnimation : Animation
	{
		/// <summary>
		/// Instantiate a new <see cref="LerpingAnimation"/> object.
		/// </summary>
		public LerpingAnimation()
		{
		}

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

		public Action? ValueChanged { get; set; }
		public object? StartValue { get; set; }
		public object? EndValue { get; set; }
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