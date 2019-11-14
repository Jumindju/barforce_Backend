INSERT INTO "machine"
(
 name
)
VALUES
(
    'DEFAULT'
);

INSERT INTO "Barforce".public.ingredient
(
 name,
 alcohollevel,
 background
)
VALUES
(
    'Coca Cola',
    0,
    '#000000'
);

INSERT INTO "Barforce".public.ingredient
(
 name,
 alcohollevel,
 background
)
VALUES
(
    'Fanta',
    0,
    '#00FFFF'
);

INSERT INTO "Barforce".public.ingredient
(
 name,
 alcohollevel,
 background
)
VALUES
(
    'Korn',
    32,
    '#111222'
);

INSERT INTO "Barforce".public.ingredient
(
 name,
 alcohollevel,
 background
)
VALUES
(
    'Wodka',
    38,
    '#123123'
);

INSERT INTO "Barforce".public.container(
    machineid, ingredientid, fillingvolume
)
VALUES (
        1,1,5
       );
INSERT INTO "Barforce".public.container(
    machineid, ingredientid, fillingvolume
)
VALUES (
        1,2,5
       );
INSERT INTO "Barforce".public.container(
    machineid, ingredientid, fillingvolume
)
VALUES (
        1,3,5
       );
INSERT INTO "Barforce".public.container(
    machineid, ingredientid, fillingvolume
)
VALUES (
        1,4,5
       );

INSERT INTO "Barforce".public.glasssize(size) VALUES (250);
INSERT INTO "Barforce".public.glasssize(size) VALUES (330);
INSERT INTO "Barforce".public.glasssize(size) VALUES (500);
INSERT INTO "Barforce".public.glasssize(size) VALUES (1000);