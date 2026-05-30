# Como gasto minha grana 📊

Mini SaaS de Análise Financeira privada e de baixo custo.

## 🚀 Estrutura do Projeto (Clean Architecture)

- **src/ComoGastoMinhaGrana.Domain**: Entidades, interfaces e lógica de negócio central.
- **src/ComoGastoMinhaGrana.Application**: Casos de uso, DTOs e orquestração (CQRS).
- **src/ComoGastoMinhaGrana.Infrastructure**: Implementação de acesso a dados (PostgreSQL) e serviços externos (Ollama/DeepSeek).
- **src/ComoGastoMinhaGrana.Api**: Endpoints da API e configuração da aplicação.

## 🛠️ Tecnologias

- **Backend**: .NET 9.0 (ASP.NET Core API)
- **Banco de Dados**: PostgreSQL 17
- **IA Local**: Ollama (Qwen 2.5 3B)
- **IA Nuvem**: DeepSeek V3/R1 (via API)

## 🐳 Como Executar

Certifique-se de ter o Docker instalado e execute:

```bash
docker-compose up -d
```

A API estará disponível em `http://localhost:8080`.
O Swagger/OpenAPI pode ser acessado em `http://localhost:8080/swagger` (ou via Scalar dependendo da configuração).

## 📄 Fluxo de Dados

1. **Upload**: PDF de extrato é enviado para a API.
2. **Extração**: Texto bruto é extraído localmente (PdfPig).
3. **Triagem**: IA Local (Ollama/Qwen) estrutura os dados em JSON.
4. **Processamento**: .NET valida cálculos e salva no PostgreSQL.
5. **Análise**: Dados anonimizados são enviados para IA em nuvem (DeepSeek) para insights profundos.
