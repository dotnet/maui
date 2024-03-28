// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if NETSTANDARD2_0

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace System;

internal static class MathF
{
	public const float PI = 3.14159265f;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Abs(float value) => Math.Abs(value);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Acos(float d) => (float)Math.Acos(d);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Ceiling(float a) => (float)Math.Ceiling(a);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Cos(float d) => (float)Math.Cos(d);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Pow(float x, float y) => (float)Math.Pow(x, y);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Round(float x, int digits) => (float)Math.Round(x, digits);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Round(float x) => (float)Math.Round(x);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Sin(float a) => (float)Math.Sin(a);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float Sqrt(float d) => (float)Math.Sqrt(d);
}
#endif