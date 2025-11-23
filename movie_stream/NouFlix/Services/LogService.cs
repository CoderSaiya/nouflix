using System.Text.Json;
using Nest;
using NouFlix.DTOs;

namespace NouFlix.Services;

public class LogService(ElasticClient client)
{
    public async Task<(long Total, List<SystemDto.LogEntry<SystemDto.AuditLog>> Items)> SearchLogsAsync(
        int page, int size, string? q, string? level, DateTime? from, DateTime? to)
    {
        var fromIndex = (page - 1) * size;
        var must = new List<QueryContainer>();
        
        must.Add(new ExistsQuery { Field = "Audit._typeTag" });

        if (!string.IsNullOrWhiteSpace(q))
        {
            must.Add(new SimpleQueryStringQuery
            {
                Fields = new[] { "message", "Audit.*" },
                Query = q
            });
        }

        if (!string.IsNullOrWhiteSpace(level))
        {
            must.Add(new TermQuery
            {
                Field = "level",
                Value = level
            });
        }

        if (from.HasValue || to.HasValue)
        {
            must.Add(new DateRangeQuery
            {
                Field = "@timestamp",
                GreaterThanOrEqualTo = from,
                LessThanOrEqualTo = to
            });
        }

        var searchResponse = await client.SearchAsync<object>(s => s
            .Index("audit-logs-*")
            .From(fromIndex)
            .Size(size)
            .Source(false)
            .Fields(f => f
                .Field("@timestamp")
                .Field("level")
                .Field("message")
                .Field("Audit.Id")
                .Field("Audit.TimestampUtc")
                .Field("Audit.CorrelationId")
                .Field("Audit.UserId")
                .Field("Audit.Username")
                .Field("Audit.Action")
                .Field("Audit.ResourceType")
                .Field("Audit.ResourceId")
                .Field("Audit.Details")
                .Field("Audit.ClientIp")
                .Field("Audit.UserAgent")
                .Field("Audit.Route")
                .Field("Audit.HttpMethod")
                .Field("Audit.StatusCode")
                .Field("CorrelationId")
                .Field("ClientIp")
                .Field("UserAgent")
                .Field("RequestPath")
            )
            .Query(q => q.Bool(b => b.Must(must.ToArray())))
            .Sort(ss => ss.Descending(new Field("@timestamp")))
        );
        
        Console.WriteLine($"Total hits: {searchResponse.Total}");
        Console.WriteLine($"Documents count (deserialized by NEST): {searchResponse.Documents?.Count()}");
        
        var items = new List<SystemDto.LogEntry<SystemDto.AuditLog>>();

        foreach (var hit in searchResponse.Hits)
        {
            var fields = hit.Fields;

            // helper đọc field
            T? F<T>(string fieldName)
            {
                // NEST: nếu field không tồn tại -> default(T)
                if (fields == null) return default;
                return fields.Value<T>(fieldName);
            }

            // Build LogEntry
            var entry = new SystemDto.LogEntry<SystemDto.AuditLog>
            {
                Id = hit.Id,
                Timestamp = F<DateTime?>("@timestamp") ?? DateTime.UtcNow,
                Level = F<string>("level"),
                // message là dạng string, nếu muốn có template thì thêm messageTemplate
                Message = F<string>("message")
            };

            // Build AuditLog từ các field Audit.*
            var audit = new SystemDto.AuditLog
            {
                Id = F<string>("Audit.Id"),
                TimestampUtc = F<DateTime?>("Audit.TimestampUtc") ?? entry.Timestamp,
                CorrelationId = F<string>("Audit.CorrelationId") ?? F<string>("CorrelationId"),
                UserId = F<string>("Audit.UserId"),
                Username = F<string>("Audit.Username"),
                Action = F<string>("Audit.Action"),
                ResourceType = F<string>("Audit.ResourceType"),
                ResourceId = F<string>("Audit.ResourceId"),
                Details = F<string>("Audit.Details"),
                ClientIp = F<string>("Audit.ClientIp") ?? F<string>("ClientIp"),
                UserAgent = F<string>("Audit.UserAgent") ?? F<string>("UserAgent"),
                Route = F<string>("Audit.Route") ?? F<string>("RequestPath"),
                HttpMethod = F<string>("Audit.HttpMethod"),
                StatusCode = F<int?>("Audit.StatusCode")
            };

            entry.Audit = audit;
            items.Add(entry);
        }

        return (searchResponse.Total, items);
    }
}