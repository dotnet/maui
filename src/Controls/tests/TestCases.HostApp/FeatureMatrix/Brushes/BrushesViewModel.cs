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
	private double brushOpacity = 1.0;
	private double linearStartX = 0.0;
	private double linearStartY = 0.0;
	private double linearEndX = 1.0;
	private double linearEndY = 0.0;
	private double radialCenterX = 0.5;
	private double radialCenterY = 0.5;
	private double radialRadius = 0.5;
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
	public ICommand ApplyAltSolidBrushCommand { get; }
	public ICommand ApplyAltLinearBrushCommand { get; }
	public ICommand ApplyAltRadialBrushCommand { get; }
	public event PropertyChangedEventHandler PropertyChanged;
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

	public double LinearStartX { get => linearStartX; set { if (Math.Abs(linearStartX - value) < double.Epsilon) return; linearStartX = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double LinearStartY { get => linearStartY; set { if (Math.Abs(linearStartY - value) < double.Epsilon) return; linearStartY = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double LinearEndX { get => linearEndX; set { if (Math.Abs(linearEndX - value) < double.Epsilon) return; linearEndX = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double LinearEndY { get => linearEndY; set { if (Math.Abs(linearEndY - value) < double.Epsilon) return; linearEndY = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double RadialCenterX { get => radialCenterX; set { if (Math.Abs(radialCenterX - value) < double.Epsilon) return; radialCenterX = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double RadialCenterY { get => radialCenterY; set { if (Math.Abs(radialCenterY - value) < double.Epsilon) return; radialCenterY = value; OnPropertyChanged(); ApplyPointChanges(); } }
	public double RadialRadius { get => radialRadius; set { var clamped = Math.Clamp(value, 0.0, 1.0); if (Math.Abs(radialRadius - clamped) < double.Epsilon) return; radialRadius = clamped; OnPropertyChanged(); ApplyPointChanges(); } }

	public BrushesViewModel()
	{
		ShadowBrush = BuildShadow(new SolidColorBrush(Colors.Transparent));

		ApplySolidBrushCommand = new Command(() =>
		{
			BrushTarget = BuildSolid(Colors.Red);
		});

		ApplyLinearGradientCommand = new Command(() =>
		{
			var start = new Point(0, 0);
			var end = new Point(1, 0);
			BrushTarget = BuildLinear(
				new GradientStopCollection
				{
					new GradientStop(Colors.Blue, 0.0f),
					new GradientStop(Colors.Green, 1.0f)
				},
				start,
				end);
			LinearStartX = start.X;
			LinearStartY = start.Y;
			LinearEndX = end.X;
			LinearEndY = end.Y;
		});

		ApplyRadialGradientCommand = new Command(() =>
		{
			var center = new Point(0.5, 0.5);
			var radius = 0.5;
			BrushTarget = BuildRadial(
				new GradientStopCollection
				{
					new GradientStop(Colors.Purple, 0.1f),
					new GradientStop(Colors.Pink, 1.0f)
				},
				center,
				radius);
			RadialCenterX = center.X;
			RadialCenterY = center.Y;
			RadialRadius = radius;
		});

		ApplyNullBrushCommand = new Command(() => BrushTarget = null);

		ApplySolidShadowCommand = new Command(() =>
		{
			ShadowBrush = BuildShadow(BuildSolid(Colors.DarkGray));
		});

		ApplyLinearShadowCommand = new Command(() =>
		{
			var start = new Point(0, 0);
			var end = new Point(1, 0);
			ShadowBrush = BuildShadow(BuildLinear(
				new GradientStopCollection
				{
					new GradientStop(Colors.Orange, 0.45f),
					new GradientStop(Colors.Red, 0.55f)
				},
				start,
				end));
			LinearStartX = start.X;
			LinearStartY = start.Y;
			LinearEndX = end.X;
			LinearEndY = end.Y;
		});

		ApplyRadialShadowCommand = new Command(() =>
		{
			var center = new Point(0.5, 0.5);
			var radius = 0.5;
			ShadowBrush = BuildShadow(BuildRadial(
				new GradientStopCollection
				{
					new GradientStop(new Color(0.2235f, 1.0f, 0.0784f), 0.0f),
					new GradientStop(new Color(0.196f, 0.8039f, 0.196f), 0.55f),
					new GradientStop(new Color(0.0f, 0.3922f, 0.0f), 1.0f)
				},
				center,
				radius));
			RadialCenterX = center.X;
			RadialCenterY = center.Y;
			RadialRadius = radius;
		});

		ApplySolidStrokeCommand = new Command(() =>
		{
			StrokeBrush = BuildSolid(Colors.DarkSlateGray);
		});
		ApplyLinearStrokeCommand = new Command(() =>
		{
			var start = new Point(0, 0);
			var end = new Point(1, 0);
			StrokeBrush = BuildLinear(
				new GradientStopCollection
				{
					new GradientStop(Colors.Gold, 0.0f),
					new GradientStop(Colors.OrangeRed, 1.0f)
				},
				start,
				end);
			LinearStartX = start.X;
			LinearStartY = start.Y;
			LinearEndX = end.X;
			LinearEndY = end.Y;
		});

		ApplyRadialStrokeCommand = new Command(() =>
		{
			var center = new Point(0.5, 0.5);
			var radius = 0.5;
			StrokeBrush = BuildRadial(
				new GradientStopCollection
				{
					new GradientStop(Colors.AliceBlue, 0.0f),
					new GradientStop(Colors.LightSkyBlue, 0.4f),
					new GradientStop(Colors.DeepSkyBlue, 0.75f)
				},
				center,
				radius);
			RadialCenterX = center.X;
			RadialCenterY = center.Y;
			RadialRadius = radius;
		});
		ApplyNullStrokeCommand = new Command(() => StrokeBrush = null);
		ApplyAltSolidBrushCommand = new Command(() =>
		{
			BrushTarget = BuildSolid(Colors.CornflowerBlue);
		});

		ApplyAltLinearBrushCommand = new Command(() =>
		{
			var start = new Point(0, 0);
			var end = new Point(1, 0);
			BrushTarget = BuildLinear(
				new GradientStopCollection
				{
					new GradientStop(Colors.DarkBlue, 0.0f),
					new GradientStop(Colors.DodgerBlue, 0.35f),
					new GradientStop(Colors.LightSkyBlue, 0.7f),
					new GradientStop(Colors.AliceBlue, 1.0f)
				},
				start,
				end);
			LinearStartX = start.X;
			LinearStartY = start.Y;
			LinearEndX = end.X;
			LinearEndY = end.Y;
		});

		ApplyAltRadialBrushCommand = new Command(() =>
		{
			var center = new Point(0.5, 0.5);
			var radius = 0.55;
			BrushTarget = BuildRadial(
				new GradientStopCollection
				{
					new GradientStop(Colors.LightCyan, 0.0f),
					new GradientStop(Colors.DeepSkyBlue, 0.5f),
					new GradientStop(Colors.MidnightBlue, 1.0f)
				},
				center,
				radius);
			RadialCenterX = center.X;
			RadialCenterY = center.Y;
			RadialRadius = radius;
		});
	}

	void ApplyPointChanges()
	{
		if (BrushTarget is LinearGradientBrush lgb)
		{
			BrushTarget = BuildLinear(lgb.GradientStops, new Point(LinearStartX, LinearStartY), new Point(LinearEndX, LinearEndY));
		}
		else if (BrushTarget is RadialGradientBrush rgb)
		{
			BrushTarget = BuildRadial(rgb.GradientStops, new Point(RadialCenterX, RadialCenterY), RadialRadius);
		}

		if (StrokeBrush is LinearGradientBrush slgb)
		{
			StrokeBrush = BuildLinear(slgb.GradientStops, new Point(LinearStartX, LinearStartY), new Point(LinearEndX, LinearEndY));
		}
		else if (StrokeBrush is RadialGradientBrush srgb)
		{
			StrokeBrush = BuildRadial(srgb.GradientStops, new Point(RadialCenterX, RadialCenterY), RadialRadius);
		}

		if (ShadowBrush?.Brush is LinearGradientBrush shlgb)
		{
			ShadowBrush = BuildShadow(BuildLinear(shlgb.GradientStops, new Point(LinearStartX, LinearStartY), new Point(LinearEndX, LinearEndY)));
		}
		else if (ShadowBrush?.Brush is RadialGradientBrush shrgb)
		{
			ShadowBrush = BuildShadow(BuildRadial(shrgb.GradientStops, new Point(RadialCenterX, RadialCenterY), RadialRadius));
		}
	}

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

	void UpdateDerivedProperties()
	{
		bool nowEmpty = BrushTarget == null || Microsoft.Maui.Controls.Brush.IsNullOrEmpty(BrushTarget);
		if (nowEmpty)
		{
			StrokeBrush = new SolidColorBrush(Colors.Transparent);
			ShadowBrush = BuildShadow(new SolidColorBrush(Colors.Transparent));
		}
		else
		{
			if (StrokeBrush == null || Microsoft.Maui.Controls.Brush.IsNullOrEmpty(StrokeBrush))
				StrokeBrush = BrushTarget;
			if (ShadowBrush == null)
				ShadowBrush = BuildShadow(new SolidColorBrush(Colors.Black));
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

	Shadow BuildShadow(Brush source) => new Shadow
	{
		Brush = source,
		Radius = 20,
		Opacity = 0.8f,
		Offset = new Point(10, 10)
	};

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}