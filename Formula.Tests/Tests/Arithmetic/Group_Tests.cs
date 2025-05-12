using FluentAssertions;
using Formula.Evaluators;
using Formula.Tests.Infrastructure;

namespace Formula.Tests;

public class Group_Tests : FormulaTest
{
    [Theory]
    [InlineData("1 - (2+4)", -5)]
    public void GroupTest(string formula, int expectedResult)
    {
        object actual = Evaluator.EvaluateFormula(formula);

        actual
            .Should()
            .Be(expectedResult);
    }
}