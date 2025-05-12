using System.Text;
using System.Text.RegularExpressions;
using Formula.Extensions;
using Formula.Models;

namespace Formula.Evaluators;

internal partial class Tokenizer
{
    [GeneratedRegex("[ ,]", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex IgnoreCharRegex();
    
    [GeneratedRegex("[ \'\"]", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex StringLiteralRegex();
    
    [GeneratedRegex(@"[\+\-\*\/\%]", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex BinaryOperatorRegex();
    
    [GeneratedRegex("[()]", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex GroupingRegex();
    
    internal Token[] Tokenize(string formula)
    {
        TokenCollection tokens = new TokenCollection();

        StringBuilder parseText = new StringBuilder();

        Token? token;
        for (int i = 0; i < formula.Length; i++)
        {
            //if (isParsingStringLiteral)
            //{
            //    parseText.Append(formula[i]);
            //    continue;
            //}

            // todo: add support for array/indexes (B5:B6) -> "[A-Za-z]\d+:[A-Za-z]\d+"
            
            // ignore comma and or space
            if (IgnoreCharRegex().IsMatch(formula[i]))
            {
                continue;
            }

            if (StringLiteralRegex().IsMatch(formula[i]))
            {
                StringBuilder stringLiteral = new();

                // parse through formula untill the string literal is completed
                // todo: does not support apostrophes or quotes within the string
                do
                {
                    i++;
                    stringLiteral.Append(formula[i]);
                } while (!StringLiteralRegex().IsMatch(formula[i+1]));

                tokens.Add(TokenType.StringLiteral, $"{stringLiteral}");
                i++; // increment i pass the string

                continue;
            }

            if (TryGetTokenForSingleChar(formula[i], out token) && token is not null)
            {
                tokens.Add(token);
                continue;
            }

            parseText.Append(formula[i]);

            if (TryGetTokenForParseTerm(formula, i, $"{parseText}", out token) && token is not null)
            {
                parseText.Clear();
                tokens.Add(token);
                continue;
            }
        }

        return tokens.ToArray();
    }
    
    private bool TryGetTokenForSingleChar(char c, out Token? token)
    {
        token = GetTokenForSingleChar(c);
        return token is not null;
    }

    private Token? GetTokenForSingleChar(char c)
    {
        // unneeded
        //if (c.In('\'') || char.IsWhiteSpace(c))
        //{
        //    return null;
        //}
        
        if (GroupingRegex().IsMatch(c))
        {
            return new Token(c == '(' ? TokenType.LeftParenthesis : TokenType.RightParenthesis, $"{c}");
        }

        if (BinaryOperatorRegex().IsMatch(c))
        {
            return new Token(TokenType.Operator, $"{c}");
        }

        //if (c.In(':'))
        //{
        //    return new Token(TokenType.ArrayToken, c);
        //}

        return null;
    }

    private bool TryGetTokenForParseTerm(string formula, int i, string parseTerm, out Token? token)
    {
        token = GetTokenForParseTerm(formula, i, parseTerm);
        return token is not null;
    }

    private Token? GetTokenForParseTerm(string formula, int i, string parseTerm)
    {
        // the next token will cause a change in parsing
        // so we need to match the parseText with regex to
        // determine what it is

        // if term is a digit and next number is not also a digit (or decimal?) (end of formula parse term valid)
        if (Regex.IsMatch(parseTerm, "^[0-9.]+$") && (formula.Length <= i + 1 || (!char.IsDigit(formula[i + 1]) && formula[i + 1] != '.')))
        {
            return new Token(TokenType.ConstantValue, parseTerm);
        }

        // if entire parse text is a individual GridCoordinate
        if (Regex.IsMatch(parseTerm, "^[A-Za-z]+\\d+$") && formula[i + 1] != ':')
        {
            return new Token(TokenType.GridCoordinate, parseTerm);
        }

        // if this is an array of grid coordinates
        if (Regex.IsMatch(parseTerm, "[A-Za-z]\\d+:[A-Za-z]\\d+"))
        {
            return new Token(TokenType.GridArray, parseTerm);
        }

        // if next token is a parenthesis then we can assume this may
        // be a function
        if (i + 1 < formula.Length && formula[i + 1] == '(')
        {
            return new Token(TokenType.Function, parseTerm);
        }

        return null;
    }
}