# Como Gasto Minha Grana — Resumo do Projeto

Mini SaaS de análise financeira privada. O usuário faz upload de extratos bancários (PDF, TXT, Excel) → Worker extrai transações via IA → DeepSeek (OpenRouter) gera análise financeira estruturada em 5 seções.

---

## Arquitetura

Clean Architecture com 5 projetos .NET 9 + frontend Nuxt 3.

| Projeto | Responsabilidade |
|---|---|
| `Domain` | Entidades: `User`, `FinancialStatement`, `Transaction`, `FinancialAnalysis`, `Category` |
| `Application` | MediatR CQRS, FluentValidation, interfaces |
| `Infrastructure` | EF Core + PostgreSQL, DeepSeek (OpenRouter), Redis, MassTransit, extratores multi-formato, sanitizador de PII |
| `API` | ASP.NET Core — `AuthController`, `StatementsController` |
| `Worker` | Consumidor RabbitMQ — processa extrato, gera análise, salva resultado |
| `UI` | Nuxt 3 + DaisyUI + Pinia — login, dashboard, detalhe com análise |

### Fluxo de processamento

```
Upload PDF → API extrai texto localmente (PdfPig/ClosedXML)
           → Sanitiza PII (CPF, CNPJ, cartão, e-mail removidos)
           → Salva FinancialStatement (status: Pending)
           → Publica ProcessStatementMessage no RabbitMQ

Worker     → DeepSeek extrai transações estruturadas
           → Constrói resumo formatado
           → DeepSeek gera análise em markdown (5 seções)
           → Salva FinancialAnalysis (status: Completed)

GET análise → Redis cache (TTL 7 dias) → fallback PostgreSQL
```

### Fluxo de rede (produção com Cloudflare)

```
Internet
   │
   ▼
Cloudflare Edge  ← TLS terminado aqui (HTTPS gratuito)
   │
   ▼ (conexão de saída — sem portas abertas no servidor)
cloudflared (Docker)
   │
   ▼
nginx (porta 80, interno)
   ├── /api/* → API :8080
   └── /*     → UI  :3000
```

---

## Estado atual do desenvolvimento

### Implementado

- [x] Modelagem de domínio e migrations (EF Core + PostgreSQL, schema `cgmg`)
- [x] Extração de texto: PDF (PdfPig), TXT, Excel (ClosedXML) — imagem é stub
- [x] Sanitizador de PII (CPF, CNPJ, número de cartão, e-mail)
- [x] Integração DeepSeek via OpenRouter (extração de transações + análise financeira)
- [x] Fila assíncrona com RabbitMQ + MassTransit (dev: NoOpMessagePublisher)
- [x] Cache de análises no Redis (TTL 7 dias)
- [x] Autenticação com ASP.NET Identity + cookies (`CGMG.Auth`)
- [x] Frontend completo: login/registro, dashboard, detalhe de extrato com análise
- [x] Suporte multi-moeda e multi-idioma no domínio
- [x] Docker Compose completo (db, redis, rabbitmq, api, worker, ui, nginx)
- [x] Cloudflare Tunnel configurado no docker-compose.yml

### Pendente

#### A. Funcionalidades Core

- [ ] **Gestão de Categorias** — `Category` entity e `ICategoryRepository` existem, mas sem endpoints. Falta: controller + CRUD commands/queries + `CategoryRepository` implementation.
- [ ] **Categorização de Transações** — Nenhum endpoint para atribuir `CategoryId` a uma transação (PATCH).
- [ ] **Regras de Ouro** — Ex: toda transação contendo "Amazon" → categoria "Compras". Falta entidade `CategoryRule` + serviço de aplicação de regras no Worker.
- [ ] **Edição de Transações** — Nenhum endpoint PATCH para corrigir valor ou descrição que a IA errou.
- [ ] **Exclusão de Extratos** — Nenhum endpoint DELETE (cascata via FK já funcionaria, falta o endpoint).

#### B. Inteligência

- [ ] **Sugestão de categoria via IA** — Integrar no fluxo do Worker para sugerir categoria no momento da extração.
- [ ] **Conversão de moeda** — Integração com API de câmbio para ver tudo em BRL (ex: AwesomeAPI, ECB).

#### C. Infraestrutura

- [ ] **OCR de imagens** — `ImageTextExtractor` é stub. Planejado como microserviço Go/Rust com Tesseract.
- [ ] **Logs e monitoramento** — Rastrear erros de extração para melhorar o prompt continuamente.

---

## Infraestrutura Docker

### Variáveis de ambiente

Copie `.env.example` para `.env` e preencha antes de `docker compose up`:

```
OPENROUTER_API_KEY=sk-or-...   # openrouter.ai/keys
CF_TUNNEL_TOKEN=               # Cloudflare Dashboard → Zero Trust → Tunnels
```

### Comandos principais

```bash
# Subir tudo
docker compose up -d

# Subir só infraestrutura (sem app)
docker compose up -d db redis rabbitmq

# Ver logs do tunnel
docker logs cgmg-cloudflared

# Aplicar migrations
cd "Como gasto minha grana"
dotnet ef database update --project src/ComoGastoMinhaGrana.Infrastructure --startup-project src/ComoGastoMinhaGrana.Api

# Dev local (sem Docker)
cd src/ComoGastoMinhaGrana.Api && dotnet run        # http://localhost:5209
cd src/ComoGastoMinhaGrana.Worker && dotnet run
cd ComoGastoMinhaGrana.UI && npm run dev            # http://localhost:3000
```

---

## Cloudflare Tunnel — Setup passo a passo

O tunnel expõe o app na internet **sem abrir portas no roteador/firewall**. O cloudflared estabelece uma conexão de saída para a Cloudflare — ela roteia o tráfego de volta para o nginx interno.

### 1. Criar o tunnel

1. Acesse [dash.cloudflare.com](https://dash.cloudflare.com) → selecione sua conta
2. Menu: **Zero Trust** → **Networks** → **Tunnels**
3. Clique em **Create a tunnel** → tipo: **Cloudflared**
4. Dê um nome (ex: `cgmg-prod`) → **Save tunnel**
5. Copie o token exibido → salve em `.env` como `CF_TUNNEL_TOKEN`

### 2. Configurar o Public Hostname

Ainda na tela do tunnel, aba **Public Hostname**:

| Campo | Valor |
|---|---|
| Subdomain | `cgmg` (ou o que preferir) |
| Domain | seu domínio no Cloudflare |
| Type | `HTTP` |
| URL | `nginx:80` |

Clique em **Save hostname**. O domínio `cgmg.seudominio.com` passará a apontar para o app.

### 3. Subir o tunnel

```bash
docker compose up -d cloudflared
docker logs cgmg-cloudflared
# Deve mostrar: "Connection registered" para 4 datacenters da Cloudflare
```

### 4. TLS automático

A Cloudflare emite e renova o certificado HTTPS automaticamente — nenhum Let's Encrypt ou cert local necessário. O nginx só precisa ouvir na porta 80 (interno).

---

## Cloudflare Access (Zero Trust) — Proteção de acesso

O **Cloudflare Access** adiciona uma camada de autenticação *na frente* do tunnel, antes de chegar na aplicação. Útil para bloquear acesso público em staging ou proteger o painel de admin.

> Este app já tem autenticação própria (ASP.NET Identity). O Access seria uma segunda camada — opcional, mas recomendada para staging/dev exposto.

### Configurar uma policy

1. **Zero Trust** → **Access** → **Applications** → **Add an application**
2. Tipo: **Self-hosted**
3. **Application name**: `Como Gasto Minha Grana`
4. **Application domain**: `cgmg.seudominio.com`
5. Clique em **Next** → **Add a policy**:
   - **Policy name**: `Apenas eu`
   - **Action**: Allow
   - **Include**: Emails → `seu@email.com`
6. **Save policy** → **Save application**

### Resultado

Ao acessar `cgmg.seudominio.com`, a Cloudflare exibe uma tela de autenticação (email OTP por padrão). Só após autenticar o usuário é redirecionado para o app. Suporte a GitHub, Google, Azure AD, e outros provedores via configuração adicional de Identity Provider.

### Validar o JWT da Cloudflare na API (opcional)

A Cloudflare injeta um header `Cf-Access-Jwt-Assertion` em todas as requisições autenticadas. Para validar no ASP.NET Core:

```csharp
// O token é assinado com a chave pública disponível em:
// https://<your-team>.cloudflareaccess.com/cdn-cgi/access/certs
// Isso é opcional — a validação já ocorre na borda da Cloudflare.
```
