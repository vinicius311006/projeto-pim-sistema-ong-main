# ONG Vestindo Sonhos - Sistema Integrado de Gestão & Aplicativo Móvel

Projeto Integrador Multidisciplinar (PIM) - Análise e Desenvolvimento de Sistemas.

Solução corporativa completa para a gestão de doações, inventário, voluntariado e eventos da ONG **Vestindo Sonhos**, integrando plataforma web administrativa, API RESTful segura, aplicativo móvel para voluntários, automações em banco de dados e pipeline moderno de DevOps.

---

## 🏛️ Arquitetura da Solução

```
                    ┌────────────────────────┐
                    │    MySQL Server 8.0    │
                    │  (Triggers, SPs, Views) │
                    └───────────┬────────────┘
                                │
                    ┌───────────┴────────────┐
                    │ ASP.NET Core (.NET 10) │
                    │ Web MVC + RESTful API  │
                    └───────┬────────────┬───┘
                            │            │
            (Cookie Auth)   │            │   (JWT Bearer Token)
                            ▼            ▼
               ┌────────────────┐   ┌────────────────────────┐
               │ Portal Web MVC │   │   App Mobile Flutter   │
               │ Administrativo │   │ (Voluntários de Campo) │
               └────────────────┘   └────────────────────────┘
```

---

## 🚀 Módulos e Funcionalidades

### 1. Back-end & API RESTful (ASP.NET Core .NET 10)
- **Autenticação Híbrida Inteligente:** Suporte simultâneo a Cookies (para o portal administrativo MVC) e tokens JWT Bearer (`/api`) para o aplicativo mobile e integrações externas.
- **Criptografia e Hashing Seguro:** Migração automática e transparente de senhas legadas para PBKDF2 (`PasswordHasher<Usuario>`).
- **Endpoints REST Documentados:**
  - `POST /api/auth/login`: Autenticação e emissão de token JWT assinado com claims de usuário e cargo.
  - `GET /api/eventos`: Consulta em tempo real de campanhas e eventos ativos com contagem de voluntários (protegido por `[Authorize]`).
- **Resiliência e Compatibilidade:** Provedor oficial `MySql.EntityFrameworkCore 10.0.9` com tratamento de contingência de gráficos e validações defensivas de dados.

### 2. Aplicativo Móvel (Flutter / Dart)
- **Público-alvo:** Voluntários de campo e coordenadores de eventos.
- **Design:** Interface moderna baseada em **Material Design 3**, paleta sóbria e humanitária (*Teal & Blue*).
- **Recursos:**
  - Tela de autenticação com validação local e feedback visual.
  - Armazenamento seguro de sessão e token JWT com `shared_preferences`.
  - Painel do Voluntário com resumo de perfil, status e ações rápidas.
  - Listagem de eventos em tempo real com indicador de status, datas formatadas, local e voluntários inscritos.
  - Suporte adaptativo de conectividade (reconhece emulador Android `10.0.2.2`, redes locais e desktop `localhost`).

### 3. Banco de Dados & Recursos Avançados
- **Scripts Disponíveis:**
  - `ScriptVestindoSonhos_MySQL.sql`: Totalmente preparado para MySQL 8.0 (MySQL Workbench).
  - `ScriptVestindoSonhos.sql`: Versão original em T-SQL (Microsoft SQL Server).
- **Stored Procedure (`sp_RelatorioFinanceiroResumido`):**
  - Consolida receitas, despesas, saldo líquido e volume de transações por mês/ano com alta performance analítica.
- **Trigger (`trg_AtualizaEstoque`):**
  - Automação transacional que atualiza o saldo da tabela `Inventario` em tempo real sempre que ocorre uma entrada ou saída em `TransacaoInventario`.
- **Views Otimizadas:**
  - `Financeiro`: Consolidação das movimentações contábeis.
  - Views de compatibilidade para entidades relacionais.

### 4. DevOps & Integração Contínua
- **Dockerfile Multi-Stage:**
  - Imagem de runtime ultra-leve baseada em `mcr.microsoft.com/dotnet/aspnet:10.0` para contêineres Linux de alta segurança.
- **GitHub Actions CI/CD (`.github/workflows/deploy.yml`):**
  - Pipeline automatizado disparado em `push` e `pull_request` na branch `main`:
    1. Checkout do código-fonte.
    2. Instalação e cache do SDK .NET 10.
    3. Restauração e compilação do projeto em modo `Release`.
    4. Validação da construção da imagem Docker.
    5. Upload dos artefatos publicados prontos para deploy.

---

## 💻 Instruções para Execução

### Pré-requisitos
- [.NET SDK 10.0](https://dotnet.microsoft.com/)
- [MySQL Server 8.0](https://dev.mysql.com/downloads/installer/) & MySQL Workbench
- [Flutter SDK](https://docs.flutter.dev/) (para execução mobile)
- [Android Studio](https://developer.android.com/studio) com emulador configurado

### 1. Configurar o Banco de Dados
1. Abra o **MySQL Workbench** e conecte-se à sua instância local (`localhost:3306`).
2. Abra e execute o arquivo [`ScriptVestindoSonhos_MySQL.sql`](./ScriptVestindoSonhos_MySQL.sql).
3. Todas as tabelas, triggers, procedures e dados iniciais serão criados automaticamente no banco `VESTINDOSONHOSDB`.

### 2. Executar o Back-end (Web MVC + API)
No terminal, dentro da pasta do projeto:
```bash
cd VestindoSonhos
dotnet run
```
O servidor iniciará ouvindo em:
- **Web & API:** [http://localhost:5000](http://localhost:5000) e [http://localhost:5192](http://localhost:5192)

**Credenciais Administrativas Padrão:**
- **E-mail:** `admin@teste.org`
- **Senha:** `123456`

### 3. Executar o Aplicativo Mobile
No terminal ou no Android Studio:
```bash
cd vestindo_sonhos_mobile
flutter run
```
Selecione o emulador Android (ex: `Medium Phone API 37.0`) ou dispositivo físico.

---

## 📁 Estrutura de Diretórios

```
projeto-pim-sistema-ong/
├── .github/workflows/
│   └── deploy.yml                     # Pipeline de CI/CD do GitHub Actions
├── VestindoSonhos/                    # Back-end ASP.NET Core .NET 10
│   ├── Controllers/
│   │   ├── Api/                       # Controladores RESTful (AuthApiController, EventosApiController)
│   │   └── ...                        # Controladores Web MVC
│   ├── Models/
│   │   ├── DTOs/                      # Objetos de Transferência de Dados (Login, Eventos)
│   │   └── ...                        # Entidades do EF Core
│   ├── Views/                         # Telas Razor MVC (Login, Home, Financeiro, etc.)
│   ├── Dockerfile                     # Multi-stage Dockerfile
│   └── appsettings.json               # Configurações de conexão e chaves JWT
├── vestindo_sonhos_mobile/            # Aplicativo Móvel em Flutter
│   ├── lib/
│   │   ├── models/                    # Modelos de dados Dart
│   │   ├── services/                  # Serviço de comunicação HTTP com a API REST
│   │   ├── screens/                   # Telas (Login, Home, Eventos)
│   │   └── main.dart                  # Ponto de entrada do app
│   └── pubspec.yaml                   # Dependências do Flutter
├── ScriptVestindoSonhos_MySQL.sql      # Script completo para MySQL (DDL + SP + Trigger)
├── ScriptVestindoSonhos.sql            # Script para SQL Server
└── README.md                          # Documentação geral da solução
```

---

## 👥 Autoria & Licença
Projeto desenvolvido com fins acadêmicos para a atividade do Projeto Integrador Multidisciplinar (PIM) - Curso de Análise e Desenvolvimento de Sistemas (2026).
