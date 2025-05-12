using System.Linq.Expressions;
using Formula.Models;
using Formula.Models.Enumeration;

namespace Formula.Expressions.Grid;

public class CellReferenceExpression
    : Expression
{
    public override Type Type => FormulaContext == FormulaContextType.String 
        ? typeof(string) 
        : typeof(double);
    
    public override ExpressionType NodeType => ExpressionType.Extension;
    //public override bool CanReduce => true;

    public GridCellReference CellReference { get; }
    public FormulaContextType FormulaContext { get; }
    
    public CellReferenceExpression(GridCellReference reference, FormulaContextType contextType = FormulaContextType.Number)
    {
        CellReference = reference;
        FormulaContext = contextType;
    }

    // public override Expression Reduce()
    // {
    //     return base.Reduce();
    // }
}