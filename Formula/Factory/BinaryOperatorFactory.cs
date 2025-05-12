using System.Linq.Expressions;
using Formula.Expressions;
using Formula.Models;

namespace Formula.Factory;

public static class ExpressionFactory
{
    public static Expression NewBinaryExpression(Token token, Expression left, Expression right)
    {
        if (token.Type is not TokenType.Operator)
        {
            throw new Exception("Invalid token type. This token type is not a operator.");
        }

        return token.Value[0] switch
        {
            '+' => Expression.Add(left, right),
            '-' => Expression.Subtract(left, right),
            '*' => Expression.Multiply(left, right),
            '/' => Expression.Divide(left, right),
            '%' => Expression.Modulo(left, right),

            _ => throw new Exception("Invalid token type. This token type is not a operator.")
        };
    }
}