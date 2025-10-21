using NouFlix.Adapters;
using NouFlix.DTOs;
using NouFlix.Services.Interface;

namespace NouFlix.Services.Backgroud;

public class TranscodeWorker(
    IServiceScopeFactory scopeFactory,
    IQueue<TranscodeDto.TranscodeJob> q,
    IStatusStorage<TranscodeDto.TranscodeStatus> status,
    ILogger<TranscodeWorker> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var job in q.DequeueAllAsync(ct))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var transcoder = scope.ServiceProvider.GetRequiredService<FfmpegHlsTranscoder>();
                
                status.Upsert(new TranscodeDto.TranscodeStatus
                {
                    JobId = job.JobId,
                    State = "Running",
                    Progress = 0
                });
                await transcoder.RunTranscodeAsync(job, ct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Transcode failed {Job}", job.JobId);
                status.Upsert(new TranscodeDto.TranscodeStatus
                {
                    JobId = job.JobId,
                    State = "Failed",
                    Error = ex.Message,
                    Progress = 0
                });
            }
        }
    }
}