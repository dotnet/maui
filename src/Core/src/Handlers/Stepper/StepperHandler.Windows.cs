﻿using System;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Handlers
{
	public partial class StepperHandler : AbstractViewHandler<IStepper, Panel>
	{
		protected override Panel CreateNativeView() => new Panel();

		public static void MapMinimum(IViewHandler handler, IStepper stepper) { }
		public static void MapMaximum(IViewHandler handler, IStepper stepper) { }
		public static void MapIncrement(IViewHandler handler, IStepper stepper) { }
		public static void MapValue(IViewHandler handler, IStepper stepper) { }
	}
}