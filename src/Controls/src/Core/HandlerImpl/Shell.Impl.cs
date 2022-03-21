using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls
{
	public partial class Shell : IFlyoutView
	{
		IView IFlyoutView.Flyout => this.FlyoutContentView;
		IView IFlyoutView.Detail => null;

		bool IFlyoutView.IsPresented { get => FlyoutIsPresented; set => FlyoutIsPresented = value; }

		bool IFlyoutView.IsGestureEnabled => false;

		FlyoutBehavior IFlyoutView.FlyoutBehavior
		{
			get
			{
				return GetEffectiveFlyoutBehavior();
			}
		}
	}
}
