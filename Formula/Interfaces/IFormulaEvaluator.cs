using System.Linq.Expressions;

namespace Formula.Evaluators;

public interface IFormulaEvaluator
{
    Expression FormulaToExpression(string formula);
    // TODO: Pass in params of visitors (incase multiple transformations are needed on an expression
    object EvaluateFormula(string formula, ExpressionVisitor? visitor = null);
    void WithFunctions(Type type);
}