# Guia de CI/CD — Bitbucket Pipelines

Este guia explica como configurar a automação de deploy na sua VPS utilizando Bitbucket Pipelines, Docker Hub e Cloudflare Tunnel.

### 1. Variáveis de Repositório
No Bitbucket, vá em **Repository settings > Pipelines > Repository variables** e adicione as seguintes variáveis:
- `DOCKERHUB_USERNAME`: Seu nome de usuário do Docker Hub.
- `DOCKERHUB_TOKEN`: Seu Personal Access Token gerado no Docker Hub (não use sua senha real).
- `SERVER_IP`: O endereço IP público da sua VPS.

### 2. Configuração de Chaves SSH
O Bitbucket precisa de acesso SSH à sua VPS para executar os comandos de deploy.
1. Vá em **Repository settings > Pipelines > SSH keys**.
2. Clique em **Generate keys**.
3. Copie a **Public key** exibida.
4. Na sua VPS, cole essa chave dentro do arquivo `~/.ssh/authorized_keys`.
5. De volta ao Bitbucket, na seção **Known hosts**, digite o IP da sua VPS e clique em **Fetch**. Isso adiciona a impressão digital (fingerprint) da VPS aos hosts confiáveis do Bitbucket.

### 3. Como funciona o Pipeline
O arquivo `bitbucket-pipelines.yml` na raiz do projeto define três etapas automáticas que rodam a cada `push` na branch `main`:

1.  **Build and Test**: Utiliza o SDK do .NET 9 para compilar a solution e rodar todos os testes unitários. Se houver falha nos testes, o pipeline para aqui.
2.  **Build and Push**: 
    - Faz o login no Docker Hub.
    - Constrói as imagens Docker para os 3 serviços: `api`, `worker` e `ui`.
    - Faz o upload (push) dessas imagens para o seu repositório no Docker Hub com a tag `:latest`.
3.  **Deploy to VPS**:
    - Conecta-se à VPS via SSH usando o pipe `ssh-run`.
    - Entra na pasta da aplicação (`~/app/cgmg`).
    - Executa `docker compose pull` para baixar as imagens mais recentes.
    - Executa `docker compose up -d` para reiniciar os containers com as novas versões.
    - Limpa imagens antigas não utilizadas (`docker image prune`) para economizar espaço em disco.

### 4. Dicas de Manutenção na VPS
- Verifique os logs se algo der errado: `docker compose logs -f`.
- O status dos containers: `docker compose ps`.
- Certifique-se de que o arquivo `.env` na VPS está atualizado com as chaves de API e tokens necessários.
