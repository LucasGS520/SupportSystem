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
                💡 <strong>Sugestão da IA:</strong>
                ${t.sugestaoIa || "Sugestão automática de solução para o chamado."}
            </p>

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
                feedback: normalizedFeedback
            };
        });

        renderDashboardTickets(tickets);
        renderTicketsList(tickets);

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
                }
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
                feedback: null
            }
        ];

        renderDashboardTickets(fallbackTickets);
        renderTicketsList(fallbackTickets);
    }
}

// Inicialização geral
document.addEventListener("DOMContentLoaded", () => {
    setupNavigation();
    setupModal();
    loadTicketsFromApi();
});
