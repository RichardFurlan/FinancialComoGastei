# Como Gasto Minha Grana — Análise de Estado e Roteiro de Deploy

> Documento gerado em: 2026-05-30

---

## 1. O que foi construído

### Backend (.NET 9 — Clean Architecture)

| Feature | Status | Endpoint(s) |
|---|---|---|
| Auth (registro, login, logout) | ✅ | `POST /api/auth/register`, `/login`, `/logout`, `GET /api/auth/me` |
| Upload de extratos (PDF, TXT, Excel) | ✅ | `POST /api/statements/upload` |
| Processamento assíncrono via Worker | ✅ | RabbitMQ → `ProcessStatementConsumer` |
| Extração de transações (DeepSeek/OpenRouter) | ✅ | Worker → `IAIService.ExtractTransactionsAsync` |
| Análise financeira IA (markdown) | ✅ | Worker → `IAIService.GenerateAnalysisAsync` |
| Cache de análise (Redis, TTL 7 dias) | ✅ | `IAnalysisCacheService` |
| Sanitizador de PII (CPF, CNPJ, cartão) | ✅ | `ISanitizerService` |
| Gestão de categorias (CRUD, multi-tenant) | ✅ | `GET/POST/PUT/DELETE /api/categories` |
| Categorização de transações | ✅ | `PATCH /api/transactions/{id}/category` |
| Regras de Ouro (auto-categorização) | ✅ | `GET/POST/PUT/DELETE /api/category-rules` |
| Aplicar regras a extrato existente | ✅ | `POST /api/statements/{id}/apply-rules` |
| Exclusão de extratos (cascade) | ✅ | `DELETE /api/statements/{id}` |
| Export CSV/Excel/PDF | ✅ | `GET /api/statements/{id}/export?format=csv\|xlsx\|pdf` |
| Relatórios (agregação, até 6 imports) | ✅ | `GET/POST/DELETE /api/reports`, `GET /api/reports/{id}` |
| Cache de relatórios (Redis, TTL 1h) | ✅ | `IReportCacheService` |
| ValidationBehavior (FluentValidation no MediatR) | ✅ | `Common/Behaviors/ValidationBehavior.cs` |
| Global exception handler | ✅ | `API/Middleware/ExceptionHandlingMiddleware.cs` |
| CORS configurável | ✅ | `Cors:AllowedOrigins` no appsettings |

### Frontend (Nuxt 3 + DaisyUI)

| Página | Status |
|---|---|
| Login / Registro | ✅ `/login` |
| Dashboard (lista de extratos + upload) | ✅ `/` |
| Detalhe do extrato (transações + categoria + gráfico pizza + análise IA) | ✅ `/statements/[id]` |
| Exportar extrato (CSV/Excel/PDF) | ✅ (botão dropdown no detalhe) |
| Gestão de categorias | ✅ `/categories` |
| Regras de Ouro | ✅ `/rules` |
| Relatórios | ✅ `/reports` |

### Infraestrutura (Docker Compose)

| Serviço | Container | Propósito |
|---|---|---|
| PostgreSQL 17 | `cgmg-db` | Banco de dados principal |
| Redis 7 | `cgmg-redis` | Cache (análises + relatórios) |
| RabbitMQ 3 | `cgmg-rabbitmq` | Fila de mensagens |
| API .NET 9 | `cgmg-api` | Backend REST |
| Worker .NET 9 | `cgmg-worker` | Processamento assíncrono |
| Nuxt 3 UI | `cgmg-ui` | Frontend |
| Nginx | `cgmg-nginx` | Reverse proxy |
| Seq | `cgmg-seq` | Agregação de logs |
| Cloudflare Tunnel | `cgmg-cloudflared` | Exposição segura na internet |

---

## 2. Gaps antes do deploy

### 🔴 Críticos — bloqueia produção

#### 2.1 Serilog não integrado
O `ILogger<T>` do .NET está em uso (logs aparecem no console), mas **não há Serilog configurado**. Isso significa que o Seq estará vazio em produção.

**O que falta:**
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Sinks.Console
```
Em `Program.cs`:
```csharp
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .WriteTo.Console()
       .WriteTo.Seq(ctx.Configuration["Seq:Url"] ?? "http://seq:5341"));
```
Em `appsettings.json`: adicionar `"Seq": { "Url": "http://seq:5341" }`.
No `docker-compose.yml`: adicionar `Seq__Url=http://seq:5341` nos serviços API e Worker.

#### 2.2 Senhas hardcoded no docker-compose
O `docker-compose.yml` usa credenciais padrão:
- PostgreSQL: `admin / password123`
- RabbitMQ: `guest / guest`

**O que falta:** Mover para `.env` com senhas fortes:
```env
POSTGRES_PASSWORD=senha_forte_aqui
RABBITMQ_PASSWORD=outra_senha_forte
```
E referenciar no docker-compose: `${POSTGRES_PASSWORD}`, `${RABBITMQ_PASSWORD}`.

#### 2.3 Migrations não rodam automaticamente
Não há script de inicialização. Se o banco estiver vazio, a API vai quebrar ao tentar inserir dados.

**O que falta:** Adicionar ao `Program.cs` da API antes de `app.Run()`:
```csharp
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
db.Database.Migrate();
```

#### 2.4 `NUXT_PUBLIC_API_BASE` não configurada no docker-compose
O `docker-compose.yml` não injeta a variável de ambiente no serviço `ui`. O frontend vai tentar acessar `http://localhost:5209` em vez de usar a URL do Nginx.

**O que falta:** Adicionar ao serviço `ui` no docker-compose:
```yaml
ui:
  environment:
    - NUXT_PUBLIC_API_BASE=http://nginx
```
Ou usar URL relativa (vazio) se o frontend e API ficarem no mesmo domínio via Nginx.

#### 2.5 Cloudflare Tunnel — configurar antes do deploy
Sem `CF_TUNNEL_TOKEN`, o serviço `cloudflared` falha silenciosamente e a app não fica acessível externamente.

**O que falta:** Seguir o guia em `docs/SEQ-SETUP.md` (seção 1–2) para criar o tunnel e obter o token. Depois adicionar ao `.env`.

---

### 🟡 Importantes — não bloqueia, mas deveria ter antes de uso real

#### 2.6 Rate limiting na API
O endpoint `POST /api/statements/upload` pode ser abusado. Sem rate limiting, qualquer usuário autenticado pode fazer uploads massivos.

**Solução simples:**
```csharp
// Program.cs
builder.Services.AddRateLimiter(opt =>
    opt.AddFixedWindowLimiter("upload", cfg =>
    {
        cfg.PermitLimit = 10;
        cfg.Window = TimeSpan.FromMinutes(1);
    }));
// No controller:
[EnableRateLimiting("upload")]
```

#### 2.7 Health check endpoint
Útil para o Cloudflare e para monitoramento geral.

```csharp
app.MapHealthChecks("/healthz");
builder.Services.AddHealthChecks()
    .AddNpgsql(connectionString)
    .AddRedis(redisConnection);
```

#### 2.8 Backup do PostgreSQL
Não há estratégia de backup configurada.

**Sugestão rápida:** Adicionar um cron job na VPS:
```bash
0 3 * * * docker exec cgmg-db pg_dump -U admin comogasto_db | gzip > /backups/db_$(date +%Y%m%d).sql.gz
```

---

## 3. Variáveis de ambiente necessárias

Crie `.env` na raiz do projeto (a partir de `.env.example`):

| Variável | Obrigatória | Onde obter | Padrão atual |
|---|---|---|---|
| `OPENROUTER_API_KEY` | ✅ Sim | [openrouter.ai/keys](https://openrouter.ai/keys) | — |
| `CF_TUNNEL_TOKEN` | ✅ Para produção | Cloudflare → Zero Trust → Tunnels | — |
| `POSTGRES_PASSWORD` | ✅ Para produção | Defina uma senha forte | `password123` (inseguro) |
| `RABBITMQ_PASSWORD` | ✅ Para produção | Defina uma senha forte | `guest` (inseguro) |
| `NUXT_PUBLIC_API_BASE` | ✅ No docker-compose | URL do Nginx ou domínio | `http://localhost:5209` |
| `Seq__Url` | Opcional | URL do container Seq | — (logs só no console) |

---

## 4. Roteiro de deploy — passo a passo

### Pré-requisitos
- [ ] VPS com Ubuntu 22+ e Docker instalado
- [ ] Domínio apontando para Cloudflare (nameservers)
- [ ] Conta no Cloudflare (gratuita) com Zero Trust ativo
- [ ] Chave OpenRouter

### Passo 1 — Preparar a VPS

```bash
# Instalar Docker e Docker Compose
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Clonar o projeto
git clone <repo-url> cgmg && cd cgmg/"Como gasto minha grana"
```

### Passo 2 — Configurar variáveis

```bash
cp .env.example .env
nano .env
# Preencher: OPENROUTER_API_KEY, POSTGRES_PASSWORD, RABBITMQ_PASSWORD, CF_TUNNEL_TOKEN
```

Atualizar `docker-compose.yml`:
- Trocar `password123` por `${POSTGRES_PASSWORD}`
- Trocar `guest` (RabbitMQ) por `${RABBITMQ_PASSWORD}`
- Adicionar `NUXT_PUBLIC_API_BASE=http://nginx` ao serviço `ui`

### Passo 3 — Subir a stack

```bash
# Subir infraestrutura primeiro
docker compose up -d db redis rabbitmq
sleep 10  # aguardar postgres estar pronto

# Aplicar migrations
docker compose run --rm api dotnet ef database update \
  --project src/ComoGastoMinhaGrana.Infrastructure \
  --startup-project src/ComoGastoMinhaGrana.Api

# Subir tudo
docker compose up -d
```

### Passo 4 — Configurar Cloudflare Tunnel

1. Cloudflare → Zero Trust → Networks → Tunnels → Create Tunnel
2. Token gerado → adicionar ao `.env` como `CF_TUNNEL_TOKEN`
3. Public Hostname: `cgmg.seudominio.com` → `http://nginx:80`
4. `docker compose restart cloudflared`
5. `docker logs cgmg-cloudflared` → verificar "Connection registered"

### Passo 5 — Proteger com Cloudflare Access

Ver `docs/SEQ-SETUP.md` seção 3 para proteger o Seq.
Para a app principal, o login próprio é suficiente (não precisa de Access na raiz).

### Passo 6 — Verificar funcionamento

```bash
# Verificar todos os containers
docker compose ps

# Verificar logs da API
docker logs cgmg-api --tail 50

# Testar endpoint
curl -s https://cgmg.seudominio.com/api/auth/me
# Deve retornar 401 (não autenticado) — isso é correto

# Testar upload de extrato no browser
# https://cgmg.seudominio.com
```

---

## 5. Funcionalidades planejadas mas não implementadas (Fase C)

Estas funcionalidades estão **arquitetadas e documentadas** mas não codificadas. São melhorias para após validação do MVP.

### 5.1 Export assíncrono + MinIO + Notificações

**Motivação:** O export atual é síncrono. Para arquivos grandes (muitas transações), pode demorar e travar a UI.

**Arquitetura planejada:**
```
Usuário clica "Exportar"
  → API retorna 202 Accepted + { jobId }
  → RabbitMQ: ExportStatementMessage
  → Worker: gera arquivo → salva no MinIO (S3 local, TTL 48h)
  → Cria Notification no banco
  → Usuário vê sino 🔔 no navbar com link de download
```

**O que precisará:**
- MinIO no docker-compose (portas 9000 + 9001)
- `IFileStorageService` + implementação MinIO SDK
- Entidade `Notification` + repositório + endpoints
- Frontend: sino com badge de não lidas

### 5.2 Microserviço Go/Rust para OCR de imagens

O `ImageTextExtractor` lança `NotSupportedException`. Para extrair texto de imagens (prints de app, fotos de extrato), está planejado um microserviço separado com Tesseract.

### 5.3 Categorização inteligente via IA

Passar categorias do usuário no prompt do Worker para que o DeepSeek sugira a categoria de cada transação no momento da extração (antes das Regras de Ouro).

### 5.4 Conversão de moeda

Integração com API de câmbio (ex: AwesomeAPI) para visualizar gastos multi-moeda convertidos para BRL.

---

## 6. Arquitetura atual — diagrama de fluxo

```
Usuário
  │
  ▼
Cloudflare Edge (TLS)
  │
  ▼ (outbound via cloudflared)
Nginx (porta 80, interno)
  ├── /api/* → API :8080
  └── /*     → UI  :3000

API (upload) → Sanitiza PII → Salva FinancialStatement → RabbitMQ
                                                              │
                                                              ▼
                                                          Worker
                                                    ├── DeepSeek (extração)
                                                    ├── Regras de Ouro
                                                    └── DeepSeek (análise)
                                                              │
                                                              ▼
                                                         PostgreSQL
                                                              │
                                                         Redis Cache

Logs → Seq (http://seq:5341)
```

---

## 7. Checklist final antes do deploy

- [ ] `.env` criado com todas as variáveis obrigatórias
- [ ] Senhas fortes no `.env` (postgres, rabbitmq)
- [ ] `NUXT_PUBLIC_API_BASE` configurada no docker-compose
- [ ] Migrations automáticas em `Program.cs` (ou step manual documentado)
- [ ] Serilog + Seq configurados (opcional mas recomendado)
- [ ] Cloudflare Tunnel criado e token no `.env`
- [ ] `CF_TUNNEL_TOKEN` testado localmente antes de ir para VPS
- [ ] Domínio apontando para Cloudflare
- [ ] Backup do PostgreSQL configurado na VPS
- [ ] `docker compose up -d` executado e todos os containers `healthy`
- [ ] Acessar `https://cgmg.seudominio.com` e criar conta de teste
- [ ] Fazer upload de um extrato PDF e verificar processamento completo
