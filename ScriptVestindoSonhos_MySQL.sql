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
    nome VARCHAR(50) NOT NULL,
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
    exigir_troca_senha TINYINT(1) NOT NULL DEFAULT 0,
    PRIMARY KEY (id),
    CONSTRAINT FK_Usuario_Cargo FOREIGN KEY (cargo) REFERENCES Cargos(id)
);

CREATE TABLE IF NOT EXISTS Eventos (
    id INT NOT NULL AUTO_INCREMENT,
    nome VARCHAR(50) NOT NULL,
    data_inicio DATETIME NOT NULL,
    data_fim DATETIME NOT NULL,
    local VARCHAR(70),
    observacao VARCHAR(255),
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
    situacao VARCHAR(20),
    observacao TEXT,
    PRIMARY KEY (id)
);

CREATE TABLE IF NOT EXISTS VoluntarioAtribuido (
    evento INT NOT NULL,
    voluntario INT NOT NULL,
    data_adicao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
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

-- ------------------------------------------------------------------------------
-- 6. VIEWS DE COMPATIBILIDADE COM O ENTITY FRAMEWORK CORE
-- Garante funcionamento independente se a consulta do EF Core usar singular ou plural
-- ------------------------------------------------------------------------------

CREATE OR REPLACE VIEW cargo AS SELECT * FROM Cargos;
CREATE OR REPLACE VIEW evento AS SELECT * FROM Eventos;
CREATE OR REPLACE VIEW tipos_ativo AS SELECT * FROM TiposAtivo;
CREATE OR REPLACE VIEW voluntario_atribuido AS SELECT * FROM VoluntarioAtribuido;

-- ------------------------------------------------------------------------------
-- 7. DADOS INICIAIS (SEEDS) PARA INICIALIZAÇÃO DO SISTEMA
-- Insere registros essenciais para testes e acesso com usuário Admin
-- ------------------------------------------------------------------------------

-- Cargos
INSERT IGNORE INTO Cargos (id, nome) VALUES 
(1, 'Administrador'),
(2, 'Colaborador');

-- Categorias de itens
INSERT IGNORE INTO TiposAtivo (id, categoria) VALUES
(1, 'Roupas'),
(2, 'Calçados'),
(3, 'Alimentos');

-- Usuários do Sistema para testes e apresentação (Todos com Senha inicial: 123456)
INSERT IGNORE INTO Usuario (id, nome, cargo, cpf, email, telefone, senha) VALUES 
(1, 'Admin Teste', 1, '12345678901', 'admin@teste.org', '(11) 98765-4321', '123456'),
(2, 'Mariana Colaboradora', 2, '23456789012', 'mariana@teste.org', '(11) 97654-3210', '123456'),
(3, 'Carlos Colaborador', 2, '34567890123', 'carlos@teste.org', '(11) 96543-2109', '123456');

-- Carteiras financeiras (Essenciais para registro financeiro)
INSERT IGNORE INTO Carteira (id, nome, saldo) VALUES 
(1, 'Conta Principal (Banco)', 5000.00),
(2, 'Caixa Físico (Sede)', 1500.00),
(3, 'Fundo de Reserva', 10000.00);

-- Categorias financeiras
INSERT IGNORE INTO Categoria (id, nome) VALUES 
(1, 'Doação'),
(2, 'Eventos'),
(3, 'Campanhas'),
(4, 'Despesas Operacionais');

-- Voluntários para teste
INSERT IGNORE INTO Voluntario (id, nome, cpf, endereco, telefone, email) VALUES 
(1, 'Carlos Eduardo Lima', '11122233344', 'Rua das Flores, 120', '11988887777', 'carlos.lima@ong.org'),
(2, 'Mariana Santos Silva', '55566677788', 'Av. Paulista, 1500', '11977776666', 'mariana.santos@ong.org');

-- Eventos iniciais
INSERT IGNORE INTO Eventos (id, nome, data_inicio, data_fim, local, responsavel, observacao) VALUES 
(1, 'Campanha do Agasalho 2026', '2026-06-01 09:00:00', '2026-06-10 18:00:00', 'Ginásio Municipal', 1, 'Arrecadação de casacos e cobertores'),
(2, 'Bazar Beneficente de Outono', '2026-10-15 09:00:00', '2026-10-15 17:00:00', 'Sede Central da ONG', 1, 'Venda comunitária e distribuição');

-- Associação inicial
INSERT IGNORE INTO VoluntarioAtribuido (evento, voluntario, data_adicao) VALUES
(1, 1, NOW()),
(1, 2, NOW());
