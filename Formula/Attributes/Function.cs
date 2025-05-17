namespace Formula.Attributes;

public class FunctionAttribute(string name) : Attribute
{
    public readonly string Name = name;
}