using System.Linq.Expressions;
using FluentAssertions;
using Formula.Attributes;
using Formula.Expressions;
using Formula.Models;
using Formula.Tests.Infrastructure;
using Formula.Visitors;
using Moq;

namespace Formula.Tests;

public class Array_Min_Tests : FormulaTest
{
    private GridExpressionVisitor _gridVisitor;

    public Array_Min_Tests()
    {
        var mockGrid = new Mock<IGrid>();
        mockGrid.Setup(g => g.GetAllCellReferencesFromArray(It.IsIn(new CellReferenceArray(new GridCellReference("A1"), new GridCellReference("A3")))))
            .Returns([
                new GridCellReference("A1"), 
                new GridCellReference("A2"), 
                new GridCellReference("A3")
            ])
            .Verifiable();
    
        mockGrid.Setup(g => g.GetValueForCell(It.IsIn(new GridCellReference("A1"))))
            .Returns(3)
            .Verifiable();
    
        mockGrid.Setup(g => g.GetValueForCell(It.IsIn(new GridCellReference("A2"))))
            .Returns(5)
            .Verifiable();
    
        mockGrid.Setup(g => g.GetValueForCell(It.IsIn(new GridCellReference("A3"))))
            .Returns(2)
            .Verifiable();
        
        _gridVisitor = new GridExpressionVisitor(mockGrid.Object);
        
        Evaluator.WithFunctions(typeof(MinExpression));
    }

    [Theory]
    [InlineData("MIN(2,1,3)", 1)]
    public void Should_Min_Numbers(string formula, int expectedResult)
    {
        var result = Evaluator.EvaluateFormula(formula);
        
        result
            .Should()
            .Be(expectedResult);
    }

    [Theory]
    [InlineData("MIN(A1:A3)", 2)]
    public void Should_Min_GridArray(string formula, int expectedResult)
    {
        var result = Evaluator.EvaluateFormula(formula, _gridVisitor);
        
        result
            .Should()
            .Be(expectedResult);
    }
}

[Function("MIN")]
public class MinExpression : Expression
{
    public List<Expression> Expressions { get; } = new();

    public MinExpression(IEnumerable<Expression> expressions)
    {
        Expressions.AddRange(expressions);
    }

    public Expression ToExpression()
    {
        if (!Expressions.Any())
            throw new InvalidOperationException("No expressions to min.");

        Expression minExpr = Expressions[0];
        
        foreach (var value in Expressions.Skip(1))
        {
            minExpr = Expression.Condition(
                Expression.LessThan(value, minExpr), 
                value, 
                minExpr);
        }

        return minExpr;
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

        Expression minExpr = Expressions[0];
        foreach (var expr in reducedExpressions)
        {   
            if (expr is BlockExpression block)
            {
                foreach (var blockExpression in block.Expressions)
                {
                    minExpr = Expression.Condition(
                        Expression.LessThan(blockExpression, minExpr),
                        blockExpression,
                        minExpr);
                }
            }
            else if (expr is NewArrayExpression newArrayExpression)
            {
                foreach (var blockExpression in newArrayExpression.Expressions)
                {
                    minExpr = Expression.Condition(
                        Expression.LessThan(blockExpression, minExpr),
                        blockExpression,
                        minExpr);
                } 
            }
            else
            {
                minExpr = Expression.Condition(
                    Expression.LessThan(expr, minExpr),
                    expr,
                    minExpr);
            }
        }

        return minExpr;
    }

    public override Type Type => typeof(double);
    public override ExpressionType NodeType => ExpressionType.Extension;
    public override bool CanReduce => true;
}