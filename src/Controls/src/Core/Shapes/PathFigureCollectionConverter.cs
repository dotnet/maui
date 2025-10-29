using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigureCollectionConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.PathFigureCollectionConverter']/Docs/*" />
	public class PathFigureCollectionConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		const bool AllowSign = true;
		const bool AllowComma = true;

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
		{
			var strValue = value?.ToString();
			PathFigureCollection pathFigureCollection = new PathFigureCollection();

			ParseStringToPathFigureCollection(pathFigureCollection, strValue);

			return pathFigureCollection;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/PathFigureCollectionConverter.xml" path="//Member[@MemberName='ParseStringToPathFigureCollection']/Docs/*" />
		public static void ParseStringToPathFigureCollection(PathFigureCollection pathFigureCollection, string? pathString)
		{
			bool figureStarted = default;
			string? currentPathString = null;
			int pathLength = default;
			int currentIndex = default;
			Point lastStart = default;
			Point lastPoint = default;
			Point secondLastPoint = default;
			char token = default;

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

			void ParseToPathFigureCollection(PathFigureCollection pathFigureCollection, string pathString, int startIndex)
			{
				PathFigure? pathFigure = null;

				currentPathString = pathString;
				pathLength = pathString.Length;
				currentIndex = startIndex;

				secondLastPoint = new Point(0, 0);
				lastPoint = new Point(0, 0);
				lastStart = new Point(0, 0);

				figureStarted = false;

				bool first = true;

				char last_cmd = ' ';

				while (ReadToken()) // Empty path is allowed in XAML
				{
					char cmd = token;

					if (first)
					{
						if ((cmd != 'M') && (cmd != 'm'))  // Path starts with M|m
						{
							throw GetBadTokenException();
						}

						first = false;
					}

					switch (cmd)
					{
						case 'm':
						case 'M':
							// XAML allows multiple points after M/m
							lastPoint = ReadPoint(cmd, !AllowComma);

							pathFigure = new PathFigure
							{
								StartPoint = lastPoint
							};
							pathFigureCollection.Add(pathFigure);

							figureStarted = true;
							lastStart = lastPoint;
							last_cmd = 'M';

							while (IsNumber(AllowComma))
							{
								lastPoint = ReadPoint(cmd, !AllowComma);

								LineSegment lineSegment = new LineSegment
								{
									Point = lastPoint
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
										lastPoint = ReadPoint(cmd, !AllowComma);
										break;
									case 'L':
										lastPoint = ReadPoint(cmd, !AllowComma);
										break;
									case 'h':
										lastPoint.X += ReadNumber(!AllowComma);
										break;
									case 'H':
										lastPoint.X = ReadNumber(!AllowComma);
										break;
									case 'v':
										lastPoint.Y += ReadNumber(!AllowComma);
										break;
									case 'V':
										lastPoint.Y = ReadNumber(!AllowComma);
										break;
								}

								EnsurePathFigure(pathFigure).Segments.Add(new LineSegment
								{
									Point = lastPoint
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
										p = lastPoint;
									}

									secondLastPoint = ReadPoint(cmd, !AllowComma);
								}
								else
								{
									p = ReadPoint(cmd, !AllowComma);

									secondLastPoint = ReadPoint(cmd, AllowComma);
								}

								lastPoint = ReadPoint(cmd, AllowComma);

								BezierSegment bezierSegment = new BezierSegment
								{
									Point1 = p,
									Point2 = secondLastPoint,
									Point3 = lastPoint
								};

								EnsurePathFigure(pathFigure).Segments.Add(bezierSegment);

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
										secondLastPoint = Reflect();
									}
									else
									{
										secondLastPoint = lastPoint;
									}

									lastPoint = ReadPoint(cmd, !AllowComma);
								}
								else
								{
									secondLastPoint = ReadPoint(cmd, !AllowComma);
									lastPoint = ReadPoint(cmd, AllowComma);
								}

								QuadraticBezierSegment quadraticBezierSegment = new QuadraticBezierSegment
								{
									Point1 = secondLastPoint,
									Point2 = lastPoint
								};


								EnsurePathFigure(pathFigure).Segments.Add(quadraticBezierSegment);

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

								lastPoint = ReadPoint(cmd, AllowComma);

								ArcSegment arcSegment = new ArcSegment
								{
									Size = new Size(w, h),
									RotationAngle = rotation,
									IsLargeArc = large,
									SweepDirection = sweep ? SweepDirection.Clockwise : SweepDirection.CounterClockwise,
									Point = lastPoint
								};

								EnsurePathFigure(pathFigure).Segments.Add(arcSegment);
							}
							while (IsNumber(AllowComma));

							last_cmd = 'A';
							break;

						case 'z':
						case 'Z':
							EnsureFigure();
							EnsurePathFigure(pathFigure).IsClosed = true;
							figureStarted = false;
							last_cmd = 'Z';

							lastPoint = lastStart; // Set reference point to be first point of current figure
							break;

						default:
							throw GetBadTokenException();
					}
				}
			}

			PathFigure EnsurePathFigure(PathFigure? pathFigure)
			{
				if (pathFigure is null)
				{
					throw GetBadTokenException();
				}
				else
				{
					return pathFigure;
				}
			}

			void EnsureFigure()
			{
				if (!figureStarted)
				{
					figureStarted = true;
				}
			}

			Point Reflect()
			{
				return new Point(
					2 * lastPoint.X - secondLastPoint.X,
					2 * lastPoint.Y - secondLastPoint.Y);
			}

			bool More()
			{
				return currentIndex < pathLength;
			}

			bool SkipWhiteSpace(bool allowComma)
			{
				bool commaMet = false;

				while (More())
				{
					char ch = currentPathString[currentIndex];

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
								throw GetBadTokenException();
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

					currentIndex++;
				}

				return commaMet;
			}

			bool ReadBool()
			{
				SkipWhiteSpace(AllowComma);

				if (More())
				{
					token = currentPathString[currentIndex++];

					if (token == '0')
					{
						return false;
					}
					else if (token == '1')
					{
						return true;
					}
				}

				throw GetBadTokenException();
			}

			bool ReadToken()
			{
				SkipWhiteSpace(!AllowComma);

				// Check for end of string
				if (More())
				{
					token = currentPathString[currentIndex++];

					return true;
				}
				else
				{
					return false;
				}
			}

			Exception GetBadTokenException()
			{
				return new FormatException(string.Format("UnexpectedToken \"{0}\" into {1}", currentPathString, currentIndex - 1));
			}

			Point ReadPoint(char cmd, bool allowcomma)
			{
				double x = ReadNumber(allowcomma);
				double y = ReadNumber(AllowComma);

				if (cmd >= 'a') // 'A' < 'a'. lower case for relative
				{
					x += lastPoint.X;
					y += lastPoint.Y;
				}

				return new Point(x, y);
			}

			bool IsNumber(bool allowComma)
			{
				bool commaMet = SkipWhiteSpace(allowComma);

				if (More())
				{
					token = currentPathString[currentIndex];

					// Valid start of a number
					if ((token == '.') || (token == '-') || (token == '+') || ((token >= '0') && (token <= '9'))
						|| (token == 'I')  // Infinity
						|| (token == 'N')) // NaN
					{
						return true;
					}
				}

				if (commaMet) // Only allowed between numbers
				{
					throw GetBadTokenException();
				}

				return false;
			}

			double ReadNumber(bool allowComma)
			{
				if (!IsNumber(allowComma))
				{
					throw GetBadTokenException();
				}

				bool simple = true;
				int start = currentIndex;

				// Allow for a sign
				// 
				// There are numbers that cannot be preceded with a sign, for instance, -NaN, but it's
				// fine to ignore that at this point, since the CLR parser will catch this later.
				if (More() && ((currentPathString[currentIndex] == '-') || currentPathString[currentIndex] == '+'))
				{
					currentIndex++;
				}

				// Check for Infinity (or -Infinity).
				if (More() && (currentPathString[currentIndex] == 'I'))
				{
					// Don't bother reading the characters, as the CLR parser will
					// do this for us later.
					currentIndex = Math.Min(currentIndex + 8, pathLength); // "Infinity" has 8 characters
					simple = false;
				}
				// Check for NaN
				else if (More() && (currentPathString[currentIndex] == 'N'))
				{
					//
					// Don't bother reading the characters, as the CLR parser will
					// do this for us later.
					//
					currentIndex = Math.Min(currentIndex + 3, pathLength); // "NaN" has 3 characters
					simple = false;
				}
				else
				{
					SkipDigits(!AllowSign);

					// Optional period, followed by more digits
					if (More() && (currentPathString[currentIndex] == '.'))
					{
						simple = false;
						currentIndex++;
						SkipDigits(!AllowSign);
					}

					// Exponent
					if (More() && ((currentPathString[currentIndex] == 'E') || (currentPathString[currentIndex] == 'e')))
					{
						simple = false;
						currentIndex++;
						SkipDigits(AllowSign);
					}
				}

				if (simple && (currentIndex <= (start + 8))) // 32-bit integer
				{
					int sign = 1;

					if (currentPathString[start] == '+')
					{
						start++;
					}
					else if (currentPathString[start] == '-')
					{
						start++;
						sign = -1;
					}

					int value = 0;

					while (start < currentIndex)
					{
						value = value * 10 + (currentPathString[start] - '0');
						start++;
					}

					return value * sign;
				}
				else
				{
					string subString = currentPathString.Substring(start, currentIndex - start);

					try
					{
						return Convert.ToDouble(subString, CultureInfo.InvariantCulture);
					}
					catch (FormatException)
					{
						throw new FormatException(string.Format("UnexpectedToken \"{0}\" into {1}", start, currentPathString));
					}
				}
			}

			void SkipDigits(bool signAllowed)
			{
				// Allow for a sign
				if (signAllowed && More() && ((currentPathString[currentIndex] == '-') || currentPathString[currentIndex] == '+'))
				{
					currentIndex++;
				}

				while (More() && (currentPathString[currentIndex] >= '0') && (currentPathString[currentIndex] <= '9'))
				{
					currentIndex++;
				}
			}
		}

		private static string ParsePathFigureCollectionToString(PathFigureCollection pathFigureCollection)
		{
			var sb = new StringBuilder();

			foreach (var pathFigure in pathFigureCollection)
			{
				sb.Append('M')
				.Append(pathFigure.StartPoint.X.ToString(CultureInfo.InvariantCulture))
				.Append(',')
				.Append(pathFigure.StartPoint.Y.ToString(CultureInfo.InvariantCulture))
				.Append(' ');

				foreach (var pathSegment in pathFigure.Segments)
				{
					if (pathSegment is LineSegment lineSegment)
					{
						sb.Append('L')
						.Append(lineSegment.Point.X.ToString(CultureInfo.InvariantCulture))
						.Append(',')
						.Append(lineSegment.Point.Y.ToString(CultureInfo.InvariantCulture))
						.Append(' ');
					}
					else if (pathSegment is BezierSegment bezierSegment)
					{
						sb.Append('C')
						.Append(bezierSegment.Point1.X.ToString(CultureInfo.InvariantCulture))
						.Append(',')
						.Append(bezierSegment.Point1.Y.ToString(CultureInfo.InvariantCulture))
						.Append(' ')
						.Append(bezierSegment.Point2.X.ToString(CultureInfo.InvariantCulture))
						.Append(',')
						.Append(bezierSegment.Point2.Y.ToString(CultureInfo.InvariantCulture))
						.Append(' ')
						.Append(bezierSegment.Point3.X.ToString(CultureInfo.InvariantCulture))
						.Append(',')
						.Append(bezierSegment.Point3.Y.ToString(CultureInfo.InvariantCulture))
						.Append(' ');
					}
					else if (pathSegment is QuadraticBezierSegment quadraticBezierSegment)
					{
						sb.Append('Q')
						.Append(quadraticBezierSegment.Point1.X.ToString(CultureInfo.InvariantCulture))
						.Append(',')
						.Append(quadraticBezierSegment.Point1.Y.ToString(CultureInfo.InvariantCulture))
						.Append(' ')
						.Append(quadraticBezierSegment.Point2.X.ToString(CultureInfo.InvariantCulture))
						.Append(',')
						.Append(quadraticBezierSegment.Point2.Y.ToString(CultureInfo.InvariantCulture))
						.Append(' ');
					}
					else if (pathSegment is ArcSegment arcSegment)
					{
						sb.Append('A')
						.Append(arcSegment.Size.Width)
						.Append(',')
						.Append(arcSegment.Size.Height)
						.Append(' ')
						.Append(arcSegment.RotationAngle)
						.Append(' ')
						.Append(arcSegment.IsLargeArc ? "1" : "0")
						.Append(',')
						.Append(arcSegment.SweepDirection == SweepDirection.Clockwise ? "1" : "0")
						.Append(' ')
						.Append(arcSegment.Point.X.ToString(CultureInfo.InvariantCulture))
						.Append(',')
						.Append(arcSegment.Point.Y.ToString(CultureInfo.InvariantCulture))
						.Append(' ');
					}
				}

				if (pathFigure.IsClosed)
				{
					sb.Append('Z');
				}

				sb.Append(' ');
			}

			if (sb.Length > 0)
			{
				sb.Length--;

				if (sb[sb.Length - 1] == ' ')
				{
					sb.Length--;
				}
			}

			return sb.ToString();
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is PathFigureCollection pathFigureCollection)
			{
				return ParsePathFigureCollectionToString(pathFigureCollection);
			}

			throw new InvalidDataException($"Value is not of type {nameof(PathFigureCollection)}");
		}
	}
}
