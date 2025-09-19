using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class BrushesOptionsPage : ContentPage
	{
		public BrushesOptionsPage(BrushesViewModel vm)
		{
			InitializeComponent();
			BindingContext = vm;
		}

		async void OnMainClicked(object sender, EventArgs e)
		{
			await Navigation.PopAsync();
		}

		void OnBrushTypeChanged(object sender, CheckedChangedEventArgs e)
		{
			if (!e.Value)
				return;
			if (BindingContext is not BrushesViewModel vm)
				return;
			if (sender == SolidRadio)
			{
				vm.ApplySolidBrushCommand.Execute(null);
				return;
			}
			if (sender == LinearRadio)
			{
				vm.ApplyLinearGradientCommand.Execute(null);
				return;
			}
			if (sender == RadialRadio)
			{
				vm.ApplyRadialGradientCommand.Execute(null);
				return;
			}
			if (sender == NoneRadio)
			{
				vm.ApplyNullBrushCommand.Execute(null);
				return;
			}
		}

		public void OnColor1Changed(object sender, CheckedChangedEventArgs e)
		{
			if (!e.Value)
				return;
			if (BindingContext is not BrushesViewModel vm)
				return;
			if (sender is RadioButton rb && rb.Content is string color)
			{
				vm.SelectedColorName1 = color;
			}
		}

		public void OnColor2Changed(object sender, CheckedChangedEventArgs e)
		{
			if (!e.Value)
				return;
			if (BindingContext is not BrushesViewModel vm)
				return;
			if (sender is RadioButton rb && rb.Content is string color)
			{
				vm.SelectedColorName2 = color;
			}
		}

		void OnShadowTypeChanged(object sender, CheckedChangedEventArgs e)
		{
			if (!e.Value)
				return;
			if (BindingContext is not BrushesViewModel vm)
				return;
			if (sender == ShadowSolidRadio)
			{
				vm.ApplySolidShadowCommand?.Execute(null);
				return;
			}
			if (sender == ShadowLinearRadio)
			{
				vm.ApplyLinearShadowCommand?.Execute(null);
				return;
			}
			if (sender == ShadowRadialRadio)
			{
				vm.ApplyRadialShadowCommand?.Execute(null);
				return;
			}
		}

		void OnStrokeTypeChanged(object sender, CheckedChangedEventArgs e)
		{
			if (!e.Value)
				return;
			if (BindingContext is not BrushesViewModel vm)
				return;
			SelectAndExecute(sender, vm,
				Solid: () => vm.ApplySolidStrokeCommand?.Execute(null),
				Linear: () => vm.ApplyLinearStrokeCommand?.Execute(null),
				Radial: () => vm.ApplyRadialStrokeCommand?.Execute(null),
				None: () => vm.ApplyNullStrokeCommand?.Execute(null));
		}

		void SelectAndExecute(object sender, BrushesViewModel vm, Action Solid, Action Linear, Action Radial, Action None)
		{
			if (sender == SolidRadio || sender == ShadowSolidRadio || sender == StrokeSolidRadio)
			{
				Solid?.Invoke();
				return;
			}
			if (sender == LinearRadio || sender == ShadowLinearRadio || sender == StrokeLinearRadio)
			{
				Linear?.Invoke();
				return;
			}
			if (sender == RadialRadio || sender == ShadowRadialRadio || sender == StrokeRadialRadio)
			{
				Radial?.Invoke();
				return;
			}
			if (sender == NoneRadio || sender == StrokeNoneRadio)
			{
				None?.Invoke();
				return;
			}
		}
	}
}