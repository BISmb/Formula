using System.Linq.Expressions;
using Formula.Expressions;
using Formula.Expressions.Grid;
using Formula.Expressions.Math;
using Formula.Factory;
using Formula.Models;

namespace Formula.Evaluators;

internal class ExpressionBuilder
{
    internal Expression Build(Token[] tokens)
    {
        if (tokens == null || tokens.Length == 0)
        {
            throw new ArgumentException("Empty or null token list");
        }

        int index = 0;
        return ParseExpression(tokens, ref index);
    }
    
    protected virtual Expression ParseExpression(Token[] tokens, ref int index)
    {
        Expression left = ParseTerm(tokens, ref index);

        while (index < tokens.Length && tokens[index].Type == TokenType.Operator) //&& (tokens[index].Value.In("+", "-", "*", "/")
        {
            Token op = tokens[index++];
            Expression right = ParseTerm(tokens, ref index);
            left = ExpressionFactory.NewBinaryExpression(op, left, right);
        }

        return left;
    }

    protected virtual Expression ParseTerm(Token[] tokens, ref int index)
    {
        Expression left = ParseFactor(tokens, ref index);

        while (index < tokens.Length && tokens[index].Type == TokenType.Operator)// && (tokens[index].Value == "*" || tokens[index].Value == "/"))
        {
            Token op = tokens[index++];
            Expression right = ParseFactor(tokens, ref index);
            left = ExpressionFactory.NewBinaryExpression(op, left, right);
        }

        return left;
    }

    protected virtual Expression ParseFactor(Token[] tokens, ref int index)
    {
        if (index >= tokens.Length)
        {
            throw new ArgumentException("Incomplete expression");
        }

        Token currentToken = tokens[index++];

        // handle array (specific to each function)
        if (currentToken.Type == TokenType.GridArray)
        {
            // A4:A6 -> SUM(A4+A5+A6), MIN(A4:A6)
            
            CellReferenceArray cellArray = new CellReferenceArray(
                new GridCellReference(currentToken.Value.Split(":")[0]),
                new GridCellReference(currentToken.Value.Split(":")[1])
            );
            // var gridArrayExpression = new CellReferenceArray(cellArray);

            return new CellArrayReferenceExpression(cellArray);

            // ParameterExpression arrayStartParameter =
            //     Expression.Parameter(typeof(double), "arrayStart");
            //
            // ParameterExpression arrayEndParameter =
            //     Expression.Parameter(typeof(double), "arrayEnd");
            //
            //
            // Dictionary<ParameterExpression, string[]> parameters = new()
            // {
            //     [arrayStartParameter] = ["arrayStart"],
            //     [arrayEndParameter] = ["arrayEnd"]
            // };
            //
            // return new SumExpression([new CellArrayReferenceExpression(cellArray)]);

            //return GridArrayExpression.Create(this, grid, currentToken.Value);

            // need to "explode" this out

            //(string Lower, string Upper) arrayBounds =
            //    (currentToken.Value.Split(":")[0], currentToken.Value.Split(":")[1]);

            //string[] cells = GetCellReferencesFromRange(currentToken.Value);
        }

        if (currentToken.Type == TokenType.Function)
        {
            // assume sum expression
            SumExpression expression;
            
            // gather parameters
            List<Expression> arguments = new();
            index++; // skip "("

            while (tokens[index].Type != TokenType.RightParenthesis)
            {
                var paramExpression = ParseExpression(tokens, ref index);
                arguments.Add(paramExpression);
            }
            
            return new SumExpression(arguments);
            
            
            
            // int paramIndex = 0;
            
            
            
            // foreach (Token token in tokens.Skip(index))
            // {
            //     if (token.Type == TokenType.LeftParenthesis) continue;
            //     if (token.Type == TokenType.RightParenthesis) break;
            //     
            //     ParameterExpression parameter =
            //         Expression.Parameter(typeof(double), $"sum{paramIndex++}");
            //     
            //     parameterExpressions.Add(parameter);
            // }
            
            // do
            // {
            //     if ()
            //     
            //     ParameterExpression parameter =
            //         Expression.Parameter(typeof(double), $"{paramIndex}");
            //     
            //     parameterExpressions.Add(parameter);
            //     paramIndex++;
            //
            // } while (tokens[paramIndex].Type != TokenType.RightParenthesis);
            
            // CellReferenceArray cellArray = new CellReferenceArray(
            //     new GridCellReference(tokens[index+1].Value.Split(":")[0]),
            //     new GridCellReference(tokens[index+1].Value.Split(":")[1])
            // );
            // var gridArrayExpression = new CellReferenceArray(cellArray);

            // ParameterExpression arrayStartParameter =
            //     Expression.Parameter(typeof(double), "arrayStart");
            //
            // ParameterExpression arrayEndParameter =
            //     Expression.Parameter(typeof(double), "arrayEnd");

            
            // Dictionary<ParameterExpression, string[]> parameters = new()
            // {
            //     [arrayStartParameter] = ["arrayStart"],
            //     [arrayEndParameter] = ["arrayEnd"]
            // };
            
            //return new SumExpression(parameterExpressions);

            // get custom expression's (like GridCellReference)
            // throw new NotImplementedException("Custom functions not implemented");
            // Dictionary<string, Type> functionClrTypes = typeof(IFormulaExpression)
            //     .Assembly
            //     .ExportedTypes
            //     .Where(t => t.IsAssignableTo(typeof(IFormulaExpression)) && !t.IsInterface && t.IsDefined(typeof(FunctionName)))
            //     .ToDictionary(type => type.GetCustomAttribute<FunctionName>()!.Name, type => type, StringComparer.OrdinalIgnoreCase);
            //
            // var targetType = functionClrTypes[functionName];
            // IFormulaExpression expression = Activator.CreateInstance(targetType, arguments.Cast<object>().ToArray()) as IFormulaExpression
            //     ?? throw new Exception();
            //
            // currentToken = tokens[index++];
            // return expression;
        }

        if (currentToken.Type == TokenType.ConstantValue)
        {
            double value;
            if (!double.TryParse(currentToken.Value, out value))
            {
                throw new ArgumentException("Invalid number format");
            }
            return Expression.Constant(value);
        }

        if (currentToken.Type == TokenType.StringLiteral)
        {
            //if (!double.TryParse(currentToken.Value, out value))
            //{
            //    throw new ArgumentException("Invalid number format");
            //}
            return Expression.Constant(currentToken.Value);
        }

        if (currentToken.Type == TokenType.LeftParenthesis)
        {
            Expression expression = ParseExpression(tokens, ref index);
            if (index >= tokens.Length || tokens[index++].Type != TokenType.RightParenthesis)
            {
                throw new ArgumentException("Mismatched parentheses");
            }
            return expression;
        }

        if (currentToken.Type == TokenType.GridCoordinate)
        {
            // if (grid is null)
            // {
            //     throw new ArgumentException("A grid was not provided but grid coordinates were found in the formula");
            // }
            //
            // throw new NotImplementedException("Grid coordinates not implemented");

            return new CellReferenceExpression(new GridCellReference(currentToken.Value));
        }

        throw new ArgumentException($"Unexpected token: {currentToken.Value}");
    }
}