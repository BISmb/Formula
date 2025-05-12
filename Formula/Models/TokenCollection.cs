namespace Formula.Models;

public class TokenCollection : List<Token>
{
    public void Add(TokenType tokenType, string value)
    {
        Add(new Token(tokenType, value));
    }
}