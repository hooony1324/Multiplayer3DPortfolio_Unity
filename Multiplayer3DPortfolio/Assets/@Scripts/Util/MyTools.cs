
public static class MyTools
{
    public static string GetRandomString(int length = 10)
    {
        return System.Guid.NewGuid().ToString("N").Substring(0, length);
    }
}
