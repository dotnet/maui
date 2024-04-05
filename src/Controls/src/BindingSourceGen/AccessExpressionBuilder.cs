using System;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.Controls.BindingSourceGen
{
    public sealed class AccessExpressionBuilder
    {
        private StringBuilder _sb = new();
        private bool _unsafeAccess = false;
        private bool _encounteredConditionalAccess = false;
        private int _buildingExpression = 0;

        public string BuildExpression(string variableName, IPathPart[] path, bool unsafeAccess = false, int depth = int.MaxValue)
        {
            if (Interlocked.CompareExchange(ref _buildingExpression, 1, 0) != 0)
            {
                throw new InvalidOperationException("Cannot generate multiple expressions concurrently");
            }

            _sb.Clear();
            _encounteredConditionalAccess = false;
            _unsafeAccess = unsafeAccess;

            try
            {
                return DoBuildExpression(variableName, path, depth);
            }
            finally
            {
                Interlocked.Exchange(ref _buildingExpression, 0);
            }
        }

        private string DoBuildExpression(string variableName, IPathPart[] path, int depth)
        {
            _sb.Append(variableName);

            depth = Clamp(depth, 0, path.Length);
            for (int i = 0; i < depth; i++)
            {
                AddPathPart(path[i], isLast: i == depth - 1);
            }

            return _sb.ToString();

            static int Clamp(int value, int min, int max)
                => Math.Max(min, Math.Min(max, value));
        }

        private void AddPathPart(IPathPart part, bool isLast)
        {
            if (part is Cast cast)
            {
                AddCast(cast, isLast);
            }
            else if (part is ConditionalAccess conditionalAccess)
            {
                AddConditionalAccess(conditionalAccess, isLast);
            }
            else if (part is IndexAccess indexer)
            {
                AppendIndexAccess(indexer);
            }
            else if (part is MemberAccess memberAccess)
            {
                AppendMemberAccess(memberAccess);
            }
            else
            {
                throw new NotSupportedException($"Unsupported path part type: {part.GetType()}");
            }
        }

        private void AddConditionalAccess(ConditionalAccess conditionalAccess, bool isLast)
        {
            _encounteredConditionalAccess = true;

            if (!_unsafeAccess)
            {
                _sb.Append('?');
            }

            AddPathPart(conditionalAccess.Part, isLast);
        }

        private void AddCast(Cast cast, bool isLast)
        {
            AddPathPart(cast.Part, isLast);

            if (_unsafeAccess)
            {
                PrependUnsafeCast(cast);
            }
            else
            {
                AppendSafeCast(cast);
            }

            if (!isLast)
            {
                WrapInParentheses();
            }
        }

        private void AppendMemberAccess(MemberAccess memberAccess)
        {
            _sb.Append('.');
            _sb.Append(memberAccess.MemberName);
        }

        private void AppendIndexAccess(IndexAccess indexAccess)
        {
            _sb.Append('[');
            _sb.Append(indexAccess.Index.FormattedIndex);
            _sb.Append(']');
        }

        private void PrependUnsafeCast(Cast cast)
        {
            var targetType = cast.TargetType;

            // If we've encoutered any conditional access previously, we need to cast all value types to their nullable versions
            if (targetType.IsValueType && _encounteredConditionalAccess)
            {
                targetType = targetType with { IsNullable = true };
            }

            _sb.Insert(0, $"({targetType})");
        }

        private void AppendSafeCast(Cast cast)
        {
            // for value types, we need to make sure we cast to a nullable type
            var targetType = cast.TargetType;
            if (cast.TargetType.IsValueType)
            {
                targetType = targetType with { IsNullable = true };
            }

            _sb.Append(" as ");
            _sb.Append(targetType.ToString());
        }

        private void WrapInParentheses()
        {
            _sb.Insert(0, '(');
            _sb.Append(')');
        }
    }
}