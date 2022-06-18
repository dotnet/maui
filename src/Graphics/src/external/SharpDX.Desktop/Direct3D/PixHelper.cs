// Copyright (c) 2010-2014 SharpDX - Alexandre Mutel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System.Runtime.InteropServices;
using SharpDX.Mathematics.Interop;

namespace SharpDX.Desktop.Direct3D
{
	/// <summary>
	/// Helper class for PIX.
	/// </summary>
	public static class PixHelper
	{
		/// <summary>
		/// Marks the beginning of a user-defined event. PIX can use this event to trigger an action.
		/// </summary>
		/// <param name="color">The Event color.</param>
		/// <param name="name">The Event Name.</param>
		/// <returns>The zero-based level of the hierarchy that this event is starting in. If an error occurs, the return value will be negative.</returns>
		/// <unmanaged>D3DPERF_BeginEvent</unmanaged>
		public static int BeginEvent(RawColorBGRA color, string name)
		{
			return D3DPERF_BeginEvent(color, name);
		}

		/// <summary>
		/// Marks the beginning of a user-defined event. PIX can use this event to trigger an action.
		/// </summary>
		/// <param name="color">The Event color.</param>
		/// <param name="name">The Event formatted Name.</param>
		/// <param name="parameters">The parameters to use for the formatted name.</param>
		/// <returns>
		/// The zero-based level of the hierarchy that this event is starting in. If an error occurs, the return value will be negative.
		/// </returns>
		/// <unmanaged>D3DPERF_BeginEvent</unmanaged>
		public static int BeginEvent(RawColorBGRA color, string name, params object[] parameters)
		{
			return D3DPERF_BeginEvent(color, string.Format(name, parameters));
		}

		/// <summary>
		/// Mark the end of a user-defined event. PIX can use this event to trigger an action.
		/// </summary>
		/// <returns>The level of the hierarchy in which the event is ending. If an error occurs, this value is negative.</returns>
		/// <unmanaged>D3DPERF_EndEvent</unmanaged>
		public static int EndEvent()
		{
			return D3DPERF_EndEvent();
		}

		/// <summary>
		/// Mark an instantaneous event. PIX can use this event to trigger an action.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <param name="name">The name.</param>
		/// <unmanaged>D3DPERF_SetMarker</unmanaged>
		public static void SetMarker(RawColorBGRA color, string name)
		{
			D3DPERF_SetMarker(color, name);
		}

		/// <summary>
		/// Mark an instantaneous event. PIX can use this event to trigger an action.
		/// </summary>
		/// <param name="color">The color.</param>
		/// <param name="name">The name to format.</param>
		/// <param name="parameters">The parameters to use to format the name.</param>
		/// <unmanaged>D3DPERF_SetMarker</unmanaged>
		public static void SetMarker(RawColorBGRA color, string name, params object[] parameters)
		{
			D3DPERF_SetMarker(color, string.Format(name, parameters));
		}

		/// <summary>
		/// Set this to false to notify PIX that the target program does not give permission to be profiled.
		/// </summary>
		/// <param name="enableFlag">if set to <c>true</c> PIX profiling is authorized. Default to true.</param>
		/// <unmanaged>D3DPERF_SetOptions</unmanaged>
		public static void AllowProfiling(bool enableFlag)
		{
			D3DPERF_SetOptions(enableFlag ? 0 : 1);
		}

		/// <summary>
		/// Gets a value indicating whether this instance is currently profiled by PIX.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is currently profiled; otherwise, <c>false</c>.
		/// </value>
		/// <unmanaged>D3DPERF_GetStatus</unmanaged>
		public static bool IsCurrentlyProfiled => D3DPERF_GetStatus() != 0;

		[DllImport("d3d9.dll", EntryPoint = "D3DPERF_BeginEvent", CharSet = CharSet.Unicode)]
		private extern static int D3DPERF_BeginEvent(RawColorBGRA color, string name);

		[DllImport("d3d9.dll", EntryPoint = "D3DPERF_EndEvent", CharSet = CharSet.Unicode)]
		private extern static int D3DPERF_EndEvent();

		[DllImport("d3d9.dll", EntryPoint = "D3DPERF_SetMarker", CharSet = CharSet.Unicode)]
		private extern static void D3DPERF_SetMarker(RawColorBGRA color, string wszName);

		[DllImport("d3d9.dll", EntryPoint = "D3DPERF_SetOptions", CharSet = CharSet.Unicode)]
		private extern static void D3DPERF_SetOptions( int options);

		[DllImport("d3d9.dll", EntryPoint = "D3DPERF_GetStatus", CharSet = CharSet.Unicode)]
		private extern static int D3DPERF_GetStatus();
	}
}
