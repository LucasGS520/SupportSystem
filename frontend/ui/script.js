// Troca de abas (Dashboard, Chamados, Base, Configurações)
function activatePage(pageName) {
    const pages = document.querySelectorAll(".page");
    const navItems = document.querySelectorAll(".nav-item");
    const mobileNavItems = document.querySelectorAll(".mobile-nav-item");

    pages.forEach((page) => {
        page.classList.toggle("active", page.id === `page-${pageName}`);
    });

    navItems.forEach((btn) => {
        btn.classList.toggle("active", btn.dataset.page === pageName);
    });

    mobileNavItems.forEach((btn) => {
        btn.classList.toggle("active", btn.dataset.page === pageName);
    });
}

function setupNavigation() {
    document.querySelectorAll(".nav-item").forEach((btn) => {
        btn.addEventListener("click", () => {
            const page = btn.dataset.page;
            activatePage(page);
        });
    });

    document.querySelectorAll(".mobile-nav-item").forEach((btn) => {
        btn.addEventListener("click", () => {
            const page = btn.dataset.page;
            activatePage(page);
        });
    });

    // Botão "Ver todos" no dashboard leva para aba de chamados
    const btnVerTodos = document.getElementById("btnVerTodos");
    if (btnVerTodos) {
        btnVerTodos.addEventListener("click", () => activatePage("tickets"));
    }
}

// Modal "Novo chamado"
function setupModal() {
    const fab = document.getElementById("btnNovoChamado");
    const modal = document.getElementById("modalNovoChamado");
    const btnFechar = document.getElementById("btnFecharModal");
    const btnCancelar = document.getElementById("btnCancelarModal");
    const consentCheckbox = document.getElementById("ticketConsent");
    const submitButton = document.getElementById("btnCriarChamado");

    const closeModal = () => modal.classList.remove("active");
    const openModal = () => modal.classList.add("active");

    if (fab && modal) {
        fab.addEventListener("click", openModal);
    }
    if (btnFechar) btnFechar.addEventListener("click", closeModal);
    if (btnCancelar) btnCancelar.addEventListener("click", closeModal);

    // Fecha se clicar fora
    if (modal) {
        modal.addEventListener("click", (e) => {
            if (e.target === modal) closeModal();
        });
    }

    if (consentCheckbox && submitButton) {
        // Impede envio enquanto o consentimento não estiver marcado.
        const updateButtonState = () => {
            submitButton.disabled = !consentCheckbox.checked;
        };

        consentCheckbox.addEventListener("change", updateButtonState);
        updateButtonState();
    }
}

// Configura controles da área de privacidade (exportação, exclusão e status).
function setupPrivacyControls() {
    const consentCheckbox = document.getElementById("ticketConsent");
    const consentStatus = document.getElementById("privacyConsentStatus");
    const exportButton = document.getElementById("btnExportarDados");
    const deleteButton = document.getElementById("btnExcluirDados");

    if (consentCheckbox && consentStatus) {
        const syncStatus = () => {
            const ativo = consentCheckbox.checked;
            consentStatus.textContent = ativo ? "Ativo" : "Revogado";
            consentStatus.classList.toggle("info", ativo);
            consentStatus.classList.toggle("danger", !ativo);
        };

        consentCheckbox.addEventListener("change", syncStatus);
        syncStatus();
    }

    if (exportButton) {
        exportButton.addEventListener("click", async () => {
            await exportUserData();
        });
    }

    if (deleteButton) {
        deleteButton.addEventListener("click", async () => {
            const confirmacao = window.confirm(
                "Tem certeza que deseja excluir todos os dados pessoais? Esta ação não pode ser desfeita."
            );

            if (!confirmacao) {
                return;
            }

            await deleteUserData();
        });
    }
}

// Realiza chamada para exportar dados pessoais do usuário autenticado.
async function exportUserData() {
    try {
        const response = await fetch("/api/privacy/export", { method: "GET" });
        if (!response.ok) {
            throw new Error(`Falha ao exportar (HTTP ${response.status})`);
        }

        const data = await response.json();
        console.log("Exportação de dados concluída:", data);
        alert("Exportação concluída! Confira o console para visualizar o JSON retornado.");
    } catch (error) {
        console.error("Erro ao exportar dados pessoais:", error);
        alert("Não foi possível exportar os dados no momento. Tente novamente mais tarde.");
    }
}

// Dispara a exclusão definitiva dos dados pessoais do usuário.
async function deleteUserData() {
    try {
        const response = await fetch("/api/privacy/forget-me", { method: "DELETE" });
        if (response.status === 404) {
            alert("Nenhum dado para excluir foi localizado.");
            return;
        }

        if (!response.ok) {
            throw new Error(`Falha ao excluir (HTTP ${response.status})`);
        }

        alert("Dados pessoais excluídos com sucesso. Faça login novamente para continuar.");
    } catch (error) {
        console.error("Erro ao excluir dados pessoais:", error);
        alert("Não foi possível excluir os dados agora. Verifique sua conexão ou tente mais tarde.");
    }
}

// Renderiza os chamados na tabela do Dashboard
function renderDashboardTickets(tickets) {
    const tbody = document.querySelector(".data-table tbody");
    if (!tbody) return;

    tbody.innerHTML = "";

    tickets.slice(0, 3).forEach((t) => {
        const tr = document.createElement("tr");

        tr.innerHTML = `
            <td>${t.id}</td>
            <td>${t.titulo}</td>
            <td>${t.solicitante || "Usuário"}</td>
            <td>
                <span class="tag ${mapPriorityTag(t.prioridade)}">
                    ${t.prioridade}
                </span>
            </td>
            <td>
                <span class="tag ${mapStatusTag(t.status)}">
                    ${t.status}
                </span>
            </td>
            <td>${t.categoria || "Outros"}</td>
            <td>${formatSlaTarget(t.slaTarget)}</td>
        `;

        tbody.appendChild(tr);
    });
}

// Renderiza os cards de chamados na página "Chamados"
function renderTicketsList(tickets) {
    const list = document.querySelector("#page-tickets .ticket-list");
    if (!list) return;

    list.innerHTML = "";

    tickets.forEach((t) => {
        const card = document.createElement("article");
        card.className = "ticket-card";

        const feedback = formatFeedback(t.feedback);
        const suggestionsSection = formatSuggestions(t.suggestions);

        card.innerHTML = `
            <header>
                <span class="ticket-id">#${t.id}</span>
                <span class="tag ${mapPriorityTag(t.prioridade)}">${t.prioridade}</span>
            </header>

            <h4>${t.titulo}</h4>

            <p class="ticket-meta">
                Aberto por <strong>${t.solicitante || "Usuário"}</strong>
                • ${t.abertoHa || "Hoje"}
            </p>

            <p class="ticket-meta">
                Categoria: <strong>${t.categoria || "Outros"}</strong>
                • Técnico: <strong>${formatTechnician(t.assignedTechnicianId)}</strong>
            </p>

            <p class="ticket-meta">
                SLA alvo: <strong>${formatSlaTarget(t.slaTarget)}</strong>
            </p>

            <p class="ticket-ai">
                💡 <strong>Sugestão em destaque:</strong>
                ${t.sugestaoIa || "Sugestão automática de solução para o chamado."}
            </p>

            ${suggestionsSection}

            ${feedback}

            <footer>
                <span class="tag ${mapStatusTag(t.status)}">${t.status}</span>
                <button class="btn small secondary">Ver detalhes</button>
            </footer>
        `;

        list.appendChild(card);
    });
}

// Faz o mapeamento de prioridade -> classe CSS
function mapPriorityTag(priority) {
    if (!priority) return "info";

    const p = priority.toLowerCase();
    if (p.includes("alta")) return "danger";
    if (p.includes("média") || p.includes("media")) return "medium";
    if (p.includes("baixa")) return "info";

    return "info";
}

// Mapeia status -> classe CSS
function mapStatusTag(status) {
    if (!status) return "info";

    const s = status.toLowerCase();
    if (s.includes("critico") || s.includes("crítico")) return "danger";
    if (s.includes("andamento")) return "warning";
    if (s.includes("aguardando")) return "info";
    if (s.includes("resolvido")) return "success";

    return "info";
}

// Formata o identificador do técnico responsável
function formatTechnician(technicianId) {
    if (!technicianId) {
        return "Aguardando designação";
    }

    return `Técnico #${technicianId}`;
}

// Converte o alvo de SLA para formato local curto
function formatSlaTarget(slaTarget) {
    if (!slaTarget) {
        return "Não definido";
    }

    const date = new Date(slaTarget);
    if (Number.isNaN(date.getTime())) {
        return "Não definido";
    }

    return new Intl.DateTimeFormat("pt-BR", {
        day: "2-digit",
        month: "2-digit",
        hour: "2-digit",
        minute: "2-digit"
    }).format(date);
}

// Monta bloco de feedback quando disponível
function formatFeedback(feedback) {
    if (!feedback) {
        return "";
    }

    const { nota, comentario, registradoEm } = feedback;

    const feedbackDate = registradoEm ? formatSlaTarget(registradoEm) : null;

    const lines = [];
    if (nota) {
        lines.push(`Nota: ${nota}/5`);
    }
    if (comentario) {
        lines.push(`Comentário: ${comentario}`);
    }
    if (feedbackDate) {
        lines.push(`Registrado em: ${feedbackDate}`);
    }

    if (!lines.length) {
        return "";
    }

    return `
        <p class="ticket-feedback">
            <strong>Feedback:</strong> ${lines.join(" • ")}
        </p>
    `;
}

// Monta a lista de sugestões combinando IA e base de conhecimento
function formatSuggestions(suggestions) {
    if (!Array.isArray(suggestions) || suggestions.length === 0) {
        return "";
    }

    const items = suggestions
        .map((suggestion) => {
            const fonte = suggestion.fonte || suggestion.Fonte || "Base de conhecimento";
            const titulo = suggestion.titulo || suggestion.Titulo || "Sugestão";
            const descricao = suggestion.descricao || suggestion.Descricao || "";

            return `
                <li>
                    <span class="suggestion-source">${fonte}</span>
                    <div>
                        <strong>${titulo}</strong>
                        <p>${descricao}</p>
                    </div>
                </li>
            `;
        })
        .join("");

    return `
        <section class="ticket-suggestions">
            <h5>📚 Sugestões relacionadas</h5>
            <ul>
                ${items}
            </ul>
        </section>
    `;
}

// Chama a API /api/tickets e alimenta a tela
async function loadTicketsFromApi() {
    try {
        const response = await fetch("/api/tickets");

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();

        // Normaliza chave -> minúsculo (caso o back use PascalCase)
        const tickets = data.map((t) => {
            const feedback = t.feedback ?? t.Feedback ?? null;

            const rawSuggestions = t.suggestions ?? t.Suggestions ?? [];
            const normalizedSuggestions = Array.isArray(rawSuggestions)
                ? rawSuggestions.map((suggestion) => ({
                      titulo: suggestion.titulo ?? suggestion.Titulo ?? "Sugestão",
                      descricao: suggestion.descricao ?? suggestion.Descricao ?? "",
                      fonte: suggestion.fonte ?? suggestion.Fonte ?? "Base de conhecimento"
                  }))
                : [];

            const normalizedFeedback = feedback
                ? {
                      nota: feedback.nota ?? feedback.Nota ?? null,
                      comentario: feedback.comentario ?? feedback.Comentario ?? null,
                      registradoEm: feedback.registradoEm ?? feedback.RegistradoEm ?? null
                  }
                : null;

            return {
                id: t.id ?? t.Id,
                titulo: t.titulo ?? t.Titulo,
                prioridade: t.prioridade ?? t.Prioridade ?? "Média",
                status: t.status ?? t.Status ?? "Em andamento",
                ownerId: t.ownerId ?? t.OwnerId ?? null,
                assignedTechnicianId: t.assignedTechnicianId ?? t.AssignedTechnicianId ?? null,
                categoria: t.categoria ?? t.Categoria ?? "Outros",
                slaTarget: t.slaTarget ?? t.SlaTarget ?? null,
                solicitante: t.solicitante ?? t.Solicitante,
                abertoEm: t.abertoEm ?? t.AbertoEm ?? null,
                abertoHa: t.abertoHa ?? t.AbertoHa,
                sugestaoIa: t.sugestaoIa ?? t.SugestaoIa,
                feedback: normalizedFeedback,
                suggestions: normalizedSuggestions,
                consentimentoDados: t.consentimentoDados ?? t.ConsentimentoDados ?? false
            };
        });

        renderDashboardTickets(tickets);
        renderTicketsList(tickets);
        updateConsentStatusFromTickets(tickets);

        console.log("Chamados carregados da API:", tickets);
    } catch (error) {
        console.error("Erro ao carregar chamados da API:", error);

        // Fallback: dados estáticos pra não quebrar a apresentação
        const fallbackTickets = [
            {
                id: 1023,
                titulo: "Erro ao acessar sistema via VPN",
                prioridade: "Alta",
                status: "Em andamento",
                ownerId: 1,
                assignedTechnicianId: 3,
                categoria: "Infraestrutura e redes",
                slaTarget: new Date().toISOString(),
                solicitante: "Maria Silva",
                abertoHa: "há 25 min",
                sugestaoIa:
                    "Verificar credenciais de AD, política de acesso e status do servidor TS.",
                feedback: {
                    nota: 5,
                    comentario: "Atendimento rápido e cordial.",
                    registradoEm: new Date().toISOString()
                },
                suggestions: [
                    {
                        titulo: "Sugestão da IA",
                        descricao:
                            "Verifique sincronização de credenciais, status do servidor VPN e políticas de firewall.",
                        fonte: "Assistente virtual"
                    },
                    {
                        titulo: "Erro de VPN após atualização de credenciais",
                        descricao:
                            "Revisar políticas de acesso no AD, confirmar sincronização de credenciais e reiniciar o serviço VPN.",
                        fonte: "Base de conhecimento"
                    }
                ]
            },
            {
                id: 1019,
                titulo: "Lentidão no sistema de folha",
                prioridade: "Média",
                status: "Aguardando análise",
                ownerId: 2,
                assignedTechnicianId: null,
                categoria: "Aplicações",
                slaTarget: null,
                solicitante: "Depto RH",
                abertoHa: "Hoje, 08:12",
                sugestaoIa:
                    "Conferir uso de CPU e memória no servidor, além de índices do banco.",
                feedback: null,
                suggestions: [
                    {
                        titulo: "Lentidão em aplicação web",
                        descricao:
                            "Validar utilização de recursos no servidor, analisar métricas de banco e aplicar índices recomendados.",
                        fonte: "Base de conhecimento"
                    }
                ]
            }
        ];

        renderDashboardTickets(fallbackTickets);
        renderTicketsList(fallbackTickets);
        updateConsentStatusFromTickets(fallbackTickets);
    }
}

// Atualiza o rótulo de consentimento com base nos tickets retornados pela API.
function updateConsentStatusFromTickets(tickets) {
    const consentStatus = document.getElementById("privacyConsentStatus");
    if (!consentStatus || !Array.isArray(tickets)) {
        return;
    }

    const hasConsent = tickets.some((ticket) => ticket.consentimentoDados);
    consentStatus.textContent = hasConsent ? "Ativo" : "Revogado";
    consentStatus.classList.toggle("info", hasConsent);
    consentStatus.classList.toggle("danger", !hasConsent);
}

// Inicialização geral
document.addEventListener("DOMContentLoaded", () => {
    setupNavigation();
    setupModal();
    setupPrivacyControls();
    loadTicketsFromApi();
});
