using System.Linq.Expressions;
using Formula.Expressions.Math;
using Formula.Models;

namespace Formula.Expressions.Grid;

public class CellArrayReferenceExpression
    : Expression
{
    public override Type Type => typeof(IEnumerable<>).MakeGenericType(Expressions.FirstOrDefault()?.Type ?? typeof(object));

    public override ExpressionType NodeType => ExpressionType.Extension;

    public override bool CanReduce => true;
    
    public CellReferenceArray ReferenceArray { get; }
    public IEnumerable<CellReferenceExpression> Expressions { get; }

    public CellArrayReferenceExpression(IEnumerable<CellReferenceExpression> expressions)
    {
        Expressions = expressions;
        
        // Check if there are any elements
        if (!Expressions.Any())
            throw new InvalidOperationException("CustomArrayExpression must contain at least one element.");
    }

    public CellArrayReferenceExpression(CellReferenceArray array)
        : this(array.Select(r => new CellReferenceExpression(r)))
    {
        ReferenceArray = array;
        
    }

    public override Expression Reduce()
    {
        var reducedElements = Expressions.Select(e => e.Reduce()).ToArray();
        // return Expression.NewArrayInit(typeof(double), reducedElements);

        var block = Block(reducedElements);
        return Block(reducedElements);

        //return Expression.Block(Expressions);
        //return NewArrayInit(typeof(double), Expressions);
    }
    
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var changed = false;
        var visitedElements = new List<CellReferenceExpression>();

        foreach (var element in Expressions)
        {
            var visited = (CellReferenceExpression)visitor.Visit(element);
            changed |= visited != element;
            visitedElements.Add(visited);
        }

        return changed ? new CellArrayReferenceExpression(visitedElements) : this;
    }
}