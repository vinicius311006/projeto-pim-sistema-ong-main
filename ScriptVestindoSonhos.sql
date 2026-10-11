CREATE TABLE Usuario (
	id INTEGER NOT NULL IDENTITY UNIQUE,
	nome VARCHAR(50) NOT NULL,
	cargo INTEGER NOT NULL,
	data_nascimento DATE,
	telefone VARCHAR(14),
	email VARCHAR(40) UNIQUE,
	endereco VARCHAR(80),
	cpf VARCHAR(11) NOT NULL UNIQUE,
	senha VARCHAR(255) NOT NULL,
	PRIMARY KEY(id)
);


CREATE TABLE Cargos (
	id INTEGER NOT NULL IDENTITY UNIQUE,
	nome VARCHAR(15) NOT NULL,
	PRIMARY KEY(id)
) ;


CREATE TABLE TiposAtivo (
	id INTEGER NOT NULL IDENTITY UNIQUE,
	categoria VARCHAR(15) NOT NULL UNIQUE,
	PRIMARY KEY(id)
) ;


CREATE TABLE Eventos (
	id INTEGER NOT NULL IDENTITY UNIQUE,
	nome VARCHAR(50) NOT NULL,
	data_inicio DATETIME2(0) NOT NULL,
	data_fim DATETIME2(0) NOT NULL,
	local VARCHAR(70),
	responsavel INTEGER NOT NULL,
	PRIMARY KEY(id)
);


CREATE TABLE Voluntario (
	id INTEGER NOT NULL IDENTITY UNIQUE,
	nome VARCHAR(60) NOT NULL,
	cpf VARCHAR(11) NOT NULL,
	endereco VARCHAR(50),
	telefone VARCHAR(14),
	email VARCHAR(40),
	PRIMARY KEY(id)
);


CREATE TABLE Beneficiario (
	id INTEGER NOT NULL IDENTITY UNIQUE,
	nome VARCHAR(80) NOT NULL,
	telefone VARCHAR(15) NOT NULL,
	endereco VARCHAR(60) NOT NULL,
	situacao INTEGER,
	observacao VARCHAR(max),
	PRIMARY KEY(id)
);


CREATE TABLE VoluntarioAtribuido (
	evento INTEGER,
	voluntario INTEGER,
	PRIMARY KEY(evento, voluntario)
);


CREATE TABLE Categoria (
	id INTEGER NOT NULL IDENTITY UNIQUE,
	nome VARCHAR(15) NOT NULL,
	PRIMARY KEY(id)
) ;


CREATE TABLE Carteira (
	id INTEGER NOT NULL IDENTITY UNIQUE,
	nome VARCHAR(30) NOT NULL,
	saldo DECIMAL(10,2) NOT NULL,
	PRIMARY KEY(id)
);


CREATE TABLE Transacao (
	id INTEGER NOT NULL IDENTITY UNIQUE,
	hora DATETIME2(0),
	usuario INTEGER NOT NULL,
	observacao VARCHAR(255),
	categoria INTEGER NOT NULL,
	razao VARCHAR(255) CHECK(razao IN ('Entrada', 'Saida')),
	PRIMARY KEY(id)
);


CREATE TABLE TransacaoFinanceira (
	id INTEGER NOT NULL,
	valor DECIMAL(10, 2) NOT NULL,
	carteira INTEGER NOT NULL,
	PRIMARY KEY(id)
);


CREATE TABLE Produto (
	id INTEGER NOT NULL IDENTITY UNIQUE,
	nome VARCHAR(60) NOT NULL,
	descricao VARCHAR(255) NOT NULL,
	cor VARCHAR(15) NOT NULL,
	categoria INTEGER,
	PRIMARY KEY(id)
);


CREATE TABLE TransacaoInventario (
	produto_id INTEGER NOT NULL,
	transacao_id INTEGER NOT NULL,
	quantidade INTEGER NOT NULL,
	beneficiario INTEGER,
	PRIMARY KEY(produto_id, transacao_id)
);


CREATE TABLE Dependente (
	id INTEGER NOT NULL IDENTITY UNIQUE,
	id_responsavel INTEGER NOT NULL,
	nome VARCHAR(60) NOT NULL,
	ano_nascimento INTEGER NOT NULL,
	sexo CHAR(1) NOT NULL CHECK(sexo IN ('M', 'F')),
	parentesco VARCHAR(20),
	PRIMARY KEY(id)
);


ALTER TABLE VoluntarioAtribuido
ADD FOREIGN KEY(voluntario) REFERENCES Voluntario(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE VoluntarioAtribuido
ADD FOREIGN KEY(evento) REFERENCES Eventos(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE Produto
ADD FOREIGN KEY(categoria) REFERENCES TiposAtivo(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE TransacaoInventario
ADD FOREIGN KEY(transacao_id) REFERENCES Transacao(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE Dependente
ADD FOREIGN KEY(id_responsavel) REFERENCES Beneficiario(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE Eventos
ADD FOREIGN KEY(responsavel) REFERENCES Usuario(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE Usuario
ADD FOREIGN KEY(cargo) REFERENCES Cargos(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE Transacao
ADD FOREIGN KEY(usuario) REFERENCES Usuario(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE Transacao
ADD FOREIGN KEY(categoria) REFERENCES Categoria(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE TransacaoFinanceira
ADD FOREIGN KEY(carteira) REFERENCES Carteira(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE TransacaoInventario
ADD FOREIGN KEY(produto_id) REFERENCES Produto(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE TransacaoFinanceira
ADD FOREIGN KEY(id) REFERENCES Transacao(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
ALTER TABLE TransacaoInventario
ADD FOREIGN KEY(beneficiario) REFERENCES Beneficiario(id)
ON UPDATE NO ACTION ON DELETE NO ACTION;
GO

-- ============================================================================
-- FASE 3: BANCO DE DADOS AVANÇADO (SQL SERVER)
-- ============================================================================

-- ----------------------------------------------------------------------------
-- 1. TABELA PRINCIPAL DE INVENTÁRIO (Controle Consolidado de Estoque)
-- ----------------------------------------------------------------------------
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Inventario')
BEGIN
    CREATE TABLE Inventario (
        id INTEGER NOT NULL IDENTITY UNIQUE,
        produto_id INTEGER NOT NULL UNIQUE,
        quantidade INTEGER NOT NULL DEFAULT 0,
        ultima_atualizacao DATETIME2(0) DEFAULT GETDATE(),
        PRIMARY KEY(id),
        CONSTRAINT FK_Inventario_Produto FOREIGN KEY(produto_id) REFERENCES Produto(id)
        ON UPDATE NO ACTION ON DELETE CASCADE
    );
END;
GO

-- Inicializa o inventário com estoque zerado para produtos existentes caso ainda não estejam mapeados
INSERT INTO Inventario (produto_id, quantidade, ultima_atualizacao)
SELECT p.id, 0, GETDATE()
FROM Produto p
WHERE NOT EXISTS (SELECT 1 FROM Inventario inv WHERE inv.produto_id = p.id);
GO

-- ----------------------------------------------------------------------------
-- 2. VIEW: Financeiro
-- Permite consultar o histórico e cabeçalho das transações financeiras
-- através da nomenclatura 'Financeiro', conforme a modelagem corporativa.
-- ----------------------------------------------------------------------------
IF OBJECT_ID('Financeiro', 'V') IS NOT NULL
    DROP VIEW Financeiro;
GO

CREATE VIEW Financeiro AS
SELECT 
    id,
    hora,
    usuario,
    observacao,
    categoria,
    razao
FROM Transacao;
GO

-- ----------------------------------------------------------------------------
-- 3. STORED PROCEDURE: sp_RelatorioFinanceiroResumido
-- Realiza JOIN entre as tabelas TransacaoFinanceira e Financeiro
-- para retornar o total de Entradas, Saídas e Saldo Líquido do mês atual.
-- Parâmetros opcionais: @Mes e @Ano (padrão: mês e ano correntes).
-- ----------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE sp_RelatorioFinanceiroResumido
    @Mes INT = NULL,
    @Ano INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Caso não sejam fornecidos, assume o mês e o ano vigentes
    SET @Mes = COALESCE(@Mes, MONTH(GETDATE()));
    SET @Ano = COALESCE(@Ano, YEAR(GETDATE()));

    SELECT 
        @Mes AS MesReferencia,
        @Ano AS AnoReferencia,
        ISNULL(SUM(CASE WHEN f.razao = 'Entrada' THEN tf.valor ELSE 0.00 END), 0.00) AS TotalEntradas,
        ISNULL(SUM(CASE WHEN f.razao IN ('Saida', 'Saída') THEN tf.valor ELSE 0.00 END), 0.00) AS TotalSaidas,
        ISNULL(SUM(CASE WHEN f.razao = 'Entrada' THEN tf.valor ELSE -tf.valor END), 0.00) AS SaldoLiquido,
        COUNT(tf.id) AS QuantidadeTransacoes
    FROM TransacaoFinanceira tf
    INNER JOIN Financeiro f ON tf.id = f.id
    WHERE MONTH(f.hora) = @Mes 
      AND YEAR(f.hora) = @Ano;
END;
GO

-- ----------------------------------------------------------------------------
-- 4. TRIGGER: trg_AtualizaEstoque
-- Acionada na inserção de novos registros na tabela TransacaoInventario.
-- Identifica a razão ('Entrada' ou 'Saída') via JOIN com a tabela Transacao
-- e atualiza a quantidade de forma consistente e segura (inclusive em lote).
-- ----------------------------------------------------------------------------
CREATE OR ALTER TRIGGER trg_AtualizaEstoque
ON TransacaoInventario
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;

    -- 1. Garante que os produtos referenciados existam previamente na tabela Inventario
    INSERT INTO Inventario (produto_id, quantidade, ultima_atualizacao)
    SELECT DISTINCT i.produto_id, 0, GETDATE()
    FROM inserted i
    WHERE NOT EXISTS (
        SELECT 1 FROM Inventario inv WHERE inv.produto_id = i.produto_id
    );

    -- 2. Agrupa a variação de saldo (delta) por produto caso haja inserções múltiplas no mesmo lote
    ;WITH DeltasPorProduto AS (
        SELECT 
            i.produto_id,
            SUM(
                CASE 
                    WHEN t.razao = 'Entrada' THEN i.quantidade
                    WHEN t.razao IN ('Saida', 'Saída') THEN -i.quantidade
                    ELSE 0
                END
            ) AS DeltaQuantidade
        FROM inserted i
        INNER JOIN Transacao t ON i.transacao_id = t.id
        GROUP BY i.produto_id
    )
    -- 3. Atualiza o saldo consolidado no Inventario
    UPDATE inv
    SET 
        inv.quantidade = inv.quantidade + d.DeltaQuantidade,
        inv.ultima_atualizacao = GETDATE()
    FROM Inventario inv
    INNER JOIN DeltasPorProduto d ON inv.produto_id = d.produto_id;
END;
GO