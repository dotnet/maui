#nullable disable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			base.OnPropertyChanged(propertyName);
			if (propertyName == Shell.FlyoutIsPresentedProperty.PropertyName)
				Handler?.UpdateValue(nameof(IFlyoutView.IsPresented));
		}
	}
}
