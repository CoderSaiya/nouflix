using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NouFlix.DTOs;
using NouFlix.Models.Common;
using NouFlix.Models.Entities;
using NouFlix.Persistence.Repositories.Interfaces;
using NouFlix.Services.Payment;

namespace NouFlix.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class TransactionController(IUnitOfWork uow, PaymentGatewayFactory paymentFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] int skip = 0,
        [FromQuery] int take = 10,
        CancellationToken ct = default)
    {
        var query = uow.Transactions.Query();
        
        var total = await query.CountAsync(ct);
        var items = await query
            .Include(x => x.User)
            .Include(x => x.Plan)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .Select(x => new TransactionDto(
                x.Id,
                x.UserId,
                x.User.Email.ToString(),
                x.PlanId,
                x.Plan != null ? x.Plan.Name : "N/A",
                x.Amount,
                x.Status,
                x.CreatedAt,
                x.Note
            ))
            .ToListAsync(ct);

        return Ok(GlobalResponse<PagedResult<TransactionDto>>.Success(new PagedResult<TransactionDto>(items, total, skip, take)));
    }

    [HttpPost("{id:guid}/refund")]
    public async Task<IActionResult> Refund(Guid id, CancellationToken ct)
    {
        var transaction = await uow.Transactions.FindAsync(id);
        if (transaction == null)
            return NotFound(GlobalResponse<string>.Error("Transaction not found"));

        if (transaction.Status != TransactionStatus.Completed)
            return BadRequest(GlobalResponse<string>.Error("Only completed transactions can be refunded"));

        // Determine provider from Note or add a Provider column. For now, assume "Stripe" as default or parse from note if we stored it.
        // Let's assume we store "Paid via {Provider}" in Note.
        var provider = "stripe"; // Default
        if (!string.IsNullOrEmpty(transaction.Note) && transaction.Note.Contains("Momo", StringComparison.OrdinalIgnoreCase))
            provider = "momo";

        var gateway = paymentFactory.Create(provider);
        var success = await gateway.RefundPayment(transaction.Id.ToString(), ct);

        if (success)
        {
            transaction.Status = TransactionStatus.Refunded;
            
            transaction.Note += " [Refunded]";
            await uow.SaveChangesAsync(ct);
            return Ok(GlobalResponse<string>.Success("Refund successful"));
        }

        return BadRequest(GlobalResponse<string>.Error("Refund failed"));
    }
}

public record TransactionDto(
    Guid Id,
    Guid UserId,
    string Username,
    Guid? PlanId,
    string PlanName,
    decimal Amount,
    TransactionStatus Status,
    DateTime CreatedAt,
    string? Note
);

public record PagedResult<T>(IEnumerable<T> Items, int Total, int Skip, int Take);
