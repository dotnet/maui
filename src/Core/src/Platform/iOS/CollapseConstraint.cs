using System;
using UIKit;

namespace Microsoft.Maui
{
	/// <summary>
	/// Constraint which forces a UIView's height to zero
	/// </summary>
	internal class CollapseConstraint : NSLayoutConstraint
	{
		public override NSLayoutRelation Relation => NSLayoutRelation.Equal;
		public override NSLayoutAttribute FirstAttribute => NSLayoutAttribute.Height;
		public override nfloat Multiplier => 0;
	}
}