using NouFlix.Adapters;
using NouFlix.DTOs;
using NouFlix.Services.Interface;

namespace NouFlix.Services.Backgroud;

public class SubtitleWorker(
    IServiceScopeFactory scopeFactory,
    IQueue<SubtitleDto.SubtitleJob> q,
    IStatusStorage<SubtitleDto.SubtitleStatus> status,
    ILogger<SubtitleWorker> log,
    IHttpClientFactory httpFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var http = httpFactory.CreateClient();
        
        await foreach (var job in q.DequeueAllAsync(ct))
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var transcoder = scope.ServiceProvider.GetRequiredService<FfmpegHlsTranscoder>();
                
                status.Upsert(new SubtitleDto.SubtitleStatus
                {
                    JobId = job.JobId,
                    State = "Running",
                    Progress = 0
                });
                
                await using var src = await http.GetStreamAsync(job.PresignedUrl, ct);
                
                await transcoder.RunSubtitleAsync(job, src, ct);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Transcode failed {Job}", job.JobId);
                status.Upsert(new SubtitleDto.SubtitleStatus
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