# Guia de Configuração — ComoGastei

Este guia cobre os três ambientes: **máquina local**, **GitHub Actions** e **VPS de produção**.

---

## Senhas: onde gerar e onde guardar

**Guarde todas as senhas no Bitwarden** antes de qualquer coisa. Cada ambiente vai receber uma cópia via `.env`.

Para gerar senhas seguras:

```powershell
# PowerShell (Windows)
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(32))
```

```bash
# Terminal Linux/Mac (VPS)
openssl rand -base64 32
```

Gere **uma senha para o banco** e **outra para o RabbitMQ**. Salve ambas no Bitwarden agora.

---

## Ambiente local

### Opção A — Tudo via Docker (recomendado para uso normal)

```bash
# 1. Criar o .env local a partir do template
cp .env.example .env

# 2. Abrir o .env e preencher com as senhas do Bitwarden
#    (veja a seção "Variáveis de ambiente" abaixo)

# 3. Subir toda a stack
docker compose up -d
```

- API disponível em: `http://localhost:80` (via Nginx)
- RabbitMQ admin em: `http://localhost:15672`
- Frontend em: `http://localhost:80`

### Opção B — Infraestrutura no Docker + API via `dotnet run` (para debug com breakpoints)

```bash
# 1. Subir apenas a infraestrutura
docker compose up -d db redis rabbitmq

# 2. Criar o arquivo de override local (está no .gitignore, nunca vai ao git)
# Caminho: src/ComoGastoMinhaGrana.Api/appsettings.Development.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=comogasto_db;Username=admin;Password=SUA_SENHA_DO_BITWARDEN"
  },
  "OpenRouter": {
    "ApiKey": "sk-or-SUA_CHAVE_OPENROUTER"
  }
}
```

```bash
# 3. Rodar a API
cd src/ComoGastoMinhaGrana.Api && dotnet run

# 4. (Opcional) Rodar o Worker em outro terminal
cd src/ComoGastoMinhaGrana.Worker && dotnet run
```

**Nota:** O `appsettings.Development.json` está no `.gitignore` — pode colocar a senha real sem risco de commitar.

### Aplicar migrations (primeira vez ou após mudanças de schema)

```bash
cd ComoGastei
dotnet ef database update --project src/ComoGastoMinhaGrana.Infrastructure --startup-project src/ComoGastoMinhaGrana.Api
```

---

## Variáveis de ambiente (`.env`)

Copie `.env.example` para `.env` e preencha:

| Variável | Obrigatória | Descrição |
|---|---|---|
| `POSTGRES_USER` | Não (padrão: `admin`) | Usuário do PostgreSQL |
| `POSTGRES_PASSWORD` | **Sim** | Senha do PostgreSQL — gere e salve no Bitwarden |
| `POSTGRES_DB` | Não (padrão: `comogasto_db`) | Nome do banco |
| `RABBITMQ_USER` | Não (padrão: `cgmg_user`) | Usuário do RabbitMQ |
| `RABBITMQ_PASS` | **Sim** | Senha do RabbitMQ — gere e salve no Bitwarden |
| `OPENROUTER_API_KEY` | **Sim** | Chave da API em [openrouter.ai/keys](https://openrouter.ai/keys) |
| `CF_TUNNEL_TOKEN` | Só em produção | Token do Cloudflare Tunnel |

---

## GitHub Actions

O pipeline (`.github/workflows/ci-cd.yml`) precisa de 4 secrets. Configure em:
`GitHub → Repositório → Settings → Secrets and variables → Actions → New repository secret`

| Secret | Como obter |
|---|---|
| `DOCKERHUB_USERNAME` | Seu usuário em hub.docker.com |
| `DOCKERHUB_TOKEN` | hub.docker.com → Account Settings → Security → New Access Token |
| `SERVER_IP` | IP público da VPS |
| `SSH_PRIVATE_KEY` | Conteúdo de `~/.ssh/id_rsa` (chave que tem acesso à VPS) |

> As senhas do banco e RabbitMQ **não vão para o GitHub** — elas ficam no `.env` da VPS.

---

## VPS de produção

### Pré-requisitos

- Ubuntu 22.04+
- Docker e Docker Compose instalados
- Porta 80 aberta no firewall
- Chave SSH configurada (a mesma do secret `SSH_PRIVATE_KEY`)

### Primeiro deploy (configuração inicial)

```bash
# 1. Instalar Docker (se ainda não tiver)
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# 2. Criar pasta do projeto
mkdir -p ~/app/cgmg && cd ~/app/cgmg

# 3. Criar o .env com os valores reais do Bitwarden
nano .env
```

Conteúdo do `.env` na VPS:

```env
POSTGRES_USER=admin
POSTGRES_PASSWORD=<senha do Bitwarden>
POSTGRES_DB=comogasto_db
RABBITMQ_USER=cgmg_user
RABBITMQ_PASS=<senha do Bitwarden>
OPENROUTER_API_KEY=sk-or-<sua chave>
CF_TUNNEL_TOKEN=<token do Cloudflare>
```

```bash
# 4. Restringir permissão do .env (só o dono pode ler)
chmod 600 .env

# 5. O deploy automático via GitHub Actions vai rodar:
#    docker compose pull && docker compose up -d
#    A partir daí, cada push na branch main faz o deploy automaticamente.
```

---

## Fluxo completo de segredos

```
Bitwarden (fonte da verdade)
       │
       ├──▶ .env local (sua máquina)     — gitignored, lido pelo docker compose
       │        POSTGRES_PASSWORD
       │        RABBITMQ_PASS
       │        OPENROUTER_API_KEY
       │
       ├──▶ GitHub Secrets               — só para CI/CD, não chegam à app
       │        DOCKERHUB_USERNAME
       │        DOCKERHUB_TOKEN
       │        SERVER_IP
       │        SSH_PRIVATE_KEY
       │
       └──▶ .env na VPS                  — gitignored, lido pelo docker compose
                POSTGRES_PASSWORD
                RABBITMQ_PASS
                OPENROUTER_API_KEY
                CF_TUNNEL_TOKEN
```

Nenhum `.env` vai para o repositório — o `.gitignore` já garante isso.
