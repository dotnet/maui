// This is a temporary namespace until we rename everything and move the legacy layouts
namespace Microsoft.Maui.Controls.Layout2
{
	public abstract class StackLayout : Layout, IStackLayout
	{
		public int Spacing { get; set; }

		bool _isMeasureValid;
		bool _isArrangeValid;

		public override bool IsMeasureValid
		{
			get
			{
				if (!_isMeasureValid)
				{
					return false;
				}

				// Children.All() would be more succinct, but the for loop is just a tad faster
				for (int n = 0; n < Children.Count; n++)
				{
					if (!Children[n].IsMeasureValid)
					{
						return false;
					}
				}

				return true;
			}

			protected set
			{
				_isMeasureValid = value;
			}
		}

		public override bool IsArrangeValid 
		{
			get
			{
				if (!_isArrangeValid)
				{
					return false;
				}

				for (int n = 0; n < Children.Count; n++)
				{
					if (!Children[n].IsArrangeValid)
					{
						return false;
					}
				}

				return true;
			}

			protected set
			{
				_isArrangeValid = value;
			}
		}
	}
}
