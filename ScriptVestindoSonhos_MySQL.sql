-- ==============================================================================
-- PROJETO PIM - ONG VESTINDO SONHOS
-- Script DDL & Recursos Avançados Adaptados para MySQL (MySQL Workbench)
-- ==============================================================================

CREATE DATABASE IF NOT EXISTS VESTINDOSONHOSDB;
USE VESTINDOSONHOSDB;

-- ------------------------------------------------------------------------------
-- 1. CRIAÇÃO DAS TABELAS
-- ------------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS Cargos (
    id INT NOT NULL AUTO_INCREMENT,
    nome VARCHAR(15) NOT NULL,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS TiposAtivo (
    id INT NOT NULL AUTO_INCREMENT,
    categoria VARCHAR(15) NOT NULL UNIQUE,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS Usuario (
    id INT NOT NULL AUTO_INCREMENT,
    nome VARCHAR(50) NOT NULL,
    cargo INT NOT NULL,
    data_nascimento DATE,
    telefone VARCHAR(14),
    email VARCHAR(40) UNIQUE,
    endereco VARCHAR(80),
    cpf VARCHAR(11) NOT NULL UNIQUE,
    senha VARCHAR(255) NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT FK_Usuario_Cargo FOREIGN KEY (cargo) REFERENCES Cargos(id)
);

CREATE TABLE IF NOT EXISTS Eventos (
    id INT NOT NULL AUTO_INCREMENT,
    nome VARCHAR(50) NOT NULL,
    data_inicio DATETIME NOT NULL,
    data_fim DATETIME NOT NULL,
    local VARCHAR(70),
    responsavel INT NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT FK_Evento_Usuario FOREIGN KEY (responsavel) REFERENCES Usuario(id)
);

CREATE TABLE IF NOT EXISTS Voluntario (
    id INT NOT NULL AUTO_INCREMENT,
    nome VARCHAR(60) NOT NULL,
    cpf VARCHAR(11) NOT NULL,
    endereco VARCHAR(50),
    telefone VARCHAR(14),
    email VARCHAR(40),
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS Beneficiario (
    id INT NOT NULL AUTO_INCREMENT,
    nome VARCHAR(80) NOT NULL,
    telefone VARCHAR(15) NOT NULL,
    endereco VARCHAR(60) NOT NULL,
    situacao INT,
    observacao TEXT,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS VoluntarioAtribuido (
    evento INT NOT NULL,
    voluntario INT NOT NULL,
    PRIMARY KEY (evento, voluntario),
    CONSTRAINT FK_VA_Evento FOREIGN KEY (evento) REFERENCES Eventos(id),
    CONSTRAINT FK_VA_Voluntario FOREIGN KEY (voluntario) REFERENCES Voluntario(id)
);

CREATE TABLE IF NOT EXISTS Categoria (
    id INT NOT NULL AUTO_INCREMENT,
    nome VARCHAR(15) NOT NULL,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS Carteira (
    id INT NOT NULL AUTO_INCREMENT,
    nome VARCHAR(30) NOT NULL,
    saldo DECIMAL(10,2) NOT NULL,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS Transacao (
    id INT NOT NULL AUTO_INCREMENT,
    hora DATETIME,
    usuario INT NOT NULL,
    observacao VARCHAR(255),
    categoria INT NOT NULL,
    razao VARCHAR(255),
    PRIMARY KEY (id),
    CONSTRAINT FK_Transacao_Usuario FOREIGN KEY (usuario) REFERENCES Usuario(id),
    CONSTRAINT FK_Transacao_Categoria FOREIGN KEY (categoria) REFERENCES Categoria(id)
);

CREATE TABLE IF NOT EXISTS TransacaoFinanceira (
    id INT NOT NULL,
    valor DECIMAL(10, 2) NOT NULL,
    carteira INT NOT NULL,
    PRIMARY KEY (id),
    CONSTRAINT FK_TF_Transacao FOREIGN KEY (id) REFERENCES Transacao(id),
    CONSTRAINT FK_TF_Carteira FOREIGN KEY (carteira) REFERENCES Carteira(id)
);

CREATE TABLE IF NOT EXISTS Produto (
    id INT NOT NULL AUTO_INCREMENT,
    nome VARCHAR(60) NOT NULL,
    descricao VARCHAR(255) NOT NULL,
    cor VARCHAR(15) NOT NULL,
    categoria INT,
    PRIMARY KEY (id),
    CONSTRAINT FK_Produto_Tipo FOREIGN KEY (categoria) REFERENCES TiposAtivo(id)
);

CREATE TABLE IF NOT EXISTS TransacaoInventario (
    produto_id INT NOT NULL,
    transacao_id INT NOT NULL,
    quantidade INT NOT NULL,
    beneficiario INT,
    PRIMARY KEY (produto_id, transacao_id),
    CONSTRAINT FK_TI_Produto FOREIGN KEY (produto_id) REFERENCES Produto(id),
    CONSTRAINT FK_TI_Transacao FOREIGN KEY (transacao_id) REFERENCES Transacao(id),
    CONSTRAINT FK_TI_Beneficiario FOREIGN KEY (beneficiario) REFERENCES Beneficiario(id)
);

CREATE TABLE IF NOT EXISTS Dependente (
    id INT NOT NULL AUTO_INCREMENT,
    id_responsavel INT NOT NULL,
    nome VARCHAR(60) NOT NULL,
    ano_nascimento INT NOT NULL,
    sexo CHAR(1) NOT NULL,
    parentesco VARCHAR(20),
    PRIMARY KEY (id),
    CONSTRAINT FK_Dependente_Beneficiario FOREIGN KEY (id_responsavel) REFERENCES Beneficiario(id)
);

-- ------------------------------------------------------------------------------
-- 2. TABELA PRINCIPAL DE INVENTÁRIO (Controle Consolidado de Estoque)
-- ------------------------------------------------------------------------------

CREATE TABLE IF NOT EXISTS Inventario (
    id INT NOT NULL AUTO_INCREMENT,
    produto_id INT NOT NULL UNIQUE,
    quantidade INT NOT NULL DEFAULT 0,
    ultima_atualizacao DATETIME DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    CONSTRAINT FK_Inventario_Produto FOREIGN KEY (produto_id) REFERENCES Produto(id) ON DELETE CASCADE
);

-- Inicializa o inventário para produtos existentes caso ainda não estejam mapeados
INSERT IGNORE INTO Inventario (produto_id, quantidade, ultima_atualizacao)
SELECT p.id, 0, NOW()
FROM Produto p;

-- ------------------------------------------------------------------------------
-- 3. VIEW: Financeiro (Visão unificada para o módulo financeiro)
-- ------------------------------------------------------------------------------

CREATE OR REPLACE VIEW Financeiro AS
SELECT 
    id,
    hora,
    usuario,
    observacao,
    categoria,
    razao
FROM Transacao;

-- ------------------------------------------------------------------------------
-- 4. STORED PROCEDURE: sp_RelatorioFinanceiroResumido
-- Retorna Entradas, Saídas e Saldo do mês atual fazendo JOIN entre
-- TransacaoFinanceira e Financeiro
-- ------------------------------------------------------------------------------

DELIMITER //

DROP PROCEDURE IF EXISTS sp_RelatorioFinanceiroResumido //

CREATE PROCEDURE sp_RelatorioFinanceiroResumido(
    IN p_Mes INT,
    IN p_Ano INT
)
BEGIN
    -- Se não especificado (NULL), assume o mês e ano atuais
    SET p_Mes = COALESCE(p_Mes, MONTH(CURRENT_DATE()));
    SET p_Ano = COALESCE(p_Ano, YEAR(CURRENT_DATE()));

    SELECT 
        p_Mes AS MesReferencia,
        p_Ano AS AnoReferencia,
        COALESCE(SUM(CASE WHEN f.razao = 'Entrada' THEN tf.valor ELSE 0.00 END), 0.00) AS TotalEntradas,
        COALESCE(SUM(CASE WHEN f.razao IN ('Saida', 'Saída') THEN tf.valor ELSE 0.00 END), 0.00) AS TotalSaidas,
        COALESCE(SUM(CASE WHEN f.razao = 'Entrada' THEN tf.valor ELSE -tf.valor END), 0.00) AS SaldoLiquido,
        COUNT(tf.id) AS QuantidadeTransacoes
    FROM TransacaoFinanceira tf
    INNER JOIN Financeiro f ON tf.id = f.id
    WHERE MONTH(f.hora) = p_Mes 
      AND YEAR(f.hora) = p_Ano;
END //

DELIMITER ;

-- ------------------------------------------------------------------------------
-- 5. TRIGGER: trg_AtualizaEstoque
-- Atualiza a quantidade na tabela Inventario ao inserir em TransacaoInventario
-- ------------------------------------------------------------------------------

DELIMITER //

DROP TRIGGER IF EXISTS trg_AtualizaEstoque //

CREATE TRIGGER trg_AtualizaEstoque
AFTER INSERT ON TransacaoInventario
FOR EACH ROW
BEGIN
    DECLARE v_razao VARCHAR(255);

    -- 1. Garante que o produto existe na tabela Inventario
    INSERT IGNORE INTO Inventario (produto_id, quantidade, ultima_atualizacao)
    VALUES (NEW.produto_id, 0, NOW());

    -- 2. Busca a razão ('Entrada' ou 'Saida') na tabela de transações
    SELECT razao INTO v_razao
    FROM Transacao
    WHERE id = NEW.transacao_id
    LIMIT 1;

    -- 3. Atualiza o estoque conforme o tipo da movimentação
    IF v_razao = 'Entrada' THEN
        UPDATE Inventario
        SET quantidade = quantidade + NEW.quantidade,
            ultima_atualizacao = NOW()
        WHERE produto_id = NEW.produto_id;
    ELSEIF v_razao IN ('Saida', 'Saída') THEN
        UPDATE Inventario
        SET quantidade = quantidade - NEW.quantidade,
            ultima_atualizacao = NOW()
        WHERE produto_id = NEW.produto_id;
    END IF;
END //

DELIMITER ;
