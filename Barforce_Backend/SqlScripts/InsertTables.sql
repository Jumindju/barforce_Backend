CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE "user"
(
    UserId       SERIAL PRIMARY KEY,
    UserName     VARCHAR(64)                 NOT NULL UNIQUE,
    BirthDay     timestamp without time zone NOT NULL,
    EMail        VARCHAR(256)                NOT NULL,
    Weight       INT                         NULL,
    Password     VARCHAR(128)                NOT NULL,
    Salt         CHAR(16)                    NOT NULL,
    Groups       smallint                    NOT NULL DEFAULT 0,
    Gender       BOOLEAN                     NOT NULL,
    Verified     uuid                        NULL     DEFAULT uuid_generate_v4(),
    CurrentToken uuid                        NULL,
    CreationTime timestamp                            default now(),
    DeleteTime   timestamp                   NULL
);

CREATE TABLE GlassSize
(
    Id           SERIAL PRIMARY KEY,
    Size         INT       NOT NULL UNIQUE,
    CreationTime timestamp without time zone default (now() at time zone 'utc'),
    DeleteTime   timestamp NULL
);

CREATE TABLE Drink
(
    Id           SERIAL PRIMARY KEY,
    GlassSizeId  INT NOT NULL REFERENCES GlassSize (Id),
    CreationTime timestamp without time zone default (now() at time zone 'utc')
);

CREATE TABLE "order"
(
    Id        SERIAL PRIMARY KEY,
    UserId    INT REFERENCES "user" (UserId),
    DrinkId   INT REFERENCES Drink (Id),
    OrderDate timestamp without time zone default (now() at time zone 'utc'),
    ServeTime timestamp without time zone NULL
);

CREATE TABLE Ingredient
(
    Id           SERIAL PRIMARY KEY,
    Name         varchar(64)  NOT NULL UNIQUE,
    AlcoholLevel decimal      NOT NULL,
    Background   varchar(256) NOT NULL,
    CreationTime timestamp without time zone default (now() at time zone 'utc'),
    DeleteTime   timestamp    NULL
);

CREATE TABLE Drink2Liquid
(
    IngredientId INT NOT NULL REFERENCES Ingredient (Id),
    DrinkId      INT NOT NULL REFERENCES Drink (Id),
    Amount       INT NOT NULL
);

CREATE TABLE Machine
(
    Id           SERIAL PRIMARY KEY,
    Name         varchar(64) NOT NULL UNIQUE,
    CreationTime timestamp without time zone default (now() at time zone 'utc'),
    DeleteTime   timestamp   NULL
);

CREATE TABLE Container
(
    Id            SERIAL PRIMARY KEY,
    MachineId     INT REFERENCES Machine (Id),
    IngredientId  INT REFERENCES Ingredient (Id),
    FillingVolume INT       NOT NULL,
    FillingLevel  INT       NOT NULL          DEFAULT 0,
    CreationTime  timestamp without time zone default (now() at time zone 'utc'),
    DeleteTime    timestamp NULL
);

CREATE TABLE FavoriteDrink
(
    UserId       INT                         NOT NULL REFERENCES "user" (UserId),
    DrinkId      INT                         NOT NULL REFERENCES drink (id),
    Name         varchar(64)                 NOT NULL,
    CreationTime timestamp without time zone default (now() at time zone 'utc'),
    DeleteTime   timestamp without time zone NULL
);