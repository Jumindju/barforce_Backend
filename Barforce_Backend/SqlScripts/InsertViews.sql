CREATE VIEW viUser AS
SELECT userid,
       username,
       email,
       birthday,
       weight,
       password,
       salt,
       groups,
       gender,
       verified,
       currenttoken
FROM "user"
WHERE deletetime IS NULL;

CREATE VIEW viGlassSize AS
SELECT Id,
       size
FROM glasssize
WHERE deletetime IS NULL;

CREATE VIEW viDrink AS
SELECT d.id,
       g.size,
       g.id AS GlassSizeId
FROM drink d
         JOIN
     viGlassSize g ON glasssizeid = g.Id;

CREATE VIEW viIngredient AS
SELECT id,
       name,
       alcohollevel,
       background
FROM ingredient
WHERE deletetime IS NULL;

CREATE VIEW viMachine AS
SELECT Id,
       name
FROM machine
WHERE deletetime IS NULL;

CREATE VIEW viContainer AS
SELECT c.id,
       ingredientid,
       fillingvolume,
       fillinglevel,
       m.name    MachineName,
       m.id   as machineId,
       i.name AS ingredientName,
       i.alcohollevel,
       i.background
FROM container c
         JOIN
     viMachine m on c.machineid = m.Id
         JOIN
     viingredient i on i.id = c.ingredientid
WHERE c.deletetime IS NULL;

CREATE VIEW viFavoriteDrink AS
SELECT userid,
       drinkid,
       name,
       size AS GlassSize,
       GlassSizeId
FROM favoritedrink fd
         join vidrink d on fd.drinkid = d.id
WHERE deletetime IS NULL;