CREATE VIEW viUser AS
    SELECT
        userid,
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
    FROM
         "user"
    WHERE
        deletetime IS NULL;

CREATE VIEW viGlassSize AS
    SELECT
        Id,
        size
    FROM
         glasssize
    WHERE
        deletetime IS NULL;

CREATE VIEW viDrink AS
    SELECT
        d.id,
        g.size
    FROM
        drink d JOIN
        viGlassSize g ON glasssizeid=g.Id;

CREATE VIEW viIngredient AS
    SELECT
        id,
        name,
        alcohollevel,
        background
    FROM
        ingredient
    WHERE
        deletetime IS NULL;

CREATE VIEW viMachine AS
    SELECT
        Id,
        name
    FROM
        machine
    WHERE
        deletetime IS NULL;

CREATE VIEW viContainer AS
    SELECT
        c.id,
        ingredientid,
        fillingvolume,
        fillinglevel,
        m.name MachineName
    FROM
        container c JOIN
        viMachine m on c.machineid=m.Id
    WHERE
        c.deletetime IS NULL
