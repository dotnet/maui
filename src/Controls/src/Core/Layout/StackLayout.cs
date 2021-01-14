using System.Linq;

// This is a temporary namespace until we rename everything and move the legacy layouts
namespace Microsoft.Maui.Controls.Layout2
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
