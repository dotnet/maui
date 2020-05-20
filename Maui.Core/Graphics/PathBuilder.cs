using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;

namespace System.Maui.Graphics {
	public class PathBuilder
	{
		public static Path Build(string definition)
		{
			if (string.IsNullOrEmpty(definition))
				return new Path();

			var pathBuilder = new PathBuilder();
			var path = pathBuilder.BuildPath(definition);
			return path;
		}

		private readonly Stack<string> _commandStack = new Stack<string>();
		private bool _closeWhenDone;
		private char _lastCommand = '~';

		private Point? _lastCurveControlPoint;
		private Point? _relativePoint;

		private Path _path;

		private float NextValue
		{
			get
			{
				var valueAsString = _commandStack.Pop();
				return ParseFloat(valueAsString);
			}
		}

		public static float ParseFloat(string value)
		{
			if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var number))
				return number;

			// Note: Illustrator will sometimes export numbers that look like "5.96.88", so we need to be able to handle them
			var split = value.Split('.');
			if (split.Length > 2)
				if (float.TryParse($"{split[0]}.{split[1]}", NumberStyles.Any, CultureInfo.InvariantCulture, out number))
					return number;

			var stringValue = GetNumbersOnly(value);
			if (float.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out number))
				return number;

			throw new Exception($"Error parsing {value} as a float.");
		}

		private static string GetNumbersOnly(string value)
		{
			var builder = new StringBuilder(value.Length);
			foreach (var c in value)
				if (char.IsDigit(c) || c == '.' || c == '-')
					builder.Append(c);

			return builder.ToString();
		}

		public Path BuildPath(string pathAsString)
		{
			try
			{
				_lastCommand = '~';
				_lastCurveControlPoint = null;
				_path = null;
				_commandStack.Clear();
				_relativePoint = new Point(0, 0);
				_closeWhenDone = false;

				pathAsString = pathAsString.Replace("Infinity", "0");
				pathAsString = Regex.Replace(pathAsString, "([a-zA-Z])", " $1 ");
				pathAsString = pathAsString.Replace("-", " -");
				pathAsString = pathAsString.Replace(" E  -", "E-");
				pathAsString = pathAsString.Replace(" e  -", "e-");

				var args = pathAsString.Split(new[] { ' ', '\r', '\n', '\t', ',' }, StringSplitOptions.RemoveEmptyEntries);
				for (var i = args.Length - 1; i >= 0; i--)
				{
					var entry = args[i];
					var c = entry[0];
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
								}

								_commandStack.Push(entry[0].ToString(CultureInfo.InvariantCulture));
							}
							else
							{
								_commandStack.Push(entry);
							}
						}

						_commandStack.Push(c.ToString(CultureInfo.InvariantCulture));
					}
					else
					{
						_commandStack.Push(entry);
					}
				}

				while (_commandStack.Count > 0)
				{
					if (_path == null)
					{
						_path = new Path();
					}

					var vCommand = _commandStack.Pop();
					HandleCommand(vCommand);
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
				throw new Exception($"An error occurred parsing the path: {pathAsString}", exc);
			}

			return _path;
		}

		private void HandleCommand(string command)
		{
			var c = command[0];

			if (_lastCommand != '~' && (char.IsDigit(c) || c == '-'))
			{
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
					HandleCommand('T');
				}
				else if (_lastCommand == 't')
				{
					_commandStack.Push(command);
					HandleCommand('t');
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
					Console.WriteLine("Don't know how to handle the path command: " + command);
				}
			}
			else
			{
				HandleCommand(c);
			}
		}

		private void HandleCommand(char command)
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
				Console.WriteLine("Don't know how to handle the path command: " + command);
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
		}

		private void MoveTo(bool isRelative)
		{
			var point = NewPoint(NextValue, NextValue, isRelative, true);
			_path.MoveTo(point);
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
			var point = NewPoint(NextValue, NextValue, isRelative, false);
			var x = NextValue;
			var y = NextValue;

			var isQuad = char.IsLetter(_commandStack.Peek()[0]);
			var point2 = NewPoint(x, y, isRelative, isQuad);

			if (isQuad)
			{
				_path.QuadTo(point, point2);
			}
			else
			{
				var point3 = NewPoint(NextValue, NextValue, isRelative, true);
				_path.CurveTo(point, point2, point3);
				_lastCurveControlPoint = point2;
			}
		}

		private void QuadTo(bool isRelative)
		{
			var point1 = NewPoint(NextValue, NextValue, isRelative, false);
			var x = NextValue;
			var y = NextValue;

			var point2 = NewPoint(x, y, isRelative, true);
			_path.QuadTo(point1, point2);
		}

		private void SmoothCurveTo(bool isRelative)
		{
			var point1 = new Point();
			var point2 = NewPoint(NextValue, NextValue, isRelative, false);

			// ReSharper disable ConvertIfStatementToNullCoalescingExpression
			if (_relativePoint != null)
			{
				if (_lastCurveControlPoint == null)
				{
					// ReSharper restore ConvertIfStatementToNullCoalescingExpression
					point1 = GraphicsOperations.GetOppositePoint((Point)_relativePoint, point2);
				}
				else if (_relativePoint != null)
				{
					point1 = GraphicsOperations.GetOppositePoint((Point)_relativePoint, (Point)_lastCurveControlPoint);
				}
			}

			var point3 = NewPoint(NextValue, NextValue, isRelative, true);
			_path.CurveTo(point1, point2, point3);
			_lastCurveControlPoint = point2;
		}

		private void ArcTo(bool isRelative)
		{
			throw new NotImplementedException();
		}

		private Point NewPoint(float x, float y, bool isRelative, bool isReference)
		{
			Point point;

			if (isRelative && _relativePoint != null)
			{

				point = new Point(((Point)_relativePoint).X + x, ((Point)_relativePoint).Y + y);
			}
			else
			{
				point = new Point(x, y);
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

		private Point NewVerticalPoint(float y, bool isRelative, bool isReference)
		{
			var point = new Point();

			if (isRelative && _relativePoint != null)
			{
				point = new Point(((Point)_relativePoint).X, ((Point)_relativePoint).Y + y);
			}
			else if (_relativePoint != null)
			{
				point = new Point(((Point)_relativePoint).X, y);
			}

			if (isReference)
				_relativePoint = point;

			return point;
		}

		private Point NewHorizontalPoint(float x, bool isRelative, bool isReference)
		{
			var point = new Point();

			if (isRelative && _relativePoint != null)
			{
				point = new Point(((Point)_relativePoint).X + x, ((Point)_relativePoint).Y);
			}
			else if (_relativePoint != null)
			{
				point = new Point(x, ((Point)_relativePoint).Y);
			}

			if (isReference)
				_relativePoint = point;

			return point;
		}
	}
}
