using System.Linq.Expressions;

namespace Formula.Expressions.Math;

public class SumExpression : Expression
{
    public List<Expression> Expressions { get; } = new();

    public SumExpression(IEnumerable<Expression> expressions)
    {
        Expressions.AddRange(expressions);
    }

    public Expression ToExpression()
    {
        if (!Expressions.Any())
            throw new InvalidOperationException("No expressions to sum.");

        Expression sum = Expressions.First();
        foreach (var expr in Expressions.Skip(1))
        {
            sum = Expression.Add(sum, expr);
        }

        return sum;
    }

    public LambdaExpression ToLambda(IEnumerable<ParameterExpression> parameters)
    {
        return Expression.Lambda(ToExpression(), parameters);
    }

    public override Expression Reduce()
    {
        var reducedExpressions = Expressions
            .Select(p => p.CanReduce ? p.Reduce() : p)
            .ToList();
        
        if (!reducedExpressions.Any())
            throw new InvalidOperationException("No expressions to reduce.");

        Expression sum = Expression.Constant(0D);
        foreach (var expr in reducedExpressions)
        {   
            if (expr is BlockExpression block)
            {
                foreach (var blockExpression in block.Expressions)
                {
                    sum = Add(sum, blockExpression);
                }
            }
            else if (expr is NewArrayExpression newArrayExpression)
            {
                foreach (var blockExpression in newArrayExpression.Expressions)
                {
                    sum = Add(sum, blockExpression);
                } 
            }
            else
            {
                sum = Add(sum, expr);
            }
        }

        return sum;
    }

    public override Type Type => typeof(double);
    public override ExpressionType NodeType => ExpressionType.Extension;
    public override bool CanReduce => true;
}