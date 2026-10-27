# Suíte de Testes Automatizados - ONG Vestindo Sonhos (`VestindoSonhos.Tests`)

Este projeto contém os **Testes Automatizados de Unidade e Integração** do sistema **Vestindo Sonhos**, desenvolvidos em **C# / .NET 10** utilizando o framework **xUnit** e banco de dados isolado em memória (**Entity Framework Core InMemory**).

## Tecnologias Utilizadas
- **xUnit (`2.9.2`)**: Framework de testes automatizados padrão da plataforma .NET (`[Fact]` e `[Theory]`).
- **Entity Framework Core InMemory (`9.0.0`)**: Banco de dados relacional em memória isolado por teste (`Guid.NewGuid()`), permitindo testar Controllers e regras de negócio sem afetar o banco MySQL de produção.
- **ASP.NET Core Identity & ClaimsPrincipal**: Simulação de autenticação e controle de acesso baseado em cargos (**RBAC**: `Administrador` vs. `Colaborador`).

---

## Estrutura dos Testes

| Arquivo | Módulo Testado | Casos de Teste |
| :--- | :--- | :--- |
| `VoluntariosApiTests.cs` | **Voluntários** | 1. Cadastro válido por Colaborador com higienização de CPF/Telefone<br>2. Rejeição de cadastro com nome vazio (`BadRequest`)<br>3. Listagem em ordem alfabética<br>4. Edição de dados cadastrais<br>5. Exclusão em cascata de escalas vinculadas (`VoluntarioAtribuido`) |
| `BeneficiariosApiTests.cs` | **Beneficiários & Dependentes** | 1. Cadastro de família beneficiária com situação padrão `"Ok"`<br>2. Validação de obrigatoriedade de nome (`BadRequest`)<br>3. Contagem agregada de dependentes por responsável<br>4. Exclusão em cascata de dependentes vinculados |
| `FinanceiroApiTests.cs` | **Financeiro & Carteiras** | 1. Lançamento de `Entrada` (Doação) somando ao saldo da carteira<br>2. Lançamento de `Saída` (Despesa) deduzindo do saldo e calculando resumo<br>3. Teste parametrizado (`[Theory]`) rejeitando valores `0` ou negativos<br>4. Estorno automático do saldo na carteira ao excluir transação |
| `DashboardAndRegrasNegocioTests.cs` | **Segurança (RBAC) & Dashboard** | 1. **Segurança RBAC**: Ocultação de saldo e transações financeiras para perfil `Colaborador`<br>2. **Segurança RBAC**: Exibição integral de indicadores financeiros para perfil `Administrador`<br>3. Validação de `#tags` de demandas reais (sem dados fictícios quando não há beneficiários)<br>4. Criptografia irreversível de senhas (`PasswordHasher<Usuario>` PBKDF2) |

---

## Como Executar os Testes

### Opção 1: Pelo Script Automatizado (1 Clique)
Execute o arquivo `rodar-testes-automatizados.bat` na raiz do projeto ou selecione a opção **`[9] Executar Testes Automatizados (.NET / xUnit)`** no `MENU_CENTRAL.bat`.

### Opção 2: Via Terminal (.NET CLI)
```bash
dotnet test ./VestindoSonhos.Tests/VestindoSonhos.Tests.csproj --verbosity normal
```
