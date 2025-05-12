using System.Linq.Expressions;
using FluentAssertions;
using Formula.Expressions;
using Formula.Models;
using Formula.Tests.Infrastructure;
using Formula.Visitors;
using Moq;

namespace Formula.Tests;

public class Array_Sum_Tests : FormulaTest
{
    private GridExpressionVisitor _gridVisitor;

    public Array_Sum_Tests()
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
            .Returns(2)
            .Verifiable();
    
        mockGrid.Setup(g => g.GetValueForCell(It.IsIn(new GridCellReference("A2"))))
            .Returns(2)
            .Verifiable();
    
        mockGrid.Setup(g => g.GetValueForCell(It.IsIn(new GridCellReference("A3"))))
            .Returns(5)
            .Verifiable();
        
        _gridVisitor = new GridExpressionVisitor(mockGrid.Object);
    }

    [Theory]
    [InlineData("SUM(1,2,3)", 6)]
    public void Should_Sum_Numbers(string formula, int expectedResult)
    {
        var result = Evaluator.EvaluateFormula(formula);
        
        result
            .Should()
            .Be(expectedResult);
    }

    [Theory]
    [InlineData("SUM(A1:A3)", 9)]
    public void Should_Sum_GridArray(string formula, int expectedResult)
    {
        var result = Evaluator.EvaluateFormula(formula, _gridVisitor);
        
        result
            .Should()
            .Be(expectedResult);
    }
}