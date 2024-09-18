namespace IPS.Grow.Func.Services;

public interface IOrderedListClient
{
    Task PushData(string key, string value);
}