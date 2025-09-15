using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public class BrushesViewModel : INotifyPropertyChanged
{
	private Brush brushTarget = new SolidColorBrush(Colors.Transparent);
	private Brush strokeBrush = new SolidColorBrush(Colors.Transparent);
	private Shadow shadowBrush;
	private bool isEmpty = false;
	private double brushOpacity = 1.0;

	public BrushesViewModel()
	{
	// Start with transparent shadow so nothing is visible until user selects
	ShadowBrush = BuildShadow(new SolidColorBrush(Colors.Transparent));

		ApplySolidBrushCommand = new Command(() =>
			BrushTarget = BuildSolid(Colors.Red));

		ApplyLinearGradientCommand = new Command(() =>
			BrushTarget = BuildLinear(
				new GradientStopCollection
				{
					new GradientStop(Colors.Blue, 0.0f),
					new GradientStop(Colors.Green, 1.0f)
				},
				new Point(0, 0),
				new Point(1, 0)));

		ApplyRadialGradientCommand = new Command(() =>
			BrushTarget = BuildRadial(
				new GradientStopCollection
				{
					new GradientStop(Colors.Purple, 0.1f),
					new GradientStop(Colors.Pink, 1.0f)
				},
				new Point(0.5, 0.5),
				0.5f));

		ApplyNullBrushCommand = new Command(() => BrushTarget = null);

		ApplySolidShadowCommand = new Command(() =>
			ShadowBrush = BuildShadow(BuildSolid(Colors.DarkGray)));

		ApplyLinearShadowCommand = new Command(() =>
			ShadowBrush = BuildShadow(BuildLinear(
				new GradientStopCollection
				{
					new GradientStop(Colors.Orange, 0.45f),
					new GradientStop(Colors.Red, 0.55f)
				},
				new Point(0, 0),
				new Point(1, 0))));

		ApplyRadialShadowCommand = new Command(() =>
			ShadowBrush = BuildShadow(BuildRadial(
				new GradientStopCollection
				{
					new GradientStop(new Color(0.2235f, 1.0f, 0.0784f), 0.0f),
					new GradientStop(new Color(0.196f, 0.8039f, 0.196f), 0.55f),
					new GradientStop(new Color(0.0f, 0.3922f, 0.0f), 1.0f)
				},
				new Point(0.5, 0.5),
				0.5f)));

		ApplySolidStrokeCommand = new Command(() => StrokeBrush = BuildSolid(Colors.DarkSlateGray));
		ApplyLinearStrokeCommand = new Command(() =>
			StrokeBrush = BuildLinear(
				new GradientStopCollection
				{
					new GradientStop(Colors.Gold, 0.0f),
					new GradientStop(Colors.OrangeRed, 1.0f)
				},
				new Point(0, 0),
				new Point(1, 0)));
		ApplyRadialStrokeCommand = new Command(() =>
			StrokeBrush = BuildRadial(
				new GradientStopCollection
				{
					new GradientStop(Colors.AliceBlue, 0.0f),
					new GradientStop(Colors.LightSkyBlue, 0.4f),
					new GradientStop(Colors.DeepSkyBlue, 0.75f)
				},
				new Point(0.5, 0.5),
				0.5f));
		ApplyNullStrokeCommand = new Command(() => StrokeBrush = null);

		// Alternate background variants (triggered by buttons)
		ApplyAltSolidBrushCommand = new Command(() =>
			BrushTarget = BuildSolid(Colors.CornflowerBlue));

		ApplyAltLinearBrushCommand = new Command(() =>
			BrushTarget = BuildLinear(
				new GradientStopCollection
				{
					new GradientStop(Colors.DarkBlue, 0.0f),
					new GradientStop(Colors.DodgerBlue, 0.35f),
					new GradientStop(Colors.LightSkyBlue, 0.7f),
					new GradientStop(Colors.AliceBlue, 1.0f)
				},
				new Point(0, 0),
				new Point(1, 0)));

		ApplyAltRadialBrushCommand = new Command(() =>
			BrushTarget = BuildRadial(
				new GradientStopCollection
				{
					new GradientStop(Colors.LightCyan, 0.0f),
					new GradientStop(Colors.DeepSkyBlue, 0.5f),
					new GradientStop(Colors.MidnightBlue, 1.0f)
				},
				new Point(0.5, 0.5),
				0.55f));
	}

	public Brush BrushTarget
	{
		get => brushTarget;
		set
		{
			if (brushTarget == value)
				return;
			brushTarget = value;
			OnPropertyChanged();
			OnPropertyChanged(nameof(HasBrush));
			UpdateDerivedProperties();
		}
	}

	public bool HasBrush => BrushTarget != null && !Microsoft.Maui.Controls.Brush.IsNullOrEmpty(BrushTarget);

	public bool IsEmpty
	{
		get => isEmpty;
		private set { if (isEmpty == value) return; isEmpty = value; OnPropertyChanged(); }
	}

	public Brush StrokeBrush
	{
		get => strokeBrush;
		private set { if (strokeBrush == value) return; strokeBrush = value; OnPropertyChanged(); }
	}

	public Shadow ShadowBrush
	{
		get => shadowBrush;
		set
		{
			if (shadowBrush == value)
				return;
			shadowBrush = value;
			OnPropertyChanged();
		}
	}

	public double BrushOpacity
	{
		get => brushOpacity;
		set
		{
			var clamped = Math.Clamp(value, 0.0, 1.0);
			if (Math.Abs(brushOpacity - clamped) < double.Epsilon)
				return;
			brushOpacity = clamped;
			RebuildBrushWithOpacity();
			OnPropertyChanged();
		}
	}

	public ICommand ApplySolidBrushCommand { get; }
	public ICommand ApplyLinearGradientCommand { get; }
	public ICommand ApplyRadialGradientCommand { get; }
	public ICommand ApplyNullBrushCommand { get; }

	public ICommand ApplySolidShadowCommand { get; }
	public ICommand ApplyLinearShadowCommand { get; }
	public ICommand ApplyRadialShadowCommand { get; }

	public ICommand ApplySolidStrokeCommand { get; }
	public ICommand ApplyLinearStrokeCommand { get; }
	public ICommand ApplyRadialStrokeCommand { get; }
	public ICommand ApplyNullStrokeCommand { get; }

	// Alternate background variants (triggered by buttons)
	public ICommand ApplyAltSolidBrushCommand { get; }
	public ICommand ApplyAltLinearBrushCommand { get; }
	public ICommand ApplyAltRadialBrushCommand { get; }

	void RebuildBrushWithOpacity()
	{
		if (BrushTarget == null || Microsoft.Maui.Controls.Brush.IsNullOrEmpty(BrushTarget))
			return;

		switch (BrushTarget)
		{
			case SolidColorBrush scb:
				BrushTarget = BuildSolid(scb.Color);
				break;
			case LinearGradientBrush lgb:
				BrushTarget = BuildLinear(lgb.GradientStops, lgb.StartPoint, lgb.EndPoint);
				break;
			case RadialGradientBrush rgb:
				BrushTarget = BuildRadial(rgb.GradientStops, rgb.Center, rgb.Radius);
				break;
		}
	}

	SolidColorBrush BuildSolid(Color color) => new(color.WithAlpha((float)brushOpacity));

	LinearGradientBrush BuildLinear(GradientStopCollection stops, Point startPoint, Point endPoint)
	{
		var newStops = new GradientStopCollection();
		foreach (var gs in stops)
			newStops.Add(new GradientStop(gs.Color.WithAlpha((float)brushOpacity), gs.Offset));
		return new LinearGradientBrush(newStops, startPoint, endPoint);
	}

	RadialGradientBrush BuildRadial(GradientStopCollection stops, Point center, double radius)
	{
		var newStops = new GradientStopCollection();
		foreach (var gs in stops)
			newStops.Add(new GradientStop(gs.Color.WithAlpha((float)brushOpacity), gs.Offset));
		return new RadialGradientBrush(newStops, center, (float)radius);
	}

	void UpdateDerivedProperties()
	{
		IsEmpty = BrushTarget == null || Microsoft.Maui.Controls.Brush.IsNullOrEmpty(BrushTarget);
		if (IsEmpty)
		{
			StrokeBrush = null;
			ShadowBrush = null;
		}
		else
		{
			if (StrokeBrush == null)
				StrokeBrush = BrushTarget;
			if (ShadowBrush == null)
				ShadowBrush = BuildShadow(new SolidColorBrush(Colors.Black));
		}
	}

	Shadow BuildShadow(Brush source) => new Shadow
	{
		Brush = source,
		Radius = 20,
		Opacity = 0.8f,
		Offset = new Point(10, 10)
	};

	public event PropertyChangedEventHandler PropertyChanged;
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}