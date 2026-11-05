using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AndroidGraphicsDebug = Android.OS.Debug;
using Microsoft.Maui.Graphics.Platform;
using Xunit;

namespace Microsoft.Maui.Graphics.DeviceTests;

public class AndroidGraphicsPathMemoryTests
{
	const int WarmupDraws = 100;
	const int MeasuredDraws = 600;
	const int MeasuredBatches = 3;
	const int OperationsPerDraw = 20;
	// The issue reports at least 3.21 MiB of growth per 12,000 path operations.
	const long MaxPathGrowthBeyondControlKb = 2 * 1024;

	[Fact]
	public async Task RepeatedPathDrawingDoesNotAccumulateNativeMemory()
	{
		using var context = new PlatformBitmapExportContext(240, 240);
		using var polygon = new PathF();
		using var arcs = new PathF();

		polygon.MoveTo(20, 20).LineTo(200, 20).LineTo(200, 200).LineTo(20, 200).Close();
		arcs.AddArc(20, 20, 200, 200, 0, 180, true);
		arcs.AddArc(20, 20, 200, 200, 180, 360, true);
		arcs.Close();

		var growthByScenario = new Dictionary<string, long>
		{
			["control"] = await MeasurePssGrowthAsync(context.Canvas, "control", polygon, arcs)
		};

		foreach (var scenario in new[] { "clip-polygon", "clip-arcs", "fill-arcs", "draw-arcs" })
			growthByScenario[scenario] = await MeasurePssGrowthAsync(context.Canvas, scenario, polygon, arcs);

		var controlGrowth = growthByScenario["control"];
		var report = string.Join(", ", growthByScenario.Select(pair => $"{pair.Key}: {pair.Value} KB"));

		foreach (var scenario in new[] { "clip-polygon", "clip-arcs", "fill-arcs", "draw-arcs" })
		{
			Assert.True(
				growthByScenario[scenario] - controlGrowth <= MaxPathGrowthBeyondControlKb,
				$"PSS growth for {scenario} exceeded the control by more than {MaxPathGrowthBeyondControlKb} KB. Results: {report}");
		}
	}

	static async Task<long> MeasurePssGrowthAsync(ICanvas canvas, string scenario, PathF polygon, PathF arcs)
	{
		for (var i = 0; i < WarmupDraws; i++)
			Draw(canvas, scenario, polygon, arcs);

		await CollectAndSettleAsync();
		var batchPss = new long[MeasuredBatches];

		for (var batch = 0; batch < MeasuredBatches; batch++)
		{
			for (var i = 0; i < MeasuredDraws; i++)
				Draw(canvas, scenario, polygon, arcs);

			await CollectAndSettleAsync();
			batchPss[batch] = await GetMedianPssAsync();
		}

		return batchPss[^1] - batchPss[^2];
	}

	static void Draw(ICanvas canvas, string scenario, PathF polygon, PathF arcs)
	{
		canvas.FillColor = Colors.CornflowerBlue;

		for (var i = 0; i < OperationsPerDraw; i++)
		{
			switch (scenario)
			{
				case "control":
					canvas.FillRectangle(20, 20, 180, 180);
					break;
				case "clip-polygon":
				case "clip-arcs":
					canvas.SaveState();
					try
					{
						canvas.ClipPath(scenario == "clip-polygon" ? polygon : arcs);
						canvas.FillRectangle(20, 20, 180, 180);
					}
					finally
					{
						canvas.RestoreState();
					}
					break;
				case "fill-arcs":
					canvas.FillPath(arcs, WindingMode.NonZero);
					break;
				case "draw-arcs":
					canvas.DrawPath(arcs);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown drawing scenario.");
			}
		}
	}

	static async Task CollectAndSettleAsync()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		await Task.Delay(TimeSpan.FromSeconds(2));
	}

	static async Task<long> GetMedianPssAsync()
	{
		var samples = new long[3];

		for (var i = 0; i < samples.Length; i++)
		{
			using var memoryInfo = new AndroidGraphicsDebug.MemoryInfo();
			AndroidGraphicsDebug.GetMemoryInfo(memoryInfo);
			samples[i] = memoryInfo.TotalPss;

			if (i < samples.Length - 1)
				await Task.Delay(TimeSpan.FromMilliseconds(250));
		}

		Array.Sort(samples);
		return samples[1];
	}
}
