using System.Linq.Expressions;
using System.Reflection;
using Formula.Attributes;
using Formula.Expressions;
using Formula.Expressions.Grid;
using Formula.Expressions.Math;
using Formula.Extensions;
using Formula.Factory;
using Formula.Models;

namespace Formula.Evaluators;

internal sealed class ExpressionBuilder
{
    private static Dictionary<string, Type> FunctionMappings =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["SUM"] = typeof(SumExpression)
        };

    public static void WithFunctions(Type type)
    {
        string name = type.TryGetAttribute<FunctionAttribute>(out var attr) && attr is not null
            ? attr.Name
            : throw new Exception("Function attribute not found");
        
        FunctionMappings.Add(name, type);
    }
    
    internal Expression Build(Token[] tokens)
    {
        if (tokens == null || tokens.Length == 0)
        {
            throw new ArgumentException("Empty or null token list");
        }

        int index = 0;
        return ParseExpression(tokens, ref index);
    }

    private Expression ParseExpression(Token[] tokens, ref int index)
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

    private Expression ParseTerm(Token[] tokens, ref int index)
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

    private Expression ParseFactor(Token[] tokens, ref int index)
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
            // gather parameters
            List<Expression> arguments = new();
            index++; // skip "("

            while (tokens[index].Type != TokenType.RightParenthesis)
            {
                var paramExpression = ParseExpression(tokens, ref index);
                arguments.Add(paramExpression);
            }
            
            // match static Dictionary
            
            FunctionMappings.TryGetValue(currentToken.Value, out var expressionType);

            if (expressionType is null)
            {
                throw new Exception("Unknown expression type");
            }

            var expression = Activator.CreateInstance(expressionType, [arguments.ToArray()]) as Expression;

            if (expression is null)
            {
                throw new Exception("Cannot create expression");
            }
            
            return expression;
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