using FluentAssertions;
using Formula.Evaluators;
using Formula.Tests.Infrastructure;

namespace Formula.Tests;

public class Add_Tests : FormulaTest
{
    [Theory]
    [InlineData("1 + 2", 3)]
    public void AddTest(string formula, int expectedResult)
    {
        // evaluate formula, returns result
        object actual = Evaluator.EvaluateFormula(formula);

        actual
            .Should()
            .Be(expectedResult);
    }
}