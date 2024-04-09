using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.BindingSourceGen;

public sealed record Setter(string[] PatternMatchingExpressions, string AssignmentStatement)
{
    public static Setter From(
        TypeDescription sourceTypeDescription,
        IPathPart[] path,
        string sourceVariableName = "source",
        string assignedValueExpression = "value")
    {
        var builder = new SetterBuilder(sourceVariableName, assignedValueExpression);

        if (path.Length > 0)
        {
            if (sourceTypeDescription.IsNullable)
            {
                builder.AddIsExpression("{}");
            }

            foreach (var part in path)
            {
                builder.AddPart(part);
            }
        }

        return builder.Build();
    }

    private sealed class SetterBuilder
    {
        private readonly string _sourceVariableName;
        private readonly string _assignedValueExpression;

        private string _expression;
        private int _variableCounter = 0;
        private List<string>? _patternMatching;
        private IPathPart? _previousPart;

        public SetterBuilder(string sourceVariableName, string assignedValueExpression)
        {
            _sourceVariableName = sourceVariableName;
            _assignedValueExpression = assignedValueExpression;

            _expression = sourceVariableName;
        }

        public void AddPart(IPathPart nextPart)
        {
            _previousPart = HandlePreviousPart(nextPart);
        }

        private IPathPart? HandlePreviousPart(IPathPart? nextPart)
        {
            if (_previousPart is {} previousPart)
            {
                if (previousPart is Cast { TargetType: var targetType })
                {
                    AddIsExpression(targetType.GlobalName);

                    if (nextPart is ConditionalAccess { Part: var innerPart })
                    {
                        // skip next conditional access, the current `is` expression handles it
                        return innerPart;
                    }
                }
                else if (previousPart is ConditionalAccess { Part: var innerPart })
                {
                    AddIsExpression("{}");
                    _expression = AccessExpressionBuilder.Build(_expression, innerPart);
                }
                else
                {
                    _expression = AccessExpressionBuilder.Build(_expression, previousPart);
                }
            }

            return nextPart;
        }

        public void AddIsExpression(string target)
        {
            var nextVariableName = CreateNextUniqueVariableName();
            var isExpression = $"{_expression} is {target} {nextVariableName}";

            _patternMatching ??= new();
            _patternMatching.Add(isExpression);
            _expression = nextVariableName;
        }

        private string CreateNextUniqueVariableName()
        {
            return $"p{_variableCounter++}";
        }

        private string CreateAssignmentStatement()
        {
            HandlePreviousPart(nextPart: null);
            return $"{_expression} = {_assignedValueExpression};";
        }

        public Setter Build()
        {
            var assignmentStatement = CreateAssignmentStatement();
            var patterns = _patternMatching?.ToArray() ?? Array.Empty<string>();
            return new Setter(patterns, assignmentStatement);
        }
    }
}
