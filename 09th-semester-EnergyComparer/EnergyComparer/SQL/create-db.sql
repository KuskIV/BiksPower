CREATE TABLE System(
    Id int NOT NULL AUTO_INCREMENT,
    Name VARCHAR(100),
    Os VARCHAR(100),
    Version INT,

    PRIMARY KEY (Id)
),

CREATE TABLE Experiment(
    Id int NOT NULL AUTO_INCREMENT,
    ConfigId INT,
    SystemId int,
    ProgramId int,
    Language VARCHAR(50),
    StartTime DATETIME(6),
    EndTime DATETIME(6),
    ProfilerId int,
    Runs int,
    Iteration INT,
    FirstProfiler VARCHAR(50),
    PRIMARY KEY (Id)
),

CREATE TABLE Program(
    Id int not NULL AUTO_INCREMENT,
    Name varchar(100),

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
),

CREATE TABLE Configuration(
    Id INT NOT NULL AUTO_INCREMENT,
    MinTemp INT,
    MaxTemp INT,
    MinutesBetweenExperiments INT,
    MinuteDurationOfExperiments INT,
    MinBattery INT,
    MaxBattery INT,
    Version INT,

    PRIMARY Key (Id)
),

CREATE TABLE RawData(
    Id int not NULL AUTO_INCREMENT,
    ExperimentId int,
    Value varchar(2000),
    Time DATETIME(6),

    PRIMARY KEY (Id)
),

CREATE TABLE Temperature(
    Id int not NULL AUTO_INCREMENT,
    ExperimentId int,
    Value decimal,
    Time DATETIME(6),
    Name VARCHAR(50),

    PRIMARY KEY (id)
),