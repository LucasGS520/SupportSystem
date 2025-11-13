# SupportSystem

API de suporte/tickets (sistema de chamados) escrita em .NET para um sistema integrado multiplataforma para a gestão de chamados de suporte Técnico, com módulo de Inteligência Artificial que auxilie na classificação, priorização e sugestão de soluções para problemas recorrentes.

## 📋 Descrição

Sistema completo de gerenciamento de tickets de suporte com as seguintes funcionalidades:

- ✅ Abertura de chamados via plataforma web, desktop e mobile
- 🤖 Classificação e priorização automática de chamados com IA
- 📚 Base de conhecimento integrada com sugestões automáticas de soluções
- 📊 Relatórios gerenciais e indicadores de desempenho (KPIs)
- 🔒 Segurança e conformidade com a LGPD

## 🏗️ Arquitetura

O projeto segue os princípios de Clean Architecture com separação em camadas:

```
SupportSystem/
├── src/
│   ├── SupportSystem.Api/          # API REST (Controllers, Program.cs)
│   ├── SupportSystem.Application/  # Lógica de negócio (Services, DTOs)
│   ├── SupportSystem.Domain/       # Entidades e Enums
│   └── SupportSystem.Infrastructure/ # Acesso a dados (DbContext, Repositories)
└── tests/
    └── SupportSystem.Tests/        # Testes unitários
```

## 🚀 Tecnologias Utilizadas

- **.NET 9.0** - Framework principal
- **Entity Framework Core 9.0** - ORM para acesso a dados
- **ML.NET 3.0** - Machine Learning para classificação de tickets
- **Swagger/OpenAPI** - Documentação da API
- **xUnit** - Framework de testes
- **In-Memory Database** - Banco de dados em memória para desenvolvimento

## 📦 Funcionalidades Implementadas

### 1. Gerenciamento de Tickets
- Criação de tickets com informações detalhadas
- Atribuição automática de categoria e prioridade usando IA
- Atualização de status (Aberto, Em Progresso, Aguardando Cliente, Resolvido, Fechado)
- Atribuição de tickets para agentes
- Comentários em tickets (públicos e internos)

### 2. Gestão de Clientes
- Cadastro de clientes com consentimento LGPD
- Histórico de tickets por cliente
- Solicitação de exclusão de dados (LGPD)

### 3. Base de Conhecimento
- Artigos organizados por categoria
- Busca por palavras-chave
- Sugestões automáticas baseadas em descrição do ticket
- Contagem de visualizações e avaliações de utilidade

### 4. Relatórios e KPIs
- KPIs gerenciais (tickets totais, abertos, resolvidos, etc.)
- Tempo médio de resolução
- Tickets por categoria e prioridade
- Performance de agentes
- Taxa de retenção de clientes

### 5. Inteligência Artificial
- Classificação automática de categoria do ticket
- Priorização automática baseada em palavras-chave
- Sugestão de artigos da base de conhecimento

### 6. LGPD (Lei Geral de Proteção de Dados)
- Campos de consentimento de processamento de dados
- Data de expiração de retenção de dados
- Solicitação de exclusão de dados
- Rastreamento de consentimento

## 🔧 Como Executar

### Pré-requisitos
- .NET SDK 9.0 ou superior

### Passos

1. Clone o repositório
```bash
git clone https://github.com/LucasGS520/SupportSystem.git
cd SupportSystem
```

2. Restore as dependências
```bash
dotnet restore
```

3. Compile o projeto
```bash
dotnet build
```

4. Execute os testes
```bash
dotnet test
```

5. Execute a API
```bash
cd src/SupportSystem.Api
dotnet run
```

6. Acesse a documentação Swagger
```
https://localhost:5001/swagger
```

## 📚 Endpoints da API

### Tickets
- `POST /api/tickets` - Criar um novo ticket
- `GET /api/tickets` - Listar todos os tickets
- `GET /api/tickets/{id}` - Obter ticket por ID
- `GET /api/tickets/customer/{customerId}` - Listar tickets de um cliente
- `PATCH /api/tickets/{id}/status` - Atualizar status do ticket
- `POST /api/tickets/{ticketId}/assign/{userId}` - Atribuir ticket a um agente
- `POST /api/tickets/{ticketId}/comments` - Adicionar comentário ao ticket

### Clientes
- `GET /api/customers` - Listar todos os clientes
- `GET /api/customers/{id}` - Obter cliente por ID
- `POST /api/customers` - Criar novo cliente
- `POST /api/customers/{id}/request-deletion` - Solicitar exclusão de dados (LGPD)

### Base de Conhecimento
- `GET /api/knowledgebase` - Listar artigos publicados
- `GET /api/knowledgebase/{id}` - Obter artigo por ID
- `POST /api/knowledgebase` - Criar novo artigo
- `POST /api/knowledgebase/{id}/helpful` - Marcar artigo como útil
- `POST /api/knowledgebase/suggest` - Obter sugestões de artigos

### Relatórios
- `GET /api/reports/kpis` - Obter KPIs e métricas de desempenho
- `GET /api/reports/satisfaction` - Obter métricas de satisfação do cliente
- `GET /api/reports/agent-performance` - Obter performance dos agentes

## 🧪 Testes

O projeto inclui testes unitários para validar a classificação automática de tickets:

```bash
dotnet test --verbosity normal
```

## 🔐 Segurança e LGPD

O sistema implementa as seguintes medidas de conformidade com a LGPD:

1. **Consentimento Explícito**: Campos para registrar consentimento do usuário
2. **Retenção de Dados**: Data de expiração automática de dados pessoais
3. **Direito ao Esquecimento**: Endpoint para solicitação de exclusão de dados
4. **Rastreamento**: Logs de quando o consentimento foi dado

## 🌐 Suporte Multi-Plataforma

A API é acessível via:
- **Web**: Aplicações web usando fetch/axios
- **Desktop**: Aplicações desktop .NET, Electron, etc.
- **Mobile**: Apps iOS/Android via HTTP
- CORS configurado para permitir acesso de qualquer origem

## 📈 Próximos Passos

- [ ] Implementar autenticação JWT
- [ ] Adicionar banco de dados SQL Server/PostgreSQL
- [ ] Melhorar modelo de ML com treinamento personalizado
- [ ] Adicionar notificações em tempo real (SignalR)
- [ ] Implementar upload de anexos em tickets
- [ ] Dashboard web para visualização de KPIs

## 📄 Licença

Este projeto está sob licença MIT.

## 👥 Contribuindo

Contribuições são bem-vindas! Por favor, abra uma issue ou pull request.
