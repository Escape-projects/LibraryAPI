namespace LibraryAPI.IntegrationTests;

/// <summary>
/// Общая точка входа для тестовых классов: один ApiFactory на всю коллекцию.
/// </summary>
public abstract class ApiTestBase
{
    protected ApiTestBase(ApiFactory factory)
    {
        Factory = factory;
    }

    protected ApiFactory Factory { get; }
}
