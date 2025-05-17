using System.Reflection;

namespace Formula.Extensions;

public static class AttributeExtensions
{
    public static bool TryGetAttribute<TAttribute>(this Type type, out TAttribute? attribute) where TAttribute : Attribute
    {
        attribute = type.IsDefined(typeof(TAttribute)) 
            ? type.GetCustomAttribute(typeof(TAttribute)) as TAttribute 
            : null;
        return attribute is not null;
    }
}