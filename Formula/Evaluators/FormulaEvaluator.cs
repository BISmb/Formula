using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using Formula.Models;
using Formula.Extensions;

namespace Formula.Evaluators;

public class FormulaEvaluator : IFormulaEvaluator
{
    private readonly Tokenizer _tokenizer;
    private readonly ExpressionBuilder _expressionBuilder;
    
    public FormulaEvaluator()
    {
        _tokenizer = new Tokenizer();
        _expressionBuilder = new ExpressionBuilder();
    }
    
    public Expression FormulaToExpression(string formula)
    {
        Token[] tokens = _tokenizer.Tokenize(formula);
        Expression expression = Parse(tokens);
        return expression;
    }

    public object EvaluateFormula(string formula, ExpressionVisitor? visitor = null) // optional visitor?
    {
        Expression expression = FormulaToExpression(formula);
        
        if (visitor is not null)
        {
            expression = visitor.Visit(expression);
        }
        
        expression = Expression.Convert(expression, typeof(object));
        
        var lambda = Expression.Lambda<Func<object>>(expression);
        var func = lambda.Compile();
        var result = func();
        return result;
    }

    private Expression Parse(Token[] tokens)
    {
        return _expressionBuilder.Build(tokens);
    }
}