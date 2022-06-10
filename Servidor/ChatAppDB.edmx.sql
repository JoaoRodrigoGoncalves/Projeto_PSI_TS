
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 05/30/2022 17:14:52
-- Generated from EDMX file: C:\Users\João Gonçalves\Desktop\Projetos\Projeto_PSI_TS\Servidor\ChatAppDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [ChatApp];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_IDUtilizador_FK]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Mensagens] DROP CONSTRAINT [FK_IDUtilizador_FK];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Mensagens]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Mensagens];
GO
IF OBJECT_ID(N'[dbo].[Utilizadores]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Utilizadores];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Mensagens'
CREATE TABLE [dbo].[Mensagens] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Texto] varchar(max)  NOT NULL,
    [dtaEnvio] datetime  NOT NULL,
    [IDUtilizador] int  NOT NULL
);
GO

-- Creating table 'Utilizadores'
CREATE TABLE [dbo].[Utilizadores] (
    [ID] int IDENTITY(1,1) NOT NULL,
    [Username] varchar(15)  NOT NULL,
    [SaltedPassword] varbinary(max)  NOT NULL,
    [userImage] int  NULL,
    [Salt] varbinary(max)  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [ID] in table 'Mensagens'
ALTER TABLE [dbo].[Mensagens]
ADD CONSTRAINT [PK_Mensagens]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- Creating primary key on [ID] in table 'Utilizadores'
ALTER TABLE [dbo].[Utilizadores]
ADD CONSTRAINT [PK_Utilizadores]
    PRIMARY KEY CLUSTERED ([ID] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [IDUtilizador] in table 'Mensagens'
ALTER TABLE [dbo].[Mensagens]
ADD CONSTRAINT [FK_IDUtilizador_FK]
    FOREIGN KEY ([IDUtilizador])
    REFERENCES [dbo].[Utilizadores]
        ([ID])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_IDUtilizador_FK'
CREATE INDEX [IX_FK_IDUtilizador_FK]
ON [dbo].[Mensagens]
    ([IDUtilizador]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------