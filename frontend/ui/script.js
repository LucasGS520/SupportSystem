/**
 * Camada de apresentação responsável por orquestrar a experiência autenticada do SupportSystem.
 */

/**
 * Estado global responsável por armazenar sessão, usuário autenticado e tickets carregados.
 */
const state = {
    token: null,
    user: null,
    tickets: [],
    selectedPriority: 1
};

/**
 * Chave utilizada no armazenamento local para preservar a sessão JWT.
 */
const storageKey = "supportSystem.jwt";

/**
 * Executa a configuração inicial após o carregamento do DOM.
 */
document.addEventListener("DOMContentLoaded", () => {
    setupNavigation();
    setupModal();
    setupPrivacyControls();
    setupAuthForms();
    setupGlobalActions();
    restoreSessionFromStorage();
});

/**
 * Reaproveita a sessão armazenada ou exibe a tela de autenticação.
 */
function restoreSessionFromStorage() {
    const storedToken = localStorage.getItem(storageKey);
    if (!storedToken) {
        applyAuthenticatedState(false);
        return;
    }

    setAuthenticatedSession(storedToken, false);
    loadTicketsFromApi();
}

/**
 * Atualiza o estado com o token fornecido e aciona carregamento de dados quando necessário.
 */
function setAuthenticatedSession(token, triggerLoad = true) {
    if (!token) {
        showAuthMessage("loginFeedback", "Token de autenticação inválido.", "error");
        return;
    }

    try {
        state.token = token;
        state.user = decodeToken(token);
        localStorage.setItem(storageKey, token);
        applyAuthenticatedState(true);
        updateUserHeader();

        if (triggerLoad) {
            loadTicketsFromApi();
        }
    } catch (error) {
        console.error("Falha ao processar token JWT", error);
        showAuthMessage("loginFeedback", "Não foi possível validar a sessão recebida.", "error");
        clearSession();
    }
}

/**
 * Remove qualquer sessão ativa e restaura a interface para o estado público.
 */
function clearSession() {
    localStorage.removeItem(storageKey);
    state.token = null;
    state.user = null;
    state.tickets = [];
    state.selectedPriority = 1;
    applyAuthenticatedState(false);
    updateUserHeader();
    clearTicketViews();
    toggleAuthCard("login");
}

/**
 * Alterna entre a interface autenticada (app) e a tela de login/cadastro.
 */
function applyAuthenticatedState(authenticated) {
    const authShell = document.getElementById("authShell");
    const appShell = document.getElementById("appShell");

    if (authenticated) {
        document.body.classList.add("app-ready");
        authShell?.classList.add("hidden");
        appShell?.classList.remove("hidden");
    } else {
        document.body.classList.remove("app-ready");
        authShell?.classList.remove("hidden");
        appShell?.classList.add("hidden");
    }
}

/**
 * Atualiza cabeçalho com nome, e-mail e avatar do usuário autenticado.
 */
function updateUserHeader() {
    const nameSpan = document.getElementById("userNameDisplay");
    const emailSpan = document.getElementById("userEmailDisplay");
    const avatar = document.getElementById("userAvatar");

    if (!state.user) {
        nameSpan && (nameSpan.textContent = "Usuário autenticado");
        emailSpan && (emailSpan.textContent = "");
        avatar && (avatar.textContent = "US");
        return;
    }

    const { name, email } = state.user;
    nameSpan && (nameSpan.textContent = name || "Usuário autenticado");
    emailSpan && (emailSpan.textContent = email || "");
    avatar && (avatar.textContent = buildInitials(name || email || "US"));    

}

/**
 * Configura navegação lateral e rodapé mobile.
 */
function setupNavigation() {
    document.querySelectorAll(".nav-item").forEach((btn) => {
        btn.addEventListener("click", () => activatePage(btn.dataset.page));
    });

    document.querySelectorAll(".mobile-nav-item").forEach((btn) => {
        btn.addEventListener("click", () => activatePage(btn.dataset.page));
    });

    const btnVerTodos = document.getElementById("btnVerTodos");
    btnVerTodos?.addEventListener("click", () => activatePage("tickets"));
}

/**
 * Ativa a página indicada e destaca os itens de navegação.
 */
function activatePage(pageName) {
    document.querySelectorAll(".page").forEach((page) => {
        page.classList.toggle("active", page.id === `page-${pageName}`);
    });

    document.querySelectorAll("[data-page]").forEach((btn) => {
        btn.classList.toggle("active", btn.dataset.page === pageName);
    });
}

/**
 * Configura formulários de login e cadastro com alternância entre cartões.
 */
function setupAuthForms() {
    const loginForm = document.getElementById("loginForm");
    const registerForm = document.getElementById("registerForm");
    const showRegister = document.getElementById("showRegister");
    const showLogin = document.getElementById("showLogin");

    loginForm?.addEventListener("submit", async (event) => {
        event.preventDefault();
        showAuthMessage("loginFeedback", "");

        const formData = new FormData(loginForm);
        const payload = {
            Email: formData.get("email")?.toString().trim(),
            Senha: formData.get("senha")?.toString()
        };

        if (!payload.Email || !payload.Senha) {
            showAuthMessage("loginFeedback", "Informe e-mail e senha para continuar.", "error");
            return;
        }

        try {
            const response = await fetch("/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            const result = await response.json();
            const sucesso = result.Sucesso ?? result.sucesso;
            const token = result.Token ?? result.token;
            const mensagem = result.Mensagem ?? result.mensagem;

            if (!response.ok || !sucesso || !token) {
                showAuthMessage("loginFeedback", mensagem ?? "Credenciais inválidas.", "error");
                return;
            }

            setAuthenticatedSession(token);
        } catch (error) {
            console.error("Erro ao autenticar", error);
            showAuthMessage("loginFeedback", "Falha ao conectar com o servidor. Tente novamente.", "error");
        }
    });

    registerForm?.addEventListener("submit", async (event) => {
        event.preventDefault();
        showAuthMessage("registerFeedback", "");

        const formData = new FormData(registerForm);
        const consent = formData.get("consentimento") === "on";

        if (!consent) {
            showAuthMessage("registerFeedback", "É necessário consentir com a política de privacidade.", "error");
            return;
        }

        const payload = {
            Nome: formData.get("nome")?.toString().trim(),
            Email: formData.get("email")?.toString().trim(),
            Senha: formData.get("senha")?.toString(),
            ConsentimentoDados: consent
        };

        if (!payload.Nome || !payload.Email || !payload.Senha) {
            showAuthMessage("registerFeedback", "Preencha todos os campos obrigatórios.", "error");
            return;
        }

        try {
            const response = await fetch("/api/auth/register", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            const result = await response.json();
            const sucesso = result.Sucesso ?? result.sucesso;
            const token = result.Token ?? result.token;
            const mensagem = result.Mensagem ?? result.mensagem;

            if (!response.ok || !sucesso || !token) {
                showAuthMessage("registerFeedback", mensagem ?? "Não foi possível concluir o cadastro.", "error");
                return;
            }

            showAuthMessage("registerFeedback", "Cadastro realizado com sucesso!", "success");
            setAuthenticatedSession(token);
        } catch (error) {
            console.error("Erro ao registrar usuário", error);
            showAuthMessage("registerFeedback", "Falha ao contatar o servidor. Tente novamente.", "error");
        }
    });

    showRegister?.addEventListener("click", () => toggleAuthCard("register"));
    showLogin?.addEventListener("click", () => toggleAuthCard("login"));
}

/**
 * Exibe o cartão de autenticação indicado.
 */
function toggleAuthCard(target) {
    const loginCard = document.getElementById("loginCard");
    const registerCard = document.getElementById("registerCard");

    if (target === "register") {
        loginCard?.classList.add("hidden");
        registerCard?.classList.remove("hidden");
    } else {
        registerCard?.classList.add("hidden");
        loginCard?.classList.remove("hidden");
    }

    showAuthMessage("loginFeedback", "");
    showAuthMessage("registerFeedback", "");
}

/**
 * Apresenta mensagens de feedback nos cartões de autenticação.
 */
function showAuthMessage(elementId, message, type) {
    const element = document.getElementById(elementId);
    if (!element) {
        return;
    }

    element.textContent = message ?? "";
    element.classList.remove("error", "success");
    if (message && type) {
        element.classList.add(type);
    }
}

/**
 * Configura ações globais como logout e feedback de tickets.
 */
function setupGlobalActions() {
    const logoutButton = document.getElementById("btnLogout");
    logoutButton?.addEventListener("click", () => {
        clearSession();
        alert("Sessão encerrada. Faça login novamente para continuar.");
    });

    const ticketList = document.querySelector("#page-tickets .ticket-list");
    ticketList?.addEventListener("click", async (event) => {
        const target = event.target;
        if (!(target instanceof HTMLElement)) {
            return;
        }

        if (target.dataset.action === "feedback") {
            const ticketId = Number(target.dataset.ticketId);
            await handleFeedbackCapture(ticketId);
        }
    });
}

/**
 * Define interação do modal de novo chamado e suas validações.
 */
function setupModal() {
    const fab = document.getElementById("btnNovoChamado");
    const modal = document.getElementById("modalNovoChamado");
    const btnFechar = document.getElementById("btnFecharModal");
    const btnCancelar = document.getElementById("btnCancelarModal");
    const form = document.getElementById("formNovoChamado");
    const consentCheckbox = document.getElementById("ticketConsent");
    const submitButton = document.getElementById("btnCriarChamado");
    const chipsContainer = document.getElementById("ticketPriorityChips");

    if (!modal || !form || !consentCheckbox || !submitButton) {
        return;
    }

    const openModal = () => {
        resetNewTicketForm();
        modal.classList.add("active");
        form.ticketTitle?.focus();
    };

    const closeModal = () => {
        modal.classList.remove("active");
    };

    fab?.addEventListener("click", openModal);
    btnFechar?.addEventListener("click", closeModal);
    btnCancelar?.addEventListener("click", closeModal);

    modal.addEventListener("click", (event) => {
        if (event.target === modal) {
            closeModal();
        }
    });

    consentCheckbox.addEventListener("change", () => {
        submitButton.disabled = !consentCheckbox.checked;
    });

    chipsContainer?.addEventListener("click", (event) => {
        const target = event.target;
        if (!(target instanceof HTMLButtonElement)) {
            return;
        }

        const priority = Number(target.dataset.priority ?? "1");
        state.selectedPriority = Number.isNaN(priority) ? 1 : priority;

        chipsContainer.querySelectorAll(".chip").forEach((chip) => {
            chip.classList.toggle("chip-active", chip === target);
    
        });
    });

    form.addEventListener("submit", async (event) => {
        event.preventDefault();
        const created = await createTicketFromForm(form);
        if (created) {
            closeModal();
        }
    });
}

/**
 * Limpa campos do formulário de novo chamado e restabelece valores padrão.
 */
function resetNewTicketForm() {
    const form = document.getElementById("formNovoChamado");
    const consentCheckbox = document.getElementById("ticketConsent");
    const submitButton = document.getElementById("btnCriarChamado");
    const chipsContainer = document.getElementById("ticketPriorityChips");

    form?.reset();
    state.selectedPriority = 1;

    if (consentCheckbox instanceof HTMLInputElement) {
        consentCheckbox.checked = false;
    }
    if (submitButton instanceof HTMLButtonElement) {
        submitButton.disabled = true;
    }
    chipsContainer?.querySelectorAll(".chip").forEach((chip, index) => {
        chip.classList.toggle("chip-active", index === 0);
    });
}

/**
 * Configura botões de privacidade (exportar dados e esquecimento).
 */
function setupPrivacyControls() {
    const exportButton = document.getElementById("btnExportarDados");
    const deleteButton = document.getElementById("btnExcluirDados");

    exportButton?.addEventListener("click", async () => {
        await exportUserData();
    });

    deleteButton?.addEventListener("click", async () => {
        const confirmacao = window.confirm(
            "Tem certeza que deseja excluir todos os dados pessoais? Esta ação não pode ser desfeita."
        );

        if (!confirmacao) {
            return;
        }

        await deleteUserData();
    });
}

/**
 * Normaliza resposta da API para o formato utilizado na UI.
 */
function normalizeTicket(rawTicket) {
    const feedback = rawTicket.feedback ?? rawTicket.Feedback ?? null;
    const suggestionsRaw = rawTicket.suggestions ?? rawTicket.Suggestions ?? [];

    const normalizedSuggestions = Array.isArray(suggestionsRaw)
        ? suggestionsRaw.map((suggestion) => ({
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
        id: rawTicket.id ?? rawTicket.Id,
        titulo: rawTicket.titulo ?? rawTicket.Titulo ?? "Ticket",
        descricao: rawTicket.descricao ?? rawTicket.Descricao ?? null,
        prioridade: rawTicket.prioridade ?? rawTicket.Prioridade ?? "Média",
        status: rawTicket.status ?? rawTicket.Status ?? "Em andamento",
        ownerId: rawTicket.ownerId ?? rawTicket.OwnerId ?? null,
        assignedTechnicianId: rawTicket.assignedTechnicianId ?? rawTicket.AssignedTechnicianId ?? null,
        categoria: rawTicket.categoria ?? rawTicket.Categoria ?? "Outros",
        slaTarget: rawTicket.slaTarget ?? rawTicket.SlaTarget ?? null,
        solicitante: rawTicket.solicitante ?? rawTicket.Solicitante ?? null,
        abertoEm: rawTicket.abertoEm ?? rawTicket.AbertoEm ?? null,
        abertoHa: rawTicket.abertoHa ?? rawTicket.AbertoHa ?? null,
        sugestaoIa: rawTicket.sugestaoIa ?? rawTicket.SugestaoIa ?? null,
        feedback: normalizedFeedback,
        suggestions: normalizedSuggestions,
        consentimentoDados: rawTicket.consentimentoDados ?? rawTicket.ConsentimentoDados ?? false
    };
}

/**
 * Realiza chamadas autenticadas para a API backend.
 */
async function apiFetch(input, init = {}) {
    if (!state.token) {
        throw new Error("Sessão não encontrada para chamada autenticada.");
    }

    const headers = new Headers(init.headers ?? {});
    headers.set("Authorization", `Bearer ${state.token}`);
    if (init.body && !headers.has("Content-Type")) {
        headers.set("Content-Type", "application/json");
    }

    return fetch(input, { ...init, headers });
}

/**
 * Obtém tickets do backend e atualiza as visões.
 */
async function loadTicketsFromApi() {
    try {
        const response = await apiFetch("/api/tickets");

        if (response.status === 401) {
            handleUnauthorized();
            return;
        }

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();
        state.tickets = Array.isArray(data)
            ? data.map(normalizeTicket)
            : [];

        renderAllSections();
    } catch (error) {
        console.error("Erro ao carregar tickets da API", error);
        const fallbackTickets = buildFallbackTickets();
        state.tickets = fallbackTickets;
        renderAllSections();
    }
}

/**
 * Executa todas as rotinas de renderização após carregar os dados.
 */
function renderAllSections() {
    renderDashboardTickets(state.tickets);
    renderTicketsList(state.tickets);
    renderKnowledgeBase(state.tickets);
    updateDashboardMetrics(state.tickets);
    updateConsentStatusFromTickets(state.tickets);
}

/**
 * Popula a tabela do dashboard com os três tickets mais recentes.
 */
function renderDashboardTickets(tickets) {
    const tbody = document.querySelector(".data-table tbody");
    if (!tbody) {
        return;
    }

    tbody.innerHTML = "";

    tickets.slice(0, 3).forEach((ticket) => {
        const row = document.createElement("tr");
        row.innerHTML = `
            <td>${escapeHtml(String(ticket.id))}</td>
            <td>${escapeHtml(ticket.titulo)}</td>
            <td>${escapeHtml(ticket.solicitante ?? "Usuário")}</td>
            <td>
                <span class="tag ${mapPriorityTag(ticket.prioridade)}">${escapeHtml(ticket.prioridade)}</span>
            </td>
            <td>
                <span class="tag ${mapStatusTag(ticket.status)}">${escapeHtml(ticket.status)}</span>
            </td>
            <td>${escapeHtml(ticket.categoria ?? "Outros")}</td>
            <td>${formatSlaTarget(ticket.slaTarget)}</td>
        `;

        tbody.appendChild(row);
    });
}

/**
 * Renderiza a lista de tickets detalhados com sugestões e feedbacks.
 */
function renderTicketsList(tickets) {
    const list = document.querySelector("#page-tickets .ticket-list");
    if (!list) {
        return;
    }

    if (!tickets.length) {
        list.innerHTML = '<p class="empty-state">Nenhum chamado disponível. Crie o primeiro ticket.</p>';
        return;
    }

    list.innerHTML = "";

    tickets.forEach((ticket) => {
        const card = document.createElement("article");
        card.className = "ticket-card";

        const feedback = formatFeedback(ticket.feedback);
        const suggestionsSection = formatSuggestions(ticket.suggestions);
        const descricao = ticket.descricao ? `<p class="ticket-description">${escapeHtml(ticket.descricao)}</p>` : "";

        const feedbackButton = shouldDisplayFeedbackButton(ticket)
            ? `<button class="btn small secondary" data-action="feedback" data-ticket-id="${ticket.id}">Registrar feedback</button>`
            : `<button class="btn small secondary">Ver detalhes</button>`;

        card.innerHTML = `
            <header>
                <span class="ticket-id">#${escapeHtml(String(ticket.id))}</span>
                <span class="tag ${mapPriorityTag(ticket.prioridade)}">${escapeHtml(ticket.prioridade)}</span>
            </header>

            <h4>${escapeHtml(ticket.titulo)}</h4>
            ${descricao}

            <p class="ticket-meta">
                Aberto por <strong>${t.solicitante || "Usuário"}</strong>
                • ${t.abertoHa || "Hoje"}
            </p>

            <p class="ticket-meta">
                Categoria: <strong>${escapeHtml(ticket.categoria ?? "Outros")}</strong>
                • Técnico: <strong>${escapeHtml(formatTechnician(ticket.assignedTechnicianId))}</strong>
            </p>

            <p class="ticket-meta">
                SLA alvo: <strong>${formatSlaTarget(ticket.slaTarget)}</strong>
            </p>

            <p class="ticket-ai">
                💡 <strong>Sugestão em destaque:</strong>
                ${escapeHtml(ticket.sugestaoIa ?? "Sugestão automática de solução para o chamado.")}
            </p>

            ${suggestionsSection}
            ${feedback}

            <footer>
                <span class="tag ${mapStatusTag(ticket.status)}">${escapeHtml(ticket.status)}</span>
                ${feedbackButton}
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

/**
 * Define se o botão de feedback deve ser exibido para determinado ticket.
 */
function shouldDisplayFeedbackButton(ticket) {
    const status = (ticket.status ?? "").toLowerCase();
    const hasFeedback = Boolean(ticket.feedback?.comentario || ticket.feedback?.nota);
    return status.includes("resolvido") && !hasFeedback;
}

/**
 * Constrói a vitrine da base de conhecimento a partir das sugestões agregadas.
 */
function renderKnowledgeBase(tickets) {
    const container = document.querySelector("#page-knowledge .knowledge-list");
    if (!container) {
        return;
    }

    const aggregated = new Map();

    tickets.forEach((ticket) => {
        (ticket.suggestions ?? []).forEach((suggestion) => {
            const key = `${suggestion.titulo}::${suggestion.fonte}`;
            if (!aggregated.has(key)) {
                aggregated.set(key, suggestion);
            }
        });
    });

    if (!aggregated.size) {
        container.innerHTML = '<p class="empty-state">Nenhuma sugestão disponível no momento. Abra um chamado para receber recomendações.</p>';
        return;
    }

    const fragments = Array.from(aggregated.values()).map((suggestion) => `
        <article class="knowledge-card">
            <h4>${escapeHtml(suggestion.titulo)}</h4>
            <p>${escapeHtml(suggestion.descricao)}</p>
            <span class="suggestion-source">${escapeHtml(suggestion.fonte ?? "Base de conhecimento")}</span>
        </article>
    `);

    container.innerHTML = fragments.join("");
}

// Formata o identificador do técnico responsável
function formatTechnician(technicianId) {
    if (!technicianId) {
        return "Aguardando designação";
    }

    return `Técnico #${technicianId}`;
}

/**
 * Atualiza métricas do dashboard de acordo com os tickets carregados.
 */
function updateDashboardMetrics(tickets) {
    const totalAbertos = tickets.filter((ticket) => !ticket.status?.toLowerCase().includes("resolvido")).length;
    const criticos = tickets.filter((ticket) => ticket.status?.toLowerCase().includes("crítico") || ticket.status?.toLowerCase().includes("critico")).length;
    const emAndamento = tickets.filter((ticket) => ticket.status?.toLowerCase().includes("andamento")).length;

    const metricAbertos = document.getElementById("metricTotalAbertos");
    const metricCriticos = document.getElementById("metricCriticos");
    const metricTempoMedio = document.getElementById("metricTempoMedio");
    const metricSatisfacao = document.getElementById("metricSatisfacao");
    const metricSugestoes = document.getElementById("metricSugestoesIa");

    metricAbertos && (metricAbertos.textContent = String(totalAbertos));
    metricCriticos && (metricCriticos.textContent = `${criticos} críticos • ${emAndamento} em andamento`);

    const diffs = tickets
        .map((ticket) => (ticket.abertoEm ? Date.now() - new Date(ticket.abertoEm).getTime() : null))
        .filter((diff) => typeof diff === "number" && diff >= 0);
    const tempoMedio = diffs.length ? formatAverageDuration(diffs) : "--";
    metricTempoMedio && (metricTempoMedio.textContent = tempoMedio);

    const notas = tickets
        .map((ticket) => ticket.feedback?.nota)
        .filter((nota) => typeof nota === "number" && nota > 0);
    const mediaSatisfacao = notas.length ? Math.round((notas.reduce((acc, nota) => acc + nota, 0) / (notas.length * 5)) * 100) : null;
    metricSatisfacao && (metricSatisfacao.textContent = mediaSatisfacao === null ? "--" : `${mediaSatisfacao}%`);

    const ticketsComSugestao = tickets.filter((ticket) => Array.isArray(ticket.suggestions) && ticket.suggestions.length > 0).length;
    const percentualSugestoes = tickets.length ? Math.round((ticketsComSugestao / tickets.length) * 100) : null;
    metricSugestoes && (metricSugestoes.textContent = percentualSugestoes === null ? "--" : `${percentualSugestoes}%`);
}

/**
 * Ajusta o rótulo de consentimento no painel de privacidade.
 */
function updateConsentStatusFromTickets(tickets) {
    const consentStatus = document.getElementById("privacyConsentStatus");
    if (!consentStatus) {
        return;
    }

    const hasConsent = tickets.some((ticket) => ticket.consentimentoDados);
    consentStatus.textContent = hasConsent ? "Ativo" : "Revogado";
    consentStatus.classList.toggle("info", hasConsent);
    consentStatus.classList.toggle("danger", !hasConsent);
}

/**
 * Solicita criação de um novo ticket a partir dos dados do formulário.
 */
async function createTicketFromForm(form) {
    if (!state.token) {
        alert("Faça login para criar um chamado.");
        return false;
    }

const formData = new FormData(form);
    const slaValue = formData.get("ticketSla")?.toString();
    const slaTarget = slaValue ? new Date(slaValue).toISOString() : null;

    const payload = {
        Titulo: formData.get("ticketTitle")?.toString().trim(),
        Categoria: Number(formData.get("ticketCategory")),
        Prioridade: state.selectedPriority,
        Descricao: formData.get("ticketDescription")?.toString().trim() ?? null,
        SlaTarget: slaTarget,
        ConsentimentoDados: formData.get("ticketConsent") === "on"
    };

    if (!payload.Titulo || !payload.Descricao) {
        alert("Preencha título e descrição para registrar o chamado.");
        return false;
    }

    if (!payload.ConsentimentoDados) {
        alert("O consentimento é obrigatório para abertura do chamado.");
        return false;
    }

    try {
        const response = await apiFetch("/api/tickets", {
            method: "POST",
            body: JSON.stringify(payload)
        });

        if (response.status === 401) {
            handleUnauthorized();
            return;
        }

        if (response.status === 400) {
            const problem = await response.json();
            const message = problem.mensagem ?? "Falha ao criar o chamado.";
            alert(message);
            return false;
        }

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const created = await response.json();
        const ticket = normalizeTicket(created);
        state.tickets.unshift(ticket);
        renderAllSections();
        alert("Chamado criado com sucesso! As sugestões serão processadas em instantes.");
        return true;
    } catch (error) {
        console.error("Erro ao criar ticket", error);
        alert("Não foi possível criar o chamado neste momento.");
        return false;
    }
}

/**
 * Solicita o feedback do usuário via prompts simples e envia atualização ao backend.
 */
async function handleFeedbackCapture(ticketId) {
    if (!state.token) {
        alert("Faça login para registrar feedback.");
        return;
    }

    const notaEntrada = window.prompt("Qual a nota do atendimento? (1 a 5)");
    if (notaEntrada === null) {
        return;
    }

    const nota = Number(notaEntrada);
    if (!Number.isFinite(nota) || nota < 1 || nota > 5) {
        alert("Informe uma nota entre 1 e 5.");
        return;
    }

    const comentario = window.prompt("Descreva brevemente sua experiência (opcional)") ?? "";

    const payload = {
        Feedback: {
            Nota: nota,
            Comentario: comentario.trim() || null,
            RegistradoEm: new Date().toISOString()
        }
    };

    try {
        const response = await apiFetch(`/api/tickets/${ticketId}`, {
            method: "PUT",
            body: JSON.stringify(payload)
        });

        if (response.status === 401) {
            handleUnauthorized();
            return;
        }

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        await loadTicketsFromApi();
        alert("Feedback registrado com sucesso!");
    } catch (error) {
        console.error("Erro ao registrar feedback", error);
        alert("Não foi possível registrar o feedback agora.");
    }
}

/**
 * Exporta os dados pessoais do usuário autenticado.
 */
async function exportUserData() {
    if (!state.token) {
        alert("Faça login para solicitar a exportação.");
        return;
    }

    try {
        const response = await apiFetch("/api/privacy/export", { method: "GET" });

        if (response.status === 401) {
            handleUnauthorized();
            return;
        }

        if (response.status === 404) {
            alert("Usuário não encontrado para exportação.");
            return;
        }

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        const data = await response.json();
        console.log("Exportação de dados concluída:", data);
        alert("Exportação concluída! Consulte o console para visualizar o JSON retornado.");
    } catch (error) {
        console.error("Erro ao exportar dados pessoais", error);
        alert("Não foi possível exportar os dados no momento.");
    }
}

/**
 * Solicita o direito ao esquecimento e encerra a sessão.
 */
async function deleteUserData() {
    if (!state.token) {
        alert("Faça login para excluir seus dados.");
        return;
    }

    try {
        const response = await apiFetch("/api/privacy/forget-me", { method: "DELETE" });

        if (response.status === 401) {
            handleUnauthorized();
            return;
        }

        if (response.status === 404) {
            alert("Nenhum dado pessoal foi localizado para exclusão.");
            return;
        }

        if (!response.ok) {
            throw new Error(`HTTP ${response.status}`);
        }

        alert("Dados pessoais excluídos com sucesso. Sua sessão será encerrada.");
        clearSession();
    } catch (error) {
        console.error("Erro ao excluir dados pessoais", error);
        alert("Não foi possível excluir os dados agora. Tente mais tarde.");
    }
}

/**
 * Trata respostas não autorizadas limpando a sessão vigente.
 */
function handleUnauthorized() {
    clearSession();
    showAuthMessage("loginFeedback", "Sua sessão expirou. Entre novamente.", "error");
}

/**
 * Constrói tickets de exemplo quando a API não estiver acessível.
 */
function buildFallbackTickets() {
    return [
        normalizeTicket({
            Id: 1023,
            Titulo: "Erro ao acessar sistema via VPN",
            Prioridade: "Alta",
            Status: "Em andamento",
            OwnerId: 1,
            AssignedTechnicianId: 3,
            Categoria: "Infraestrutura e redes",
            Descricao: "Usuária relata falha ao autenticar na VPN corporativa após troca de senha.",
            SlaTarget: new Date().toISOString(),
            Solicitante: "Maria Silva",
            AbertoHa: "há 25 min",
            SugestaoIa: "Verificar credenciais de AD, política de acesso e status do servidor TS.",
            Feedback: {
                Nota: 5,
                Comentario: "Atendimento rápido e cordial.",
                RegistradoEm: new Date().toISOString()
            },
            Suggestions: [
                {
                    Titulo: "Sugestão da IA",
                    Descricao: "Verifique sincronização de credenciais, status do servidor VPN e políticas de firewall.",
                    Fonte: "Assistente virtual"
                },
                {
                    Titulo: "Erro de VPN após atualização de credenciais",
                    Descricao: "Revisar políticas de acesso no AD, confirmar sincronização de credenciais e reiniciar o serviço VPN.",
                    Fonte: "Base de conhecimento"
                }
            ],
            ConsentimentoDados: true
        }),
        normalizeTicket({
            Id: 1019,
            Titulo: "Lentidão no sistema de folha",
            Prioridade: "Média",
            Status: "Aguardando análise",
            OwnerId: 2,
            Categoria: "Aplicações",
            Descricao: "Equipe de RH reporta demora ao calcular folha durante o fechamento do mês.",
            SlaTarget: null,
            Solicitante: "Depto RH",
            AbertoHa: "Hoje, 08:12",
            SugestaoIa: "Conferir uso de CPU e memória no servidor, além de índices do banco.",
            Suggestions: [
                {
                    Titulo: "Lentidão em aplicação web",
                    Descricao: "Validar utilização de recursos no servidor, analisar métricas de banco e aplicar índices recomendados.",
                    Fonte: "Base de conhecimento"
                }
            ],
            ConsentimentoDados: true
        })
    ];
}

/**
 * Limpa componentes visuais quando não há sessão ativa.
 */
function clearTicketViews() {
    const tbody = document.querySelector(".data-table tbody");
    const list = document.querySelector("#page-tickets .ticket-list");
    const knowledge = document.querySelector("#page-knowledge .knowledge-list");

    tbody && (tbody.innerHTML = "");
    list && (list.innerHTML = '<p class="empty-state">Faça login para visualizar seus chamados.</p>');
    knowledge && (knowledge.innerHTML = '<p class="empty-state">A base de conhecimento será exibida após o login.</p>');
    updateDashboardMetrics([]);
    updateConsentStatusFromTickets([]);
}

/**
 * Aplica proteção contra XSS em textos exibidos dinamicamente.
 */
function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

/**
 * Mapeia prioridades para classes de cor da interface.
 */
function mapPriorityTag(priority) {
    if (!priority) {
        return "info";
    }
    const normalized = priority.toLowerCase();
    if (normalized.includes("alta")) return "danger";
    if (normalized.includes("média") || normalized.includes("media")) return "medium";
    if (normalized.includes("baixa")) return "info";
    return "info";
}

/**
 * Mapeia status para classes de cor da interface.
 */
function mapStatusTag(status) {
    if (!status) {
        return "info";
    }
    const normalized = status.toLowerCase();
    if (normalized.includes("crit")) return "danger";
    if (normalized.includes("andamento")) return "warning";
    if (normalized.includes("aguard")) return "info";
    if (normalized.includes("resol")) return "success";
    return "info";
}

/**
 * Formata o SLA em horário local.
 */
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

/**
 * Recupera identificador do técnico para exibição.
 */
function formatTechnician(technicianId) {
    return technicianId ? `Técnico #${technicianId}` : "Aguardando designação";
}

/**
 * Converte feedback em bloco de texto com informações relevantes.
 */
function formatFeedback(feedback) {
    if (!feedback) {
        return "";
    }

    const items = [];
    if (feedback.nota) {
        items.push(`Nota: ${feedback.nota}/5`);
    }
    if (feedback.comentario) {
        items.push(`Comentário: ${escapeHtml(feedback.comentario)}`);
    }
    if (feedback.registradoEm) {
        items.push(`Registrado em: ${formatSlaTarget(feedback.registradoEm)}`);
    }

    if (!items.length) {
        return "";
    }

    return `
        <p class="ticket-feedback">
            <strong>Feedback:</strong> ${items.join(" • ")}
        </p>
    `;
}

// Monta a lista de sugestões combinando IA e base de conhecimento
function formatSuggestions(suggestions) {
    if (!Array.isArray(suggestions) || !suggestions.length) {
        return "";
    }

    const items = suggestions
        .map((suggestion) => `
            <li>
                <span class="suggestion-source">${escapeHtml(suggestion.fonte ?? "Base de conhecimento")}</span>
                <div>
                    <strong>${escapeHtml(suggestion.titulo)}</strong>
                    <p>${escapeHtml(suggestion.descricao)}</p>
                </div>
            </li>
        `)
        .join("");

    return `
        <section class="ticket-suggestions">
            <h5>📚 Sugestões relacionadas</h5>
            <ul>${items}</ul>
        </section>
    `;
}

/**
 * Gera iniciais para o avatar exibido no topo.
 */
function buildInitials(text) {
    if (!text) {
        return "US";
    }

    const parts = text.trim().split(/\s+/).slice(0, 2);
    const initials = parts.map((part) => part.charAt(0)).join("");
    return initials.toUpperCase() || "US";
}

/**
 * Decodifica o token JWT para obter claims relevantes.
 */
function decodeToken(token) {
    const segments = token.split(".");
    if (segments.length < 2) {
        throw new Error("Token JWT inválido");
    }

    const payloadSegment = segments[1].replace(/-/g, "+").replace(/_/g, "/");
    const decoded = atob(payloadSegment);
    const payload = JSON.parse(decoded);

    return {
        id: Number(payload.sub ?? payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] ?? 0),
        name:
            payload.unique_name ??
            payload.name ??
            payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ??
            "Usuário autenticado",
        email: payload.email ?? payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"] ?? "",
        role: payload.role ?? payload["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] ?? "user"
    };
}

/**
 * Calcula a média de durações (em milissegundos) e formata em horas/minutos.
 */
function formatAverageDuration(diffList) {
    if (!diffList.length) {
        return "--";
    }
    
    const averageMs = diffList.reduce((acc, value) => acc + value, 0) / diffList.length;
    const minutes = Math.floor(averageMs / 60000);
    if (minutes < 60) {
        return `${minutes} min`;
    }
    const hours = Math.floor(minutes / 60);
    const remainingMinutes = minutes % 60;
    return `${hours}h ${String(remainingMinutes).padStart(2, "0")}min`;
}

/**
 * Converte uma data para tempo relativo amigável.
 */
function formatRelativeTime(value) {
    if (!value) {
        return null;
    }
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return null;
    }
    const diffMs = Date.now() - date.getTime();
    if (diffMs < 60_000) {
        return "agora mesmo";
    }
    if (diffMs < 3_600_000) {
        const minutes = Math.max(1, Math.round(diffMs / 60_000));
        return `há ${minutes} min`;
    }
    if (diffMs < 86_400_000) {
        const hours = Math.max(1, Math.round(diffMs / 3_600_000));
        return `há ${hours}h`;
    }
    const days = Math.max(1, Math.round(diffMs / 86_400_000));
    return days === 1 ? "há 1 dia" : `há ${days} dias`;
}