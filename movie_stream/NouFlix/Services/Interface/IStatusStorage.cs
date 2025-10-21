using NouFlix.DTOs;

namespace NouFlix.Services.Interface;

public interface IStatusStorage<TStatus>
{
    void Upsert(TStatus s);
    TStatus? Get(string jobId);
}