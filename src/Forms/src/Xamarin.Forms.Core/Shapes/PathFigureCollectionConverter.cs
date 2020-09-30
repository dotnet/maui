using System;
using System.Globalization;
using Xamarin.Forms;

namespace Xamarin.Forms.Shapes
{
	public class PathFigureCollectionConverter : TypeConverter
	{
		const bool AllowSign = true;
		const bool AllowComma = true;

		static bool _figureStarted;
		static string _pathString;
		static int _pathLength;
		static int _curIndex;
		static Point _lastStart;
		static Point _lastPoint;
		static Point _secondLastPoint;
		static char _token;

		public override object ConvertFromInvariantString(string value)
		{
			PathFigureCollection pathFigureCollection = new PathFigureCollection();

			ParseStringToPathFigureCollection(pathFigureCollection, value);

			return pathFigureCollection;
		}

		public static void ParseStringToPathFigureCollection(PathFigureCollection pathFigureCollection, string pathString)
		{
			if (pathString != null)
			{
				int curIndex = 0;

				while ((curIndex < pathString.Length) && char.IsWhiteSpace(pathString, curIndex))
				{
					curIndex++;
				}

				if (curIndex < pathString.Length)
				{
					if (pathString[curIndex] == 'F')
					{
						curIndex++;

						while ((curIndex < pathString.Length) && char.IsWhiteSpace(pathString, curIndex))
						{
							curIndex++;
						}

						// If we ran out of text, this is an error, because 'F' cannot be specified without 0 or 1
						// Also, if the next token isn't 0 or 1, this too is illegal
						if ((curIndex == pathString.Length) ||
							((pathString[curIndex] != '0') &&
							 (pathString[curIndex] != '1')))
						{
							throw new FormatException("IllegalToken");
						}

						// Increment curIndex to point to the next char
						curIndex++;
					}
				}

				ParseToPathFigureCollection(pathFigureCollection, pathString, curIndex);
			}
		}

		static void ParseToPathFigureCollection(PathFigureCollection pathFigureCollection, string pathString, int startIndex)
		{
			PathFigure pathFigure = null;

			_pathString = pathString;
			_pathLength = pathString.Length;
			_curIndex = startIndex;

			_secondLastPoint = new Point(0, 0);
			_lastPoint = new Point(0, 0);
			_lastStart = new Point(0, 0);

			_figureStarted = false;

			bool first = true;

			char last_cmd = ' ';

			while (ReadToken()) // Empty path is allowed in XAML
			{
				char cmd = _token;

				if (first)
				{
					if ((cmd != 'M') && (cmd != 'm'))  // Path starts with M|m
					{
						ThrowBadToken();
					}

					first = false;
				}

				switch (cmd)
				{
					case 'm':
					case 'M':
						// XAML allows multiple points after M/m
						_lastPoint = ReadPoint(cmd, !AllowComma);

						pathFigure = new PathFigure
						{
							StartPoint = _lastPoint
						};
						pathFigureCollection.Add(pathFigure);

						_figureStarted = true;
						_lastStart = _lastPoint;
						last_cmd = 'M';

						while (IsNumber(AllowComma))
						{
							_lastPoint = ReadPoint(cmd, !AllowComma);

							LineSegment lineSegment = new LineSegment
							{
								Point = _lastPoint
							};
							pathFigure.Segments.Add(lineSegment);

							last_cmd = 'L';
						}
						break;

					case 'l':
					case 'L':
					case 'h':
					case 'H':
					case 'v':
					case 'V':
						EnsureFigure();

						do
						{
							switch (cmd)
							{
								case 'l':
									_lastPoint = ReadPoint(cmd, !AllowComma);
									break;
								case 'L':
									_lastPoint = ReadPoint(cmd, !AllowComma);
									break;
								case 'h':
									_lastPoint.X += ReadNumber(!AllowComma);
									break;
								case 'H':
									_lastPoint.X = ReadNumber(!AllowComma);
									break;
								case 'v':
									_lastPoint.Y += ReadNumber(!AllowComma);
									break;
								case 'V':
									_lastPoint.Y = ReadNumber(!AllowComma);
									break;
							}

							pathFigure.Segments.Add(new LineSegment
							{
								Point = _lastPoint
							});
						}
						while (IsNumber(AllowComma));

						last_cmd = 'L';
						break;

					case 'c':
					case 'C': // Cubic Bezier
					case 's':
					case 'S': // Smooth cublic Bezier
						EnsureFigure();

						do
						{
							Point p;

							if ((cmd == 's') || (cmd == 'S'))
							{
								if (last_cmd == 'C')
								{
									p = Reflect();
								}
								else
								{
									p = _lastPoint;
								}

								_secondLastPoint = ReadPoint(cmd, !AllowComma);
							}
							else
							{
								p = ReadPoint(cmd, !AllowComma);

								_secondLastPoint = ReadPoint(cmd, AllowComma);
							}

							_lastPoint = ReadPoint(cmd, AllowComma);

							BezierSegment bezierSegment = new BezierSegment
							{
								Point1 = p,
								Point2 = _secondLastPoint,
								Point3 = _lastPoint
							};

							pathFigure.Segments.Add(bezierSegment);

							last_cmd = 'C';
						}
						while (IsNumber(AllowComma));

						break;

					case 'q':
					case 'Q': // Quadratic Bezier
					case 't':
					case 'T': // Smooth quadratic Bezier
						EnsureFigure();

						do
						{
							if ((cmd == 't') || (cmd == 'T'))
							{
								if (last_cmd == 'Q')
								{
									_secondLastPoint = Reflect();
								}
								else
								{
									_secondLastPoint = _lastPoint;
								}

								_lastPoint = ReadPoint(cmd, !AllowComma);
							}
							else
							{
								_secondLastPoint = ReadPoint(cmd, !AllowComma);
								_lastPoint = ReadPoint(cmd, AllowComma);
							}

							QuadraticBezierSegment quadraticBezierSegment = new QuadraticBezierSegment
							{
								Point1 = _secondLastPoint,
								Point2 = _lastPoint
							};

							pathFigure.Segments.Add(quadraticBezierSegment);

							last_cmd = 'Q';
						}
						while (IsNumber(AllowComma));

						break;

					case 'a':
					case 'A':
						EnsureFigure();

						do
						{
							// A 3,4 5, 0, 0, 6,7
							double w = ReadNumber(!AllowComma);
							double h = ReadNumber(AllowComma);
							double rotation = ReadNumber(AllowComma);
							bool large = ReadBool();
							bool sweep = ReadBool();

							_lastPoint = ReadPoint(cmd, AllowComma);

							ArcSegment arcSegment = new ArcSegment
							{
								Size = new Size(w, h),
								RotationAngle = rotation,
								IsLargeArc = large,
								SweepDirection = sweep ? SweepDirection.Clockwise : SweepDirection.CounterClockwise,
								Point = _lastPoint
							};

							pathFigure.Segments.Add(arcSegment);
						}
						while (IsNumber(AllowComma));

						last_cmd = 'A';
						break;

					case 'z':
					case 'Z':
						EnsureFigure();
						pathFigure.IsClosed = true;
						_figureStarted = false;
						last_cmd = 'Z';

						_lastPoint = _lastStart; // Set reference point to be first point of current figure
						break;

					default:
						ThrowBadToken();
						break;
				}
			}
		}

		static void EnsureFigure()
		{
			if (!_figureStarted)
				_figureStarted = true;
		}

		static Point Reflect()
		{
			return new Point(
				2 * _lastPoint.X - _secondLastPoint.X,
				2 * _lastPoint.Y - _secondLastPoint.Y);
		}

		static bool More()
		{
			return _curIndex < _pathLength;
		}

		static bool SkipWhiteSpace(bool allowComma)
		{
			bool commaMet = false;

			while (More())
			{
				char ch = _pathString[_curIndex];

				switch (ch)
				{
					case ' ':
					case '\n':
					case '\r':
					case '\t':
						break;

					case ',':
						if (allowComma)
						{
							commaMet = true;
							allowComma = false; // One comma only
						}
						else
						{
							ThrowBadToken();
						}
						break;

					default:
						// Avoid calling IsWhiteSpace for ch in (' ' .. 'z']
						if (((ch > ' ') && (ch <= 'z')) || !char.IsWhiteSpace(ch))
						{
							return commaMet;
						}
						break;
				}

				_curIndex++;
			}

			return commaMet;
		}

		static bool ReadBool()
		{
			SkipWhiteSpace(AllowComma);

			if (More())
			{
				_token = _pathString[_curIndex++];

				if (_token == '0')
				{
					return false;
				}
				else if (_token == '1')
				{
					return true;
				}
			}

			ThrowBadToken();

			return false;
		}

		static bool ReadToken()
		{
			SkipWhiteSpace(!AllowComma);

			// Check for end of string
			if (More())
			{
				_token = _pathString[_curIndex++];

				return true;
			}
			else
			{
				return false;
			}
		}

		static void ThrowBadToken()
		{
			throw new FormatException(string.Format("UnexpectedToken \"{0}\" into {1}", _pathString, _curIndex - 1));
		}

		static Point ReadPoint(char cmd, bool allowcomma)
		{
			double x = ReadNumber(allowcomma);
			double y = ReadNumber(AllowComma);

			if (cmd >= 'a') // 'A' < 'a'. lower case for relative
			{
				x += _lastPoint.X;
				y += _lastPoint.Y;
			}

			return new Point(x, y);
		}

		static bool IsNumber(bool allowComma)
		{
			bool commaMet = SkipWhiteSpace(allowComma);

			if (More())
			{
				_token = _pathString[_curIndex];

				// Valid start of a number
				if ((_token == '.') || (_token == '-') || (_token == '+') || ((_token >= '0') && (_token <= '9'))
					|| (_token == 'I')  // Infinity
					|| (_token == 'N')) // NaN
				{
					return true;
				}
			}

			if (commaMet) // Only allowed between numbers
			{
				ThrowBadToken();
			}

			return false;
		}

		static double ReadNumber(bool allowComma)
		{
			if (!IsNumber(allowComma))
			{
				ThrowBadToken();
			}

			bool simple = true;
			int start = _curIndex;

			// Allow for a sign
			// 
			// There are numbers that cannot be preceded with a sign, for instance, -NaN, but it's
			// fine to ignore that at this point, since the CLR parser will catch this later.
			if (More() && ((_pathString[_curIndex] == '-') || _pathString[_curIndex] == '+'))
			{
				_curIndex++;
			}

			// Check for Infinity (or -Infinity).
			if (More() && (_pathString[_curIndex] == 'I'))
			{
				// Don't bother reading the characters, as the CLR parser will
				// do this for us later.
				_curIndex = Math.Min(_curIndex + 8, _pathLength); // "Infinity" has 8 characters
				simple = false;
			}
			// Check for NaN
			else if (More() && (_pathString[_curIndex] == 'N'))
			{
				//
				// Don't bother reading the characters, as the CLR parser will
				// do this for us later.
				//
				_curIndex = Math.Min(_curIndex + 3, _pathLength); // "NaN" has 3 characters
				simple = false;
			}
			else
			{
				SkipDigits(!AllowSign);

				// Optional period, followed by more digits
				if (More() && (_pathString[_curIndex] == '.'))
				{
					simple = false;
					_curIndex++;
					SkipDigits(!AllowSign);
				}

				// Exponent
				if (More() && ((_pathString[_curIndex] == 'E') || (_pathString[_curIndex] == 'e')))
				{
					simple = false;
					_curIndex++;
					SkipDigits(AllowSign);
				}
			}

			if (simple && (_curIndex <= (start + 8))) // 32-bit integer
			{
				int sign = 1;

				if (_pathString[start] == '+')
				{
					start++;
				}
				else if (_pathString[start] == '-')
				{
					start++;
					sign = -1;
				}

				int value = 0;

				while (start < _curIndex)
				{
					value = value * 10 + (_pathString[start] - '0');
					start++;
				}

				return value * sign;
			}
			else
			{
				string subString = _pathString.Substring(start, _curIndex - start);

				try
				{
					return Convert.ToDouble(subString, CultureInfo.InvariantCulture);
				}
				catch (FormatException)
				{
					throw new FormatException(string.Format("UnexpectedToken \"{0}\" into {1}", start, _pathString));
				}
			}
		}

		static void SkipDigits(bool signAllowed)
		{
			// Allow for a sign
			if (signAllowed && More() && ((_pathString[_curIndex] == '-') || _pathString[_curIndex] == '+'))
			{
				_curIndex++;
			}

			while (More() && (_pathString[_curIndex] >= '0') && (_pathString[_curIndex] <= '9'))
			{
				_curIndex++;
			}
		}
	}
}