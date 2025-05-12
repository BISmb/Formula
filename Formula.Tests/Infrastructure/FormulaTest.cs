using Formula.Evaluators;

namespace Formula.Tests.Infrastructure;

public abstract class FormulaTest
{
    protected IFormulaEvaluator Evaluator { get; }

    public FormulaTest()
    {
        Evaluator = new FormulaEvaluator();
    }
}