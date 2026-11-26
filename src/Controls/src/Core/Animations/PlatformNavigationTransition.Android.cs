#nullable enable
using System;
using AAnimation = Android.Views.Animations.Animation;

namespace Microsoft.Maui.Controls.Animations
{
	public sealed partial class PlatformNavigationTransition
	{
		readonly Func<bool, AAnimation?>? _createAnimation;

		/// <summary>
		/// A transition run by fragment animations. <paramref name="createAnimation"/> is called once for the page
		/// being shown (with <see langword="true"/>) and once for the page being hidden (with <see langword="false"/>),
		/// and returns its animation, or <see langword="null"/> for none. For a modal, only the modal page animates.
		/// </summary>
		public PlatformNavigationTransition(Func<bool, AAnimation?> createAnimation)
		{
			_kind = PlatformNavigationTransitionKind.Custom;
			_createAnimation = createAnimation ?? throw new ArgumentNullException(nameof(createAnimation));
		}

		/// <summary>The animation for one side, when this is a custom transition.</summary>
		internal AAnimation? CreateAnimation(bool enter) => _createAnimation?.Invoke(enter);
	}
}
