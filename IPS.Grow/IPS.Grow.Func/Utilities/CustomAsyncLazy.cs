namespace IPS.Grow.Func.Utilities;
public sealed class CustomAsyncLazy<T>
{
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
    private readonly Func<Task<T>> _externalFactoryMethod;
    private readonly Func<Task<T>>[] _factoryMethods;
    private T? _value;
    private long _currentIndex;

    public CustomAsyncLazy(Func<Task<T>> factoryMethod)
    {
        _externalFactoryMethod = factoryMethod;
        _factoryMethods = new Func<Task<T>>[] {
                InternalGetValue,
                () => Task.FromResult(_value ?? throw new ArgumentNullException(nameof(_value)))
            };
        _currentIndex = 0;
    }

    private async Task<T> InternalGetValue()
    {
        await _semaphoreSlim.WaitAsync();

        try
        {
            if (Interlocked.Read(ref _currentIndex) == 0)
            {
                _value = await _externalFactoryMethod();
                Interlocked.Increment(ref _currentIndex);
            }

            return await GetValue();
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    public async Task<T> GetValue()
    {
        return await _factoryMethods[Interlocked.Read(ref _currentIndex)]();
    }
}