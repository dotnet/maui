using System;

namespace Microsoft.Maui.Graphics
{
	public class PaintGradientStop : IComparable<PaintGradientStop>
	{
		private Color _color;
		private float _offset;

		public PaintGradientStop(float offset, Color color)
		{
			_color = color;
			_offset = offset;
		}

		public PaintGradientStop(PaintGradientStop source)
		{
			_color = source._color;
			_offset = source._offset;
		}

		public Color Color
		{
			get => _color;
			set => _color = value;
		}

		public float Offset
		{
			get => _offset;
			set => _offset = value;
		}

		public int CompareTo(PaintGradientStop obj)
		{
			if (_offset < obj._offset)

/* Unmerged change from project 'Graphics(net8.0)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/

/* Unmerged change from project 'Graphics(net8.0-maccatalyst)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/

/* Unmerged change from project 'Graphics(net8.0-windows10.0.19041.0)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/

/* Unmerged change from project 'Graphics(net8.0-windows10.0.20348.0)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/

/* Unmerged change from project 'Graphics(net8.0-macos)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/

/* Unmerged change from project 'Graphics(net7.0)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/

/* Unmerged change from project 'Graphics(net7.0-ios)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/

/* Unmerged change from project 'Graphics(net7.0-maccatalyst)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/

/* Unmerged change from project 'Graphics(net7.0-android)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/

/* Unmerged change from project 'Graphics(net7.0-windows10.0.19041.0)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/

/* Unmerged change from project 'Graphics(net7.0-windows10.0.20348.0)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/

/* Unmerged change from project 'Graphics(net7.0-macos)'
Before:
				return -1;
			if (_offset > obj._offset)
				return 1;
After:
			{
				return -1;
*/
			{
				return -1;
			}

			if (_offset > obj._offset)
			{
				return 1;
			}

			return 0;
		}
	}
}
