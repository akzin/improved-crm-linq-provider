using System;
using System.Linq.Expressions;
using Akzin.Crm.Linq.Relinq.Parsing.ExpressionVisitors;
using Akzin.Crm.Linq.Relinq.Parsing.ExpressionVisitors.TreeEvaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Akzin.Crm.Linq.Tests.Cas
{
    [TestClass]
    public class RelinqSandboxTests
    {
        [TestMethod]
        public void PartialEvaluationHandlesClosuresCallsArraysAndArithmeticWithoutCompilation()
        {
            var values = new[] { 2, 5 };
            var offset = 3;
            Expression<Func<int>> expression = () => values[1] + Double(offset);

            var result = PartialEvaluatingExpressionVisitor.EvaluateIndependentSubtrees(
                expression.Body,
                new AllowAllFilter());

            Assert.AreEqual(ExpressionType.Constant, result.NodeType);
            Assert.AreEqual(11, ((ConstantExpression)result).Value);
        }

        [TestMethod]
        public void PartialEvaluationHandlesNullableCoalesceAndConditional()
        {
            int? value = null;
            var useFallback = true;
            Expression<Func<int>> expression = () => useFallback ? (value ?? 7) : 9;

            var result = PartialEvaluatingExpressionVisitor.EvaluateIndependentSubtrees(
                expression.Body,
                new AllowAllFilter());

            Assert.AreEqual(7, ((ConstantExpression)result).Value);
        }

        private static int Double(int value)
        {
            return value * 2;
        }

        private sealed class AllowAllFilter : EvaluatableExpressionFilterBase
        {
        }
    }
}
