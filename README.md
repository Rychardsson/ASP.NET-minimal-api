# 🚗 Minimal API - Sistema de Veículos

Uma API moderna construída com **ASP.NET Core Minimal APIs** para gerenciamento de veículos e administradores, implementando autenticação JWT e documentação Swagger.

## 🎯 Funcionalidades

### ✅ **Recursos Implementados**

- 🔐 **Autenticação JWT** com controle de acesso baseado em roles
- 🚗 **CRUD completo de Veículos** (Create, Read, Update, Delete)
- 👥 **Gerenciamento de Administradores** com diferentes perfis
- 📚 **Documentação automática** com Swagger/OpenAPI
- 🔍 **Paginação** de resultados
- ✅ **Validações** automáticas de dados
- 🧪 **Testes de integração** abrangentes
- 📊 **Endpoints de estatísticas** e health check

### 🏗️ **Arquitetura**

- **Minimal APIs** - Abordagem moderna e performática
- **Entity Framework Core** - ORM para acesso a dados
- **MySQL** - Banco de dados relacional
- **JWT Bearer** - Autenticação e autorização
- **Swagger UI** - Interface para documentação e testes

## 🚀 Como Executar

### Pré-requisitos

- .NET 7.0 SDK
- MySQL Server
- Visual Studio 2022 ou VS Code

### 1. Clone o repositório

```bash
git clone <url-do-repositorio>
cd ASP.NET-minimal-api
```

### 2. Configure o banco de dados

Atualize a string de conexão no `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "MySql": "Server=localhost;Database=minimal_api;Uid=root;Pwd=suasenha;"
  }
}
```

### 3. Execute as migrations

```bash
cd Api
dotnet ef database update
```

### 4. Execute a aplicação

```bash
dotnet run
```

### 5. Acesse a documentação

Abra o navegador em: `https://localhost:7xxx` (o Swagger UI será carregado automaticamente)

## 📋 Endpoints da API

### 🏠 **Home & Utilitários**

- `GET /` - Informações básicas da API
- `GET /api/health` - Health check da aplicação
- `GET /api/estatisticas` - Estatísticas gerais (requer autenticação)

### 👥 **Administradores**

- `POST /administradores/login` - Login e obtenção do token JWT
- `GET /administradores` - Listar administradores (apenas Adm)
- `GET /administradores/{id}` - Buscar administrador por ID (apenas Adm)
- `POST /administradores` - Criar novo administrador (apenas Adm)

### 🚗 **Veículos**

- `GET /veiculos` - Listar veículos (requer autenticação)
- `GET /veiculos/{id}` - Buscar veículo por ID (Adm/Editor)
- `POST /veiculos` - Criar novo veículo (Adm/Editor)
- `PUT /veiculos/{id}` - Atualizar veículo (apenas Adm)
- `DELETE /veiculos/{id}` - Deletar veículo (apenas Adm)

## 🔐 Autenticação

### Perfis de Usuário

- **Adm (Administrador)**: Acesso total a todas as funcionalidades
- **Editor**: Pode visualizar, criar e editar veículos (sem deletar)

### Como usar JWT

1. Faça login através do endpoint `/administradores/login`
2. Copie o token retornado
3. No Swagger UI, clique em "Authorize" e insira: `Bearer {seu-token}`
4. Agora você pode acessar os endpoints protegidos

### Exemplo de Login

```json
POST /administradores/login
{
  "email": "admin@teste.com",
  "senha": "123456"
}
```

**Resposta:**

```json
{
  "email": "admin@teste.com",
  "perfil": "Adm",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

## 🧪 Executando Testes

```bash
cd Test
dotnet test
```

Os testes incluem:

- ✅ Verificação de endpoints públicos
- ✅ Validação de autenticação
- ✅ Testes de integração
- ✅ Verificação do Swagger

## 🏗️ Estrutura do Projeto

```
Api/
├── Program.cs                 # Configuração principal da aplicação
├── Extensions/
│   └── EndpointsExtensions.cs # Extensões para organização dos endpoints
├── Dominio/
│   ├── Entidades/            # Classes de entidade (Veiculo, Administrador)
│   ├── DTOs/                 # Data Transfer Objects
│   ├── ModelViews/           # View Models para responses
│   ├── Interfaces/           # Contratos de serviços
│   ├── Servicos/            # Implementação da lógica de negócio
│   └── Enuns/               # Enumerações (Perfil)
├── Infraestrutura/
│   └── Db/                  # Contexto do Entity Framework
└── Migrations/              # Migrações do banco de dados

Test/
├── Requests/                # Testes de integração dos endpoints
├── Domain/                  # Testes de domínio
└── Helpers/                 # Utilitários para testes
```

## 🔧 Tecnologias Utilizadas

- **ASP.NET Core 7.0** - Framework web
- **Minimal APIs** - Abordagem simplificada para APIs
- **Entity Framework Core** - ORM
- **MySQL** - Banco de dados
- **JWT Bearer** - Autenticação
- **Swagger/OpenAPI** - Documentação
- **MSTest** - Framework de testes

## 📝 Exemplos de Uso

### Criando um Veículo

```json
POST /veiculos
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "Civic",
  "marca": "Honda",
  "ano": 2023
}
```

### Listando Veículos com Paginação

```http
GET /veiculos?pagina=1
Authorization: Bearer {token}
```

### Atualizando um Veículo

```json
PUT /veiculos/1
Authorization: Bearer {token}
Content-Type: application/json

{
  "nome": "Civic Type R",
  "marca": "Honda",
  "ano": 2023
}
```

## 🤝 Contribuindo

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

## 🎓 Aprendizado

Este projeto demonstra:

- **Minimal APIs** - Sintaxe moderna e performática
- **Injeção de Dependência** - Padrão fundamental do .NET
- **Entity Framework** - Mapeamento objeto-relacional
- **JWT Authentication** - Autenticação stateless
- **Swagger Integration** - Documentação automática
- **Testing Best Practices** - Testes de integração robustos
- **Clean Architecture** - Separação de responsabilidades

---

⭐ **Dica**: Explore todos os endpoints através da interface Swagger para uma experiência completa da API!
