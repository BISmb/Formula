namespace Formula.Models;

public enum TokenType
{
    ConstantValue,
    StringLiteral,
    GridCoordinate,
    GridArray,
    Function,
    Operator,
    LeftParenthesis,
    RightParenthesis
}

public record Token(TokenType Type, string Value);