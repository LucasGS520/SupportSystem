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

            <p class="ticket-ai">
                💡 <strong>Sugestão da IA:</strong>
                ${t.sugestaoIa || "Sugestão automática de solução para o chamado."}
            </p>

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

// Chama a API /api/tickets e alimenta a tela
async function loadTicketsFromApi() {
    try {
        const response = await fetch("/api/tickets");

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();

        // Normaliza chave -> minúsculo (caso o back use PascalCase)
        const tickets = data.map((t) => ({
            id: t.id ?? t.Id,
            titulo: t.titulo ?? t.Titulo,
            prioridade: t.prioridade ?? t.Prioridade ?? "Média",
            status: t.status ?? t.Status ?? "Em andamento",
            solicitante: t.solicitante ?? t.Solicitante,
            abertoHa: t.abertoHa,
            sugestaoIa: t.sugestaoIa
        }));

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
                solicitante: "Maria Silva",
                abertoHa: "há 25 min",
                sugestaoIa:
                    "Verificar credenciais de AD, política de acesso e status do servidor TS."
            },
            {
                id: 1019,
                titulo: "Lentidão no sistema de folha",
                prioridade: "Média",
                status: "Aguardando análise",
                solicitante: "Depto RH",
                abertoHa: "Hoje, 08:12",
                sugestaoIa:
                    "Conferir uso de CPU e memória no servidor, além de índices do banco."
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
