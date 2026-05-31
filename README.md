# ComoGastei

Mini SaaS de análise financeira pessoal e privada. Faça upload de extratos bancários e receba uma análise detalhada gerada por IA — sem expor seus dados financeiros a serviços externos.

## Como funciona

```
Upload de extrato (PDF, TXT, Excel)
   │
   ▼
API extrai o texto localmente (PdfPig / ClosedXML)
   │
   ▼
Sanitizador remove CPF, CNPJ, cartão e e-mail
   │
   ▼
Worker envia para DeepSeek → extrai transações + gera análise em 5 seções
   │
   ▼
Resultado salvo no PostgreSQL e cacheado no Redis (7 dias)
```

Nenhum dado bruto chega à IA — o sanitizador remove informações pessoais identificáveis antes de qualquer chamada externa.

## Stack

**Backend**
- .NET 9 — ASP.NET Core Web API + Worker Service
- Clean Architecture (Domain / Application / Infrastructure / API / Worker)
- Entity Framework Core + PostgreSQL
- MediatR (CQRS) + FluentValidation
- MassTransit + RabbitMQ
- Redis (cache de análises)
- ASP.NET Core Identity (autenticação por cookie)

**Frontend**
- Nuxt 3 + TypeScript
- DaisyUI + Tailwind CSS
- Pinia

**IA**
- DeepSeek V3 via [OpenRouter](https://openrouter.ai)

**Infraestrutura**
- Docker Compose (PostgreSQL, Redis, RabbitMQ, Nginx, Seq, Cloudflare Tunnel)
- GitHub Actions → Docker Hub → VPS

## Rodando localmente

**Pré-requisitos:** Docker instalado.

```bash
# 1. Clone o repositório
git clone https://github.com/RichardFurlan/FinancialComoGastei.git
cd FinancialComoGastei

# 2. Configure as variáveis de ambiente
cp .env.example .env
# Edite .env e preencha POSTGRES_PASSWORD, RABBITMQ_PASS e OPENROUTER_API_KEY

# 3. Suba a stack
docker compose up -d
```

A aplicação fica disponível em `http://localhost`.

Para desenvolvimento com `dotnet run` (debug com breakpoints), crie `src/ComoGastoMinhaGrana.Api/appsettings.Development.json` com a connection string local — esse arquivo está no `.gitignore`.

## Variáveis de ambiente

| Variável | Obrigatória | Descrição |
|---|---|---|
| `POSTGRES_PASSWORD` | Sim | Senha do PostgreSQL |
| `RABBITMQ_PASS` | Sim | Senha do RabbitMQ |
| `OPENROUTER_API_KEY` | Sim | Chave da API — [openrouter.ai/keys](https://openrouter.ai/keys) |
| `CF_TUNNEL_TOKEN` | Produção | Token do Cloudflare Tunnel |

## Licença

Uso pessoal.
