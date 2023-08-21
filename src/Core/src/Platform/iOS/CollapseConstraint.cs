// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
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