namespace IPS.Grow.Func.Triggers.Entities;

public interface IBaseEntity<TInput>
{
    Task<bool> UpsertAsync(TInput input);
    Task<bool> DeleteAsync();
}
