namespace ConfigurationManager;

public static class ObjectUtils
{
    public static T Apply<T>(this T a, Action<T> f)
    {
        f(a);
        return a;
    }
    
    public static R Let<T, R>(this T self, Func<T, R> block) 
    {
        return block(self);
    }
}