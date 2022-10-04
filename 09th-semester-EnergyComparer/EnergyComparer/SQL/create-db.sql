CREATE TABLE System(
    Id int NOT NULL AUTO_INCREMENT,
    Name VARCHAR(100),
    Os VARCHAR(100),
    Version INT,

    PRIMARY KEY (Id)
),

CREATE TABLE Experiment(
    Id int NOT NULL AUTO_INCREMENT,
    SystemId int,
    Version int,
    ProgramId int,
    Language VARCHAR(50),
    StartTime DATETIME,
    EndTime DATETIME,
    ProfilerId int,
    Runs int,

    PRIMARY KEY (Id)
),

CREATE TABLE Temperature(
    Id int not NULL AUTO_INCREMENT,
    ExperimentId int,
    Value decimal,
    Time DATETIME,
    Name VARCHAR(50),

    PRIMARY KEY (id)
),

CREATE TABLE Program(
    Id int not NULL AUTO_INCREMENT,
    Name varchar(100),

    PRIMARY KEY (Id)
),

CREATE TABLE RawData(
    Id int not NULL AUTO_INCREMENT,
    ExperimentId int,
    Value varchar(1000),
    Time DATETIME,

    PRIMARY KEY (Id)
),

CREATE TABLE Profiler(
    Id INT NOT NULL AUTO_INCREMENT,
    Name VARCHAR(100),

    PRIMARY KEY (Id)
),

CREATE TABLE Run(
    Id INT NOT NULL AUTO_INCREMENT,
    SystemId INT,
    ProgramId INT,

    PRIMARY KEY (Id)
)