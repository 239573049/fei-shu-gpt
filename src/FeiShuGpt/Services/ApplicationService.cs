namespace FeiShuGpt.Services;

public abstract class ApplicationService<T> : ServiceBase
{
    protected IFreeSql FreeSql => GetRequiredService<IFreeSql>();

    protected ILogger<T> Logger => GetRequiredService<ILogger<T>>();

}
