# Seq — Setup na VPS + Cloudflare Access

Seq é um agregador de logs estruturados. A API (.NET) envia logs via Serilog → Seq armazena e indexa → você visualiza no browser com filtros em tempo real.

```
API/Worker (Serilog)
       │
       ▼ HTTP :5341
     Seq (Docker)
       │
       ▼ HTTP :80
     Nginx /seq/
       │
       ▼
Cloudflare Tunnel ──► logs.seudominio.com
       │
Cloudflare Access ──► Só você passa
```

---

## 1. Subir o stack na VPS

```bash
# Na VPS, dentro da pasta do projeto
cp .env.example .env
# Preencha OPENROUTER_API_KEY e CF_TUNNEL_TOKEN no .env

docker compose up -d
docker logs cgmg-seq   # deve mostrar "Seq is running"
```

Verifique localmente (se tiver acesso SSH à VPS):
```bash
ssh -L 8088:localhost:80 usuario@ip-da-vps
# Abra http://localhost:8088/seq no seu PC
```

---

## 2. Expor via Cloudflare Tunnel

### 2.1 Criar o hostname público

1. Acesse **dash.cloudflare.com** → sua conta → **Zero Trust** → **Networks** → **Tunnels**
2. Clique no tunnel existente (`cgmg-prod`) → aba **Public Hostnames** → **Add a hostname**

| Campo | Valor |
|---|---|
| Subdomain | `logs` |
| Domain | `seudominio.com` |
| Type | `HTTP` |
| URL | `nginx:80` |
| Path | `/seq/` |

3. Clique em **Save hostname**

Após salvar, `https://logs.seudominio.com/seq/` fica acessível — mas ainda público. O passo seguinte protege com autenticação.

> **Alternativa mais simples:** crie um hostname separado apontando direto para `seq:80` (sem passar pelo nginx). Isso mantém o Seq totalmente isolado da URL pública da aplicação.
>
> | Campo | Valor |
> |---|---|
> | Subdomain | `logs` |
> | URL | `seq:80` |
>
> Com isso, acesse `https://logs.seudominio.com` diretamente (sem `/seq/`).

---

## 3. Proteger com Cloudflare Access (só você acessa)

### 3.1 Criar a Application no Zero Trust

1. **Zero Trust** → **Access** → **Applications** → **Add an application**
2. Tipo: **Self-hosted**
3. Preencha:
   - **Application name**: `Seq Logs`
   - **Application domain**: `logs.seudominio.com`
   - (Se usou path) **Path**: `/seq/`
4. Clique em **Next**

### 3.2 Criar a Policy

1. **Policy name**: `Apenas eu`
2. **Action**: Allow
3. Em **Configure rules** → **Include**:
   - **Selector**: Emails
   - **Value**: `seu@email.com`
4. **Save policy** → **Save application**

A partir desse momento, ao acessar `https://logs.seudominio.com/seq/`, a Cloudflare exibe uma tela de login com OTP por e-mail antes de deixar você entrar.

### 3.3 Opção mais segura: One-time PIN + IP específico

Para restringir ainda mais (ex: só do seu IP fixo em casa):

Em **Configure rules** → adicione uma segunda regra:
- **Selector**: IP ranges
- **Value**: `seu.ip.fixo/32`

Com as duas regras em **AND** (Require), só alguém com o seu e-mail **E** o seu IP passa.

---

## 4. Integrar Serilog no .NET (quando for implementar)

### 4.1 Instalar pacotes

```bash
# Na pasta do projeto (API e Worker)
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Seq
dotnet add package Serilog.Sinks.Console
```

### 4.2 Configurar em `Program.cs`

```csharp
using Serilog;

// Antes de builder.Build()
builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .Enrich.FromLogContext()
       .Enrich.WithMachineName()
       .WriteTo.Console()
       .WriteTo.Seq(ctx.Configuration["Seq:Url"] ?? "http://seq:5341"));
```

### 4.3 Adicionar ao `appsettings.json`

```json
{
  "Seq": {
    "Url": "http://seq:5341"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    }
  }
}
```

### 4.4 Adicionar variável de ambiente no `docker-compose.yml`

```yaml
api:
  environment:
    - Seq__Url=http://seq:5341
```

---

## 5. Navegar logs no Seq UI

| O que fazer | Como |
|---|---|
| Ver todos os logs | Aba **Events**, deixar filtro vazio |
| Filtrar por nível | `@Level = 'Error'` |
| Buscar por usuário | `UserId = 'guid-aqui'` |
| Buscar por extrato | `StatementId = 'guid-aqui'` |
| Ver só erros de hoje | `@Level = 'Error' and @Timestamp > Now() - 24h` |
| Salvar filtro | Clicar em **Signals** → **Add Signal** |

Os campos `StatementId` e `UserId` nos logs do `DeleteStatementCommandHandler` já são propriedades estruturadas — aparecem como campos filtráveis automaticamente quando Serilog estiver integrado.

---

## 6. Verificar que está funcionando

```bash
# Teste de ingestion manual
curl -X POST http://localhost:5341/api/events/raw \
  -H "Content-Type: application/json" \
  -d '{"Events":[{"Timestamp":"2026-01-01T00:00:00Z","Level":"Information","MessageTemplate":"Teste do Seq funcionando!"}]}'

# Deve retornar 201 Created
# Abrir https://logs.seudominio.com/seq/ → ver o evento aparecer
```
