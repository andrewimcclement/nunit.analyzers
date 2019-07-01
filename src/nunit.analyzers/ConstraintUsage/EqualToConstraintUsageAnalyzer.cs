using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Analyzers.Constants;
using NUnit.Analyzers.Extensions;

namespace NUnit.Analyzers.ConstraintUsage
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EqualToConstraintUsageAnalyzer : BaseAssertionAnalyzer
    {
        private static readonly DiagnosticDescriptor equalToDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IsEqualToConstraintUsage,
            title: EqualToConstraintUsageConstants.IsEqualToTitle,
            messageFormat: EqualToConstraintUsageConstants.IsEqualToMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: EqualToConstraintUsageConstants.IsEqualToDescription);

        private static readonly DiagnosticDescriptor notEqualToDescriptor = DiagnosticDescriptorCreator.Create(
            id: AnalyzerIdentifiers.IsNotEqualToConstraintUsage,
            title: EqualToConstraintUsageConstants.IsNotEqualToTitle,
            messageFormat: EqualToConstraintUsageConstants.IsNotEqualToMessage,
            category: Categories.Assertion,
            defaultSeverity: DiagnosticSeverity.Info,
            description: EqualToConstraintUsageConstants.IsNotEqualToDescription);

        private static readonly string[] SupportedPositiveAssertMethods = new[] {
            NunitFrameworkConstants.NameOfAssertThat,
            NunitFrameworkConstants.NameOfAssertTrue,
            NunitFrameworkConstants.NameOfAssertIsTrue
        };

        private static readonly string[] SupportedNegativeAssertMethods = new[] {
            NunitFrameworkConstants.NameOfAssertFalse,
            NunitFrameworkConstants.NameOfAssertIsFalse
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            equalToDescriptor, notEqualToDescriptor);


        protected override void AnalyzeAssertInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax assertExpression, IMethodSymbol methodSymbol)
        {
            var negated = false;

            if (SupportedNegativeAssertMethods.Contains(methodSymbol.Name))
            {
                negated = !negated;
            }
            else if (!SupportedPositiveAssertMethods.Contains(methodSymbol.Name))
            {
                return;
            }

            var constraint = assertExpression.GetArgumentExpression(methodSymbol, NunitFrameworkConstants.NameOfExpressionParameter);

            // Constraint should be either absent, or Is.True, or Is.False
            if (constraint != null)
            {
                if (!(constraint is MemberAccessExpressionSyntax memberAccess
                       && memberAccess.Expression is IdentifierNameSyntax identifierSyntax
                       && identifierSyntax.Identifier.Text == NunitFrameworkConstants.NameOfIs
                       && (memberAccess.Name.Identifier.Text == NunitFrameworkConstants.NameOfIsTrue
                           || memberAccess.Name.Identifier.Text == NunitFrameworkConstants.NameOfIsFalse)))
                {
                    return;
                }
                else if (memberAccess.Name.Identifier.Text == NunitFrameworkConstants.NameOfIsFalse)
                {
                    negated = !negated;
                }
            }

            var actual = assertExpression.GetArgumentExpression(methodSymbol, NunitFrameworkConstants.NameOfActualParameter)
                ?? assertExpression.GetArgumentExpression(methodSymbol, NunitFrameworkConstants.NameOfConditionParameter);

            var shouldReport = false;

            if (actual.IsKind(SyntaxKind.EqualsExpression)
                || IsStaticObjectEquals(actual, context)
                || IsInstanceObjectEquals(actual, context))
            {
                shouldReport = true;
            }
            else if (actual.IsKind(SyntaxKind.NotEqualsExpression)
                || actual is PrefixUnaryExpressionSyntax unarySyntax
                    && unarySyntax.IsKind(SyntaxKind.LogicalNotExpression)
                    && (IsStaticObjectEquals(unarySyntax.Operand, context)
                        || IsInstanceObjectEquals(unarySyntax.Operand, context)))
            {
                shouldReport = true;
                negated = !negated;
            }

            if (shouldReport && !negated)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    equalToDescriptor,
                    actual.GetLocation()));
            }
            else if (shouldReport && negated)
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    notEqualToDescriptor,
                    actual.GetLocation()));
            }
        }

        private static bool IsStaticObjectEquals(ExpressionSyntax expressionSyntax, SyntaxNodeAnalysisContext context)
        {
            if (!(expressionSyntax is InvocationExpressionSyntax invocation))
                return false;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;

            return methodSymbol != null
                && methodSymbol.IsStatic
                && methodSymbol.Parameters.Length == 2
                && methodSymbol.Name == nameof(object.Equals)
                && methodSymbol.ContainingType.SpecialType == SpecialType.System_Object;
        }

        private static bool IsInstanceObjectEquals(ExpressionSyntax expressionSyntax, SyntaxNodeAnalysisContext context)
        {
            if (!(expressionSyntax is InvocationExpressionSyntax invocation))
                return false;

            var methodSymbol = context.SemanticModel.GetSymbolInfo(invocation.Expression).Symbol as IMethodSymbol;

            return methodSymbol != null
                && !methodSymbol.IsStatic
                && methodSymbol.Parameters.Length == 1
                && methodSymbol.Name == nameof(object.Equals);
        }
    }
}
