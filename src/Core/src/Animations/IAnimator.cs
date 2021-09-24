using System;
namespace Microsoft.Maui.Animations
{
	public interface IAnimator
	{
		void AddAnimation(Animation animation);
		void RemoveAnimation(Animation animation);
	}
}
