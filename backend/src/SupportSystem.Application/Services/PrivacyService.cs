using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SupportSystem.Application.Abstractions;
using SupportSystem.Application.DTOs;
using SupportSystem.Application.Interfaces;
using SupportSystem.Application.Options;
using SupportSystem.Domain.Extensions;
using SupportSystem.Domain.Repositories;

namespace SupportSystem.Application.Services;

// Orquestra as operações LGPD como exportação, exclusão e retenção de dados.
public class PrivacyService : IPrivacyService
{
    private readonly IUserRepository _userRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<PrivacyService> _logger;
    private readonly PrivacyOptions _options;

    // Cria o serviço com as dependências necessárias.
    public PrivacyService(
        IUserRepository userRepository,
        ITicketRepository ticketRepository,
        IDateTimeProvider dateTimeProvider,
        IOptions<PrivacyOptions> options,
        ILogger<PrivacyService> logger)
    {
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<UserDataExportDto?> ExportAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Solicitação de exportação para usuário inexistente {UserId}", userId);
            return null;
        }

        var tickets = await _ticketRepository.GetByOwnerAsync(userId, cancellationToken);
        var exportTickets = tickets
            .Select(ticket => new ExportTicketDto
            {
                Id = ticket.Id,
                Titulo = ticket.Titulo,
                Status = ticket.Status.ToDisplayName(),
                Categoria = ticket.Categoria.ToDisplayName(),
                AbertoEm = ticket.AbertoEm,
                FeedbackComentario = ticket.ConsentimentoDados ? ticket.Feedback?.Comentario : null
            })
            .ToList();

        _logger.LogInformation("Dados exportados para o usuário {UserId}", userId);

        return new UserDataExportDto
        {
            UserId = user.Id,
            Nome = user.Nome,
            Email = user.Email,
            CriadoEm = user.CriadoEm,
            ConsentimentoDados = user.ConsentimentoDados,
            ConsentimentoRegistradoEm = user.ConsentimentoRegistradoEm,
            Tickets = exportTickets
        };
    }

    public async Task<bool> DeleteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Tentativa de exclusão para usuário inexistente {UserId}", userId);
            return false;
        }

        var removedTickets = await _ticketRepository.DeleteByOwnerAsync(userId, cancellationToken);
        await _userRepository.DeleteAsync(user, cancellationToken);

        _logger.LogInformation(
            "Usuário {UserId} removido juntamente com {TicketCount} tickets", userId, removedTickets);

        return true;
    }

    public async Task<int> ApplyRetentionPolicyAsync(CancellationToken cancellationToken = default)
    {
        var retentionDays = Math.Max(0, _options.RetentionDays);
        var cutoff = _dateTimeProvider.UtcNow.AddDays(-retentionDays);
        var removed = await _ticketRepository.DeleteOlderThanAsync(cutoff, cancellationToken);

        _logger.LogInformation(
            "Política de retenção aplicada removendo {TicketCount} tickets anteriores a {Cutoff}",
            removed,
            cutoff);

        return removed;
    }
}