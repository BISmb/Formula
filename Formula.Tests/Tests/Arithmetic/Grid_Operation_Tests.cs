using FluentAssertions;
using Formula.Expressions;
using Formula.Models;
using Formula.Tests.Infrastructure;
using Formula.Visitors;
using Moq;

namespace Formula.Tests;

public class Grid_Operation_Tests : FormulaTest
{
    [Theory]
    [InlineData("G5+5", 10)]
    public void GridOperation_Test(string formula, double expectedResult)
    {
        var mockGrid = new Mock<IGrid>();
        mockGrid.Setup(g => g.GetValueForCell(It.IsIn(new GridCellReference("G5"))))
            .Returns(5D)
            .Verifiable();
        
        // todo: provide casting visitor? Convert all expression output to double for expected result of double
        // and vice-versa for expected results of string?
        
        var result = Evaluator.EvaluateFormula(formula, new GridExpressionVisitor(mockGrid.Object));
        
        result
            .Should()
            .Be(expectedResult);
    }
}