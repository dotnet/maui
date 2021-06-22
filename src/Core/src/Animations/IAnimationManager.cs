using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Animations
{
	public interface IAnimationManager
	{
		double SpeedModifier { get; set; }
		ITicker Ticker { get; set; }
		void Add(Animation animation);
		void Remove(Animation animation);
	}
}
