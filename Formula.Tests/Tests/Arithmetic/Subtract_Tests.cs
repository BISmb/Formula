using FluentAssertions;
using Formula.Evaluators;
using Formula.Tests.Infrastructure;

namespace Formula.Tests;

public class Subtract_Tests : FormulaTest
{
    [Theory]
    [InlineData("1-2", -1)]
    public void SubtractTest(string formula, int expectedResult)
    {
        // evaluate formula, returns result
        object actual = Evaluator.EvaluateFormula(formula);

        actual
            .Should()
            .Be(expectedResult);
    }
}