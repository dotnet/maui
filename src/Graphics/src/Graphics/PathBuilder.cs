using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Provides functionality for constructing path objects from string definitions.
	/// </summary>
	public class PathBuilder
	{
		/// <summary>
		/// Builds a path from a string definition.
		/// </summary>
		/// <param name="definition">The string definition of the path using SVG-like path commands.</param>
		/// <returns>A new <see cref="PathF"/> object representing the defined path.</returns>
		/// <remarks>Returns an empty path if the definition is null or empty.</remarks>
		public static PathF Build(string definition)
		{
			if (string.IsNullOrEmpty(definition))
				return new PathF();

			var pathBuilder = new PathBuilder();
			var path = pathBuilder.BuildPath(definition);
			return path;
		}

		private readonly Stack<string> _commandStack = new Stack<string>();
		private bool _closeWhenDone;
		private char _lastCommand = '~';
		private PointF? _lastCurveControlPoint;
		private PointF? _lastMoveTo;

		private PathF _path;
		private PointF? _relativePoint;

		private bool NextBoolValue
		{
			get
			{
				string vValueAsString = _commandStack.Pop();

				if ("1".Equals(vValueAsString, StringComparison.Ordinal))
				{
					return true;
				}

				return false;
			}
		}

		private float NextValue
		{
			get
			{
				string vValueAsString = _commandStack.Pop();
				try
				{
					return ParseFloat(vValueAsString);
				}
				catch (Exception exc)
				{
					throw new Exception("Error parsing a path value.", exc);
				}
			}
		}

		/// <summary>
		/// Parses a string value as a float using invariant culture.
		/// </summary>
		/// <param name="value">The string representation of a number.</param>
		/// <returns>The float value parsed from the string.</returns>
		/// <exception cref="Exception">Thrown when the string cannot be parsed as a float.</exception>
		/// <remarks>Handles special cases like Illustrator's malformed number formats (e.g., "5.96.88").</remarks>
		public static float ParseFloat(string value)
		{
			if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
			{
				return number;
			}

			// Note: Illustrator will sometimes export numbers that look like "5.96.88", so we need to be able to handle them
			var split = value.Split(new[] { '.' });
			if (split.Length > 2)
			{
				if (float.TryParse($"{split[0]}.{split[1]}", NumberStyles.Any, CultureInfo.InvariantCulture, out number))
				{
					return number;
				}
			}

			string stringValue = GetNumbersOnly(value);
			if (float.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out number))
			{
				return number;
			}

			throw new Exception($"Error parsing {value} as a float.");
		}

		private static string GetNumbersOnly(string value)
		{
			var builder = new StringBuilder(value.Length);
			foreach (char c in value)
			{
				if (char.IsDigit(c) || c == '.' || c == '-')
				{
					builder.Append(c);
				}
			}

			return builder.ToString();
		}

		/// <summary>
		/// Builds a path from a string definition.
		/// </summary>
		/// <param name="pathAsString">The string definition of the path using SVG-like path commands.</param>
		/// <returns>A <see cref="PathF"/> object representing the defined path.</returns>
		/// <exception cref="Exception">Thrown when there's an error parsing the path commands.</exception>
		public PathF BuildPath(string pathAsString)
		{
			try
			{
				_lastCommand = '~';
				_lastCurveControlPoint = null;
				_path = null;
				_commandStack.Clear();
				_relativePoint = new PointF(0, 0);
				_closeWhenDone = false;

#if DEBUG_PATH
				System.Diagnostics.Debug.WriteLine(aPathString);
#endif
#if NETSTANDARD2_0
				pathAsString = pathAsString.Replace("Infinity", "0");
#else
				pathAsString = pathAsString.Replace("Infinity", "0", StringComparison.Ordinal);
#endif
				pathAsString = SeparateLetterCharsWithSpaces(pathAsString);
#if NETSTANDARD2_0
				pathAsString = pathAsString.Replace("-", " -");
				pathAsString = pathAsString.Replace(" E  -", "E-");
				pathAsString = pathAsString.Replace(" e  -", "e-");
#else
				pathAsString = pathAsString.Replace("-", " -", StringComparison.Ordinal);
				pathAsString = pathAsString.Replace(" E  -", "E-", StringComparison.Ordinal);
				pathAsString = pathAsString.Replace(" e  -", "e-", StringComparison.Ordinal);
#endif
#if DEBUG_PATH
				System.Diagnostics.Debug.WriteLine(aPathString);
#endif
				string[] args = pathAsString.Split(new[] { ' ', '\r', '\n', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
				for (int i = args.Length - 1; i >= 0; i--)
				{
					string entry = args[i];
					char c = entry[0];
					if (char.IsLetter(c))
					{
						if (entry.Length > 1)
						{
							entry = entry.Substring(1);
							if (char.IsLetter(entry[0]))
							{
								if (entry.Length > 1)
								{
									_commandStack.Push(entry.Substring(1));
#if DEBUG_PATH
										System.Diagnostics.Debug.WriteLine(vEntry.Substring(1));
#endif
								}

								_commandStack.Push(entry[0].ToInvariantString());
#if DEBUG_PATH
								 System.Diagnostics.Debug.WriteLine(vEntry[0].ToString());
#endif
							}
							else
							{
								_commandStack.Push(entry);
#if DEBUG_PATH
								System.Diagnostics.Debug.WriteLine(vEntry);
#endif
							}
						}

						_commandStack.Push(c.ToInvariantString());
#if DEBUG_PATH
						System.Diagnostics.Debug.WriteLine(vChar.ToString());
#endif
					}
					else
					{
						_commandStack.Push(entry);
#if DEBUG_PATH
						System.Diagnostics.Debug.WriteLine(vEntry);
#endif
					}
				}

				while (_commandStack.Count > 0)
				{
					if (_path == null)
					{
						_path = new PathF();
					}

					string topCommand = _commandStack.Pop();
					var firstLetter = topCommand[0];

					if (IsCommand(firstLetter))
						HandleCommand(topCommand);
					else
					{
						_commandStack.Push(topCommand);
						HandleCommand(_lastCommand.ToString());
					}
				}

				if (_path != null && !_path.Closed)
				{
					if (_closeWhenDone)
					{
						_path.Close();
					}
				}
			}
			catch (Exception exc)
			{
				System.Diagnostics.Debug.WriteLine("=== An error occurred parsing the path. ===", exc);
				System.Diagnostics.Debug.WriteLine(pathAsString);
#if DEBUG
				throw;
#endif
			}

			static string SeparateLetterCharsWithSpaces(string input)
			{
				var sb = new StringBuilder(input.Length, maxCapacity: 3 * input.Length);
				foreach (var character in input)
				{
					if (char.IsLetter(character))
					{
						sb.Append(' ');
						sb.Append(character);
						sb.Append(' ');
					}
					else
					{
						sb.Append(character);
					}
				}
				return sb.ToString();
			}

			return _path;
		}

		private bool IsCommand(char firstLetter)
		{
			if (char.IsDigit(firstLetter))
				return false;

			if (firstLetter == '.')
				return false;

			if (firstLetter == '-')
				return false;

			if (firstLetter == 'e' || firstLetter == 'E')
				return false;

			return true;
		}

		private void HandleCommand(string command)
		{
			char c = command[0];

			if (_lastCommand != '~' && (char.IsDigit(c) || c == '-'))
			{
				var previousCommand = _commandStack.Peek()?[0];

				if (_lastCommand == 'M')
				{
					_commandStack.Push(command);
					HandleCommand('L');
				}
				else if (_lastCommand == 'm')
				{
					_commandStack.Push(command);
					HandleCommand('l');
				}
				else if (_lastCommand == 'L')
				{
					_commandStack.Push(command);
					HandleCommand('L');
				}
				else if (_lastCommand == 'l')
				{
					_commandStack.Push(command);
					HandleCommand('l');
				}
				else if (_lastCommand == 'H')
				{
					_commandStack.Push(command);
					HandleCommand('H');
				}
				else if (_lastCommand == 'h')
				{
					_commandStack.Push(command);
					HandleCommand('h');
				}
				else if (_lastCommand == 'V')
				{
					_commandStack.Push(command);
					HandleCommand('V');
				}
				else if (_lastCommand == 'v')
				{
					_commandStack.Push(command);
					HandleCommand('v');
				}
				else if (_lastCommand == 'C')
				{
					_commandStack.Push(command);
					HandleCommand('C');
				}
				else if (_lastCommand == 'c')
				{
					_commandStack.Push(command);
					HandleCommand('c');
				}
				else if (_lastCommand == 'S')
				{
					_commandStack.Push(command);
					HandleCommand('S');
				}
				else if (_lastCommand == 's')
				{
					_commandStack.Push(command);
					HandleCommand('s');
				}
				else if (_lastCommand == 'Q')
				{
					_commandStack.Push(command);
					HandleCommand('Q');
				}
				else if (_lastCommand == 'q')
				{
					_commandStack.Push(command);
					HandleCommand('q');
				}
				else if (_lastCommand == 'T')
				{
					_commandStack.Push(command);
					HandleCommand('T', previousCommand);
				}
				else if (_lastCommand == 't')
				{
					_commandStack.Push(command);
					HandleCommand('t', previousCommand);
				}
				else if (_lastCommand == 'A')
				{
					_commandStack.Push(command);
					HandleCommand('A');
				}
				else if (_lastCommand == 'a')
				{
					_commandStack.Push(command);
					HandleCommand('a');
				}
				else
				{
					System.Diagnostics.Debug.WriteLine("Don't know how to handle the path command: " + command);
				}
			}
			else
			{
				HandleCommand(c);
			}
		}

		private void HandleCommand(char command, char? previousCommand = null)
		{
			if (command == 'M')
			{
				MoveTo(false);
			}
			else if (command == 'm')
			{
				MoveTo(true);
				if (_lastCommand == '~')
				{
					command = 'm';
				}
			}
			else if (command == 'z' || command == 'Z')
			{
				ClosePath();
			}
			else if (command == 'L')
			{
				LineTo(false);
			}
			else if (command == 'l')
			{
				LineTo(true);
			}
			else if (command == 'Q')
			{
				QuadTo(false);
			}
			else if (command == 'q')
			{
				QuadTo(true);
			}
			else if (command == 'T')
			{
				ReflectiveQuadTo(false, previousCommand);
			}
			else if (command == 't')
			{
				ReflectiveQuadTo(true, previousCommand);
			}
			else if (command == 'C')
			{
				CurveTo(false);
			}
			else if (command == 'c')
			{
				CurveTo(true);
			}
			else if (command == 'S')
			{
				SmoothCurveTo(false);
			}
			else if (command == 's')
			{
				SmoothCurveTo(true);
			}
			else if (command == 'A')
			{
				ArcTo(false);
			}
			else if (command == 'a')
			{
				ArcTo(true);
			}
			else if (command == 'H')
			{
				HorizontalLineTo(false);
			}
			else if (command == 'h')
			{
				HorizontalLineTo(true);
			}
			else if (command == 'V')
			{
				VerticalLineTo(false);
			}
			else if (command == 'v')
			{
				VerticalLineTo(true);
			}
			else
			{
				System.Diagnostics.Debug.WriteLine("Don't know how to handle the path command: " + command);
			}

			if (!(command == 'C' || command == 'c' || command == 's' || command == 'S'))
			{
				_lastCurveControlPoint = null;
			}

			_lastCommand = command;
		}

		private void ClosePath()
		{
			_path.Close();
			_relativePoint = _lastMoveTo;

			/*int vSegments = path.SegmentCount;
		 if (vSegments >= 2)
		 {
			if (path.GetSegmentType(vSegments-2) == PathOperation.MOVE_TO && path.GetSegmentType(vSegments-1) == PathOperation.CLOSE)
			{
			   path.RemoveAllSegmentsAfter(vSegments-2);
			}
		 }*/
		}

		private void MoveTo(bool isRelative)
		{
			if (_path.SubPathCount == 1)
			{
				if (_path.FirstPoint.Equals(_path.LastPoint))
				{
					_closeWhenDone = true;
				}
			}

			var xOffset = NextValue;
			var yOffset = NextValue;
			var point = NewPoint(xOffset, yOffset, isRelative, true);
			_path.MoveTo(point);
			_lastMoveTo = point;
		}

		private void LineTo(bool isRelative)
		{
			var point = NewPoint(NextValue, NextValue, isRelative, true);
			_path.LineTo(point);
		}

		private void HorizontalLineTo(bool isRelative)
		{
			var point = NewHorizontalPoint(NextValue, isRelative, true);
			_path.LineTo(point);
		}

		private void VerticalLineTo(bool isRelative)
		{
			var point = NewVerticalPoint(NextValue, isRelative, true);
			_path.LineTo(point);
		}

		private void CurveTo(bool isRelative)
		{
			var point1 = NewPoint(NextValue, NextValue, isRelative, false);
			float x = NextValue;
			float y = NextValue;

			bool isQuad = char.IsLetter(_commandStack.Peek()[0]);
			var point2 = NewPoint(x, y, isRelative, isQuad);

			if (isQuad)
			{
				_path.QuadTo(point1, point2);
				_lastCurveControlPoint = point1;
			}
			else
			{
				var point3 = NewPoint(NextValue, NextValue, isRelative, true);
				_path.CurveTo(point1, point2, point3);
				_lastCurveControlPoint = point2;
				//System.Diagnostics.Debug.WriteLine($"CurveTo({point1.X},{point1.Y},{point2.X},{point2.Y},{point3.X},{point3.Y})");
			}
		}

		private void QuadTo(bool isRelative)
		{
			var point1 = NewPoint(NextValue, NextValue, isRelative, false);
			var point2 = NewPoint(NextValue, NextValue, isRelative, true);
			_lastCurveControlPoint = point1;
			_path.QuadTo(point1, point2);
		}

		private void ReflectiveQuadTo(bool isRelative, char? previousCommand)
		{
			var lastPoint = _path.LastPoint;
			var point1 = lastPoint;
			var lastCurveControlPoint = _lastCurveControlPoint ?? default;
			switch (previousCommand)
			{
				case 'Q':
				case 'q':
				case 'T':
				case 't':
					var dx = lastPoint.X - lastCurveControlPoint.X;
					var dy = lastPoint.Y - lastCurveControlPoint.Y;
					point1 = point1.Offset(dx, dy);
					break;
			}
			var point2 = NewPoint(NextValue, NextValue, isRelative, true);
			_lastCurveControlPoint = point1;
			_path.QuadTo(point1, point2);
		}

		private void SmoothCurveTo(bool isRelative)
		{
			PointF? point1 = null;
			var point2 = NewPoint(NextValue, NextValue, isRelative, false);

			// ReSharper disable ConvertIfStatementToNullCoalescingExpression
			if (_lastCurveControlPoint == null && _relativePoint != null)
			{
				// ReSharper restore ConvertIfStatementToNullCoalescingExpression
				point1 = GeometryUtil.GetOppositePoint((PointF)_relativePoint, point2);
			}
			else if (_relativePoint != null && _lastCurveControlPoint != null)
			{
				point1 = GeometryUtil.GetOppositePoint((PointF)_relativePoint, (PointF)_lastCurveControlPoint);
			}

			var point3 = NewPoint(NextValue, NextValue, isRelative, true);
			if (point1 != null)
				_path.CurveTo((PointF)point1, point2, point3);
			_lastCurveControlPoint = point2;
		}

		private void ArcTo(bool isRelative)
		{
			var startPoint = _relativePoint ?? default;

			var rx = NextValue;
			var ry = NextValue;

			var r = NextValue;
			var largeArcFlag = NextBoolValue;
			var sweepFlag = NextBoolValue;
			var endPoint = NewPoint(NextValue, NextValue, isRelative, false);

			var arcPath = new PathF(startPoint);
			arcPath.SVGArcTo(rx, ry, r, largeArcFlag, sweepFlag, endPoint.X, endPoint.Y, startPoint.X, startPoint.Y);

			for (int s = 0; s < arcPath.OperationCount; s++)
			{
				var segmentType = arcPath.GetSegmentType(s);
				var pointsInSegment = arcPath.GetPointsForSegment(s);

				if (segmentType == PathOperation.Move)
				{
					// do nothing
				}
				else if (segmentType == PathOperation.Line)
				{
					_path.LineTo(pointsInSegment[0]);
				}
				else if (segmentType == PathOperation.Cubic)
				{
					_path.CurveTo(pointsInSegment[0], pointsInSegment[1], pointsInSegment[2]);
				}
				else if (segmentType == PathOperation.Quad)
				{
					_path.QuadTo(pointsInSegment[0], pointsInSegment[1]);
				}
			}

			_relativePoint = _path.LastPoint;
		}

		private PointF NewPoint(float x, float y, bool isRelative, bool isReference)
		{
			PointF point = default;

			if (isRelative && _relativePoint != null)
			{
				point = new PointF(((PointF)_relativePoint).X + x, ((PointF)_relativePoint).Y + y);
			}
			else
			{
				point = new PointF(x, y);
			}

			// If this is the reference point, we want to store the location before
			// we translate it into the final coordinates.  This way, future relative
			// points will start from an un-translated position.
			if (isReference)
			{
				_relativePoint = point;
			}

			return point;
		}

		private PointF NewVerticalPoint(float y, bool isRelative, bool isReference)
		{
			PointF point = default;

			if (isRelative && _relativePoint != null)
			{
				point = new PointF(((PointF)_relativePoint).X, ((PointF)_relativePoint).Y + y);
			}
			else if (_relativePoint != null)
			{
				point = new PointF(((PointF)_relativePoint).X, y);
			}

			// If this is the reference point, we want to store the location before
			// we translate it into the final coordinates.  This way, future relative
			// points will start from an un-translated position.
			if (isReference)
			{
				_relativePoint = point;
			}

			return point;
		}

		private PointF NewHorizontalPoint(float x, bool isRelative, bool isReference)
		{
			PointF point = default;

			if (isRelative && _relativePoint != null)
			{
				point = new PointF(((PointF)_relativePoint).X + x, ((PointF)_relativePoint).Y);
			}
			else if (_relativePoint != null)
			{
				point = new PointF(x, ((PointF)_relativePoint).Y);
			}

			// If this is the reference point, we want to store the location before
			// we translate it into the final coordinates.  This way, future relative
			// points will start from an un-translated position.
			if (isReference)
			{
				_relativePoint = point;
			}

			return point;
		}
	}
}
