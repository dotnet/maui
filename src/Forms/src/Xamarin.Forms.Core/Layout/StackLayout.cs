using System.Linq;
using Xamarin.Platform;

// This is a temporary namespace until we rename everything and move the legacy layouts
namespace Xamarin.Forms.Layout2
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
