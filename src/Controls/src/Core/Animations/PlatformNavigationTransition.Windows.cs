#nullable enable
using System;
using Microsoft.UI.Xaml.Media.Animation;

namespace Microsoft.Maui.Controls.Animations
{
	public sealed partial class PlatformNavigationTransition
	{
		/// <summary>
		/// A transition run by WinUI's <see cref="NavigationTransitionInfo"/> for the frame navigation.
		/// </summary>
		public PlatformNavigationTransition(NavigationTransitionInfo transitionInfo)
		{
			_kind = PlatformNavigationTransitionKind.Custom;
			TransitionInfo = transitionInfo ?? throw new ArgumentNullException(nameof(transitionInfo));
		}

		internal NavigationTransitionInfo? TransitionInfo { get; }
	}
}
