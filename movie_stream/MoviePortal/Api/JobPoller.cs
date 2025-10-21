using MoviePortal.Models.DTOs;

namespace MoviePortal.Api;

public class JobPoller
{
    public async Task PollAsync(
        Func<CancellationToken, Task<TranscodeDto.TranscodeStatus?>> getStatus,
        Func<TranscodeDto.TranscodeStatus, Task> onProgress,
        Func<TranscodeDto.TranscodeStatus, Task> onDone,
        TimeSpan? interval = null,
        CancellationToken ct = default)
    {
        var tick = interval ?? TimeSpan.FromSeconds(2);
        while (!ct.IsCancellationRequested)
        {
            var st = await getStatus(ct);
            if (st is null) { await Task.Delay(tick, ct); continue; }

            if (string.Equals(st.State, "Done", StringComparison.OrdinalIgnoreCase))
            {
                await onDone(st);
                return;
            }

            if (string.Equals(st.State, "Failed", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException(st.Error ?? "Job failed.");

            await onProgress(st);
            await Task.Delay(tick, ct);
        }
    }
}