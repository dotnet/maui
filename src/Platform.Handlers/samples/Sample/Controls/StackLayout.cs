using System.Linq;
using System.Runtime.InteropServices;
using Xamarin.Platform;

namespace Sample
{
	public abstract class StackLayout : Layout, IStackLayout
	{
		public int Spacing { get; set; }

		bool _isMeasureValid;
		public override bool IsMeasureValid
		{
			get
			{
				return _isMeasureValid
					&& Children.All(child => child.IsMeasureValid);
			}

			protected set
			{
				_isMeasureValid = value;
			}
		}
	}
}
