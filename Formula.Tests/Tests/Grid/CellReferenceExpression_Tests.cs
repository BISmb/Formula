using System.Linq.Expressions;
using FluentAssertions;
using Formula.Expressions;
using Formula.Expressions.Grid;
using Formula.Models;
using Formula.Visitors;
using Moq;

namespace Formula.Tests;

public class CellReferenceExpression_Tests
{
    private Mock<IGrid> _mockGrid;
    private readonly object _expectedResult = 10D;
    private object _actualResult { get; init; }

    public CellReferenceExpression_Tests()
    {
        _mockGrid = new Mock<IGrid>();
        _mockGrid.Setup(g => g.GetValueForCell(It.IsIn(new GridCellReference("A4"))))
            .Returns(_expectedResult)
            .Verifiable();
        
        Expression expression = new CellReferenceExpression(new("A4"));
        GridExpressionVisitor gridVisitor = new GridExpressionVisitor(_mockGrid.Object);
        //expression = gridVisitor.Visit(expression);    
        
        Expression visitedGridExpression = gridVisitor.Visit(expression) 
                                           ?? throw new NullReferenceException("No expression returned from visitor");
    
        // Compile and execute
        
        var lambda2 = Expression.Lambda<Func<double>>(visitedGridExpression);
        var func2 = lambda2.Compile();
        _actualResult = func2();
    }
    
    [Fact]
    public void Should_Be_Expected_Result()
    {
        _actualResult
            .Should()
            .Be(_expectedResult);
    }
    
    [Fact]
    public void Should_Invoke_Method_From_Grid()
    {
        _mockGrid.Verify(m => m.GetValueForCell(It.IsIn(new GridCellReference("A4"))));
    }
}