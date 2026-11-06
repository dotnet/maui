using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Java.Interop;
using Microsoft.Maui.Graphics.Platform;
using Xunit;

namespace Microsoft.Maui.Graphics.DeviceTests;

public class AndroidGraphicsPathMemoryTests
{
	[Theory]
	[InlineData("clip-polygon")]
	[InlineData("clip-arcs")]
	[InlineData("fill-arcs")]
	[InlineData("draw-arcs")]
	[InlineData("segment-arc")]
	[InlineData("rotated-arcs")]
	public async Task PathOperationsDisposeTemporaryJavaPeers(string scenario)
	{
		await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>
		{
			using var context = new PlatformBitmapExportContext(240, 240);
			using var polygon = new PathF();
			using var arcs = new PathF();

			polygon.MoveTo(20, 20).LineTo(200, 20).LineTo(200, 200).LineTo(20, 200).Close();
			arcs.AddArc(20, 20, 200, 200, 0, 180, true);
			arcs.AddArc(20, 20, 200, 200, 180, 360, true);
			arcs.Close();

			var arcSegmentIndex = FindFirstArcSegmentIndex(arcs);
			Action run = scenario switch
			{
				"clip-polygon" => () => ClipPath(context.Canvas, polygon),
				"clip-arcs" => () => ClipPath(context.Canvas, arcs),
				"fill-arcs" => () => context.Canvas.FillPath(arcs, WindingMode.NonZero),
				"draw-arcs" => () => context.Canvas.DrawPath(arcs),
				"segment-arc" => () =>
				{
					using var path = arcs.AsAndroidPathFromSegment(arcSegmentIndex, 1, 1);
				},
				"rotated-arcs" => () =>
				{
					using var path = arcs.AsRotatedAndroidPath(new PointF(110, 110), 1, 1, 45);
				},
				_ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "Unknown drawing scenario.")
			};

			run();
			var existingPeers = GetPathPeers();
			for (var i = 0; i < 20; i++)
				run();

			var additionalPeers = GetPathPeers();
			additionalPeers.ExceptWith(existingPeers);
			try
			{
				Assert.Empty(additionalPeers);
			}
			finally
			{
				foreach (var peer in additionalPeers)
					peer.Dispose();
			}
		});
	}

	static void ClipPath(ICanvas canvas, PathF path)
	{
		canvas.SaveState();
		try
		{
			canvas.ClipPath(path);
			canvas.FillRectangle(20, 20, 180, 180);
		}
		finally
		{
			canvas.RestoreState();
		}
	}

	static int FindFirstArcSegmentIndex(PathF path)
	{
		var index = 0;
		foreach (var segmentType in path.SegmentTypes)
		{
			if (segmentType == PathOperation.Arc)
				return index;

			index++;
		}

		throw new InvalidOperationException("The test path does not contain an arc segment.");
	}

	static HashSet<IJavaPeerable> GetPathPeers()
	{
		var peers = new HashSet<IJavaPeerable>(ReferenceEqualityComparer.Instance);
		foreach (var entry in JniEnvironment.Runtime.ValueManager.GetSurfacedPeers())
		{
			if (entry.SurfacedPeer.TryGetTarget(out var peer) &&
				peer.PeerReference.IsValid &&
				peer is global::Android.Graphics.Path or global::Android.Graphics.RectF)
			{
				peers.Add(peer);
			}
		}

		return peers;
	}
}
