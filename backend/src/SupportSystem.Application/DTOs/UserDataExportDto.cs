using System;
using System.Collections.Generic;

namespace SupportSystem.Application.DTOs;

// Representa o pacote completo de dados pessoais exportados para LGPD.
public record class UserDataExportDto
{
    // Identificador do usuário autenticado.
    public int UserId { get; init; }

    // Nome descriptografado do usuário
    public required string Nome { get; init; }

    // Endereço de e-mail utilizado para login.
    public required string Email { get; init; }

    // Data de criação da conta em UTC.
    public DateTime CriadoEm { get; init; }

    // Flag que indica se existe consentimento válido.
    public bool ConsentimentoDados { get; init; }

    // Instante em que o consentimento foi registrado.
    public DateTime? ConsentimentoRegistradoEm { get; init; }

    // Chamados vinculados ao usuário com dados sanitizados.
    public IReadOnlyList<ExportTicketDto> Tickets { get; init; } = Array.Empty<ExportTicketDto>();
}

// Projeção simplificada de ticket utilizada durante exportações LGPD.
public record class ExportTicketDto
{
    // Identificador do ticket.
    public int Id { get; init; }

    // Título do chamado.
    public required string Titulo { get; init; }

    // Status atual exibido em texto.
    public required string Status { get; init; }

    // Categoria textual do ticket.
    public required string Categoria { get; init; }

    // Momento de abertura em UTC.
    public DateTime AbertoEm { get; init; }

    // Comentário de feedback quando permitido pelo consentimento.
    public string? FeedbackComentario { get; init; }
}