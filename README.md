# Secure Orders Backend

API desenvolvida em ASP.NET Core seguindo os princípios de Clean Architecture e Domain-Driven Design (DDD), com foco em segurança, organização de código, 
escalabilidade e boas práticas utilizadas em ambientes corporativos.

Este projeto foi criado com o objetivo de servir como portfólio técnico, demonstrando domínio em backend moderno com .NET e arquitetura limpa.

---

## Arquitetura

O projeto segue a Clean Architecture, separando claramente as responsabilidades em camadas:

src/
 ├── SecureOrders.Api           → Camada de apresentação (Controllers)
 ├── SecureOrders.Application   → Regras de aplicação, Services, Validators
 ├── SecureOrders.Domain        → Entidades e regras de domínio
 └── SecureOrders.Infrastructure→ Persistência, Auth, Redis, Logs

---

## Segurança

- Autenticação baseada em JWT
- Proteção de endpoints com autorização
- Tokens com expiração configurável

---

## Tecnologias utilizadas

- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- Redis
- JWT Authentication
- FluentValidation
- AutoMapper
- Serilog
- Swagger / OpenAPI
- Health Checks

---

## Documentação da API

A API possui documentação automática com Swagger.

Após rodar o projeto:
https://localhost:PORTA/swagger

---

## Como executar o projeto

Pré-requisitos:
- .NET SDK
- Banco de dados configurado
- Redis

Comandos básicos:

dotnet restore  
dotnet run --project src/SecureOrders.Api

---

## Roadmap

- [x] Clean Architecture
- [x] Autenticação JWT
- [x] Redis
- [ ] Testes unitários
- [ ] Docker
- [ ] Deploy em Cloud

---

## Autor

Elton Araujo  
Backend Developer | .NET

Projeto criado para fins de estudo e portfólio.
