DROP DATABASE IF EXISTS ChatApp;
CREATE DATABASE ChatApp;
USE ChatApp;

CREATE TABLE Utilizadores
(
    ID INTEGER UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    Username VARCHAR(15) NOT NULL,
    Password VARCHAR(32) NOT NULL,
    Imagem INTEGER(1) UNSIGNED DEFAULT NULL
)ENGINE=innoDB;

CREATE TABLE Mensagens
(
    ID INTEGER UNSIGNED PRIMARY KEY AUTO_INCREMENT,
    Texto TEXT(400) NOT NULL,
    dtaEnvio DATETIME DEFAULT NOW(),
    IDUtilizador INTEGER UNSIGNED NOT NULL,
    CONSTRAINT IDUtilizador_FK FOREIGN KEY(IDUtilizador) REFERENCES Utilizadores(ID) 
)ENGINE=innoDB;