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
- **Script do Banco de Dados:**
  - `ScriptVestindoSonhos_MySQL.sql`: Script oficial e completo preparado para MySQL 8.0 (MySQL Workbench), incluindo tabelas, Stored Procedure, Trigger e dados de teste iniciais.
- **Stored Procedure (`sp_RelatorioFinanceiroResumido`):**
  - Consolida receitas, despesas, saldo líquido e volume de transações por mês/ano com alta performance analítica.
- **Trigger (`trg_AtualizaEstoque`):**
  - Automação transacional que atualiza o saldo da tabela `Inventario` em tempo real sempre que ocorre uma entrada ou saída em `TransacaoInventario`.
- **Views Otimizadas:**
  - `Financeiro`: Consolidação das movimentações contábeis.
  - Views de compatibilidade para entidades relacionais.

### 4. Integração Contínua (CI/CD)
- **GitHub Actions CI/CD (`.github/workflows/deploy.yml`):**
  - Pipeline automatizado disparado em `push` e `pull_request` na branch `main`:
    1. Checkout do código-fonte.
    2. Instalação e cache do SDK .NET 10.
    3. Restauração e compilação do projeto em modo `Release`.
    4. Upload dos binários compilados e prontos para execução.

---

## 💻 Formas de Execução no Ambiente Acadêmico

O sistema foi preparado para rodar de forma simples e 100% local:

---

### Opção 1: Executável Portátil Standalone (Recomendado para Apresentação no PIM)
Para rodar em qualquer computador Windows **SEM precisar instalar o .NET SDK, VS Code ou Android Studio**:
1. Execute o script [`gerar-executavel-portatil.bat`](./gerar-executavel-portatil.bat) (ou use a pasta já gerada `VestindoSonhos_Portavel`).
2. Abra a pasta `VestindoSonhos_Portavel` e dê **dois cliques em `1-CLIQUE_INICIAR_SISTEMA.bat`** (ou em `VestindoSonhos.exe`).
3. O sistema abrirá automaticamente no navegador em: [http://localhost:5000/Login](http://localhost:5000/Login)
*(O runtime do .NET 10 já vai totalmente embutido no executável!)*

---

### Opção 2: Execução em Ambiente de Desenvolvimento (.NET SDK 10)
Caso a máquina de desenvolvimento possua o [.NET SDK 10.0](https://dotnet.microsoft.com/) e o [MySQL Workbench](https://dev.mysql.com/downloads/workbench/):
1. **Configurar o Banco de Dados:**
   - Abra o MySQL Workbench e execute o script [`ScriptVestindoSonhos_MySQL.sql`](./ScriptVestindoSonhos_MySQL.sql) (ou dê dois cliques em [`importar-banco-local.bat`](./importar-banco-local.bat)).
2. **Rodar a Aplicação Web:**
   ```bash
   cd VestindoSonhos
   dotnet run
   ```
   Acesse [http://localhost:5000/Login](http://localhost:5000/Login).

3. **Credenciais Administrativas Padrão:**
   - **E-mail:** `admin@teste.org`
   - **Senha:** `123456`

---

### 📱 Executar o Aplicativo Mobile (Flutter)
No terminal ou no Android Studio:
```bash
cd vestindo_sonhos_mobile
flutter run
```
Selecione o emulador Android ou dispositivo físico. O app se conecta automaticamente à API REST na porta 5000.

---

## 📁 Estrutura de Diretórios

```
projeto-pim-sistema-ong/
├── VestindoSonhos/                    # Back-end ASP.NET Core .NET 10 (MVC + APIs REST)
│   ├── Controllers/
│   │   ├── Api/                       # APIs RESTful (Dashboard, Beneficiários, Inventário, Financeiro, etc.)
│   │   └── ...                        # Controladores Web MVC
│   ├── Models/
│   ├── Views/                         # Telas Razor MVC (Design institucional)
│   └── wwwroot/mobile/                # Aplicativo mobile embutido na Web
├── vestindo_sonhos_mobile/            # Aplicativo Móvel em Flutter (Android & iOS)
│   ├── lib/
│   │   ├── models/                    # Modelos de dados
│   │   ├── screens/                   # Telas (Login, Home, Beneficiários, Inventário, Financeiro, Eventos)
│   │   ├── services/                  # Comunicação com a API REST
│   │   └── widgets/                   # Componentes (Configurador de IP dinâmico)
│   └── pubspec.yaml                   # Dependências do Flutter
├── VestindoSonhos_Portavel/           # Pacote independente portátil para rodar em qualquer PC
│   ├── VestindoSonhos.exe             # Executável sem necessidade de .NET SDK
│   ├── VestindoSonhos_Mobile.apk      # Aplicativo Android pronto para instalar
│   └── 1-CLIQUE_INICIAR_SISTEMA.bat   # Inicializador automático
├── ScriptVestindoSonhos_MySQL.sql      # Script oficial completo do MySQL (Tabelas + Procedure + Trigger + Carga)
├── importar-banco-local.bat           # Importador automático do banco de dados
├── subir-para-github.bat              # Sincronizador automático com o GitHub em 1 clique
├── gerar-executavel-portatil.bat      # Compilador do pacote portátil
└── README.md                          # Documentação geral da solução
```

---

## 👥 Autoria & Licença
Projeto desenvolvido com fins acadêmicos para a atividade do Projeto Integrador Multidisciplinar (PIM) - Curso de Análise e Desenvolvimento de Sistemas (2026).
