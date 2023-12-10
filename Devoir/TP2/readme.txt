SCRIPT bd, nommer le bd "tp2":
-- 1. Create the 'admin' schema
CREATE SCHEMA admin;

-- 2. Create the 'users' table in the 'admin' schema
CREATE TABLE admin.users (
    user_id SERIAL PRIMARY KEY,
    username VARCHAR(255),
    password_hash VARCHAR(255),
    is_admin BOOLEAN,
    last_login_date TIMESTAMP WITH TIME ZONE,
    postgres_username VARCHAR(255),
    postgres_password VARCHAR(255)
);

-- 3. Add functions and procedure to the 'admin' schema

-- Function to create a restricted user
CREATE OR REPLACE FUNCTION admin.create_restricted_user(username VARCHAR, password VARCHAR)
RETURNS void AS $$
BEGIN
    EXECUTE format('CREATE USER %I WITH PASSWORD %L', username, password);
    EXECUTE format('GRANT SELECT ON ALL TABLES IN SCHEMA public TO %I', username);
    -- Add additional privileges as required
END;
$$ LANGUAGE plpgsql;

-- Function to promote user to admin
CREATE OR REPLACE FUNCTION admin.promote_to_admin(username VARCHAR)
RETURNS void AS $$
BEGIN
    EXECUTE format('ALTER USER %I WITH SUPERUSER', username);
END;
$$ LANGUAGE plpgsql;

-- Procedure to delete a restricted user
CREATE OR REPLACE PROCEDURE admin.delete_restricted_user(username VARCHAR)
LANGUAGE plpgsql AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM pg_catalog.pg_user WHERE usename = username) THEN
        EXECUTE 'DROP USER ' || quote_ident(username);
    ELSE
        RAISE EXCEPTION 'User does not exist';
    END IF;
END;
$$;

-- 4. Create the 'connaissance' table in the 'public' schema
CREATE TABLE public.connaissance (
    id SERIAL PRIMARY KEY,
    titre VARCHAR(255),
    description TEXT,
    champs JSONB
);

---------------------------------------------------------------------------------------------
remplir tableau procedure:
INSERT INTO public.connaissance (id, titre, description, champs) VALUES
(15, 'Ember', 'L''archère de la forêt, protectrice des animaux et de la nature.', '{"force": 4, "genre": "Légende", "defense": 4, "origine": "", "vitesse": 8, "dexterite": 7, "apparition": "2015", "specialite": "Arc et Katars", "arme_principale": "Arc", "arme_secondaire": "Katars"}'),
(18, 'Xull', 'ogre of despair', '{"force": "9", "genre": "Warrior", "defense": "6", "origine": "Ogre galaxy", "vitesse": "5", "dexterite": "5", "apparition": "2018", "specialite": "canon and axe", "arme_principale": "canon", "arme_secondaire": "axe"}'),
(12, 'Asuri', 'Un personnage agile et rapide, expert en arts martiaux.', '{"force": 5, "genre": "Légende", "defense": 4, "origine": "La jungle inexplorée", "vitesse": 7, "dexterite": 8, "apparition": "2015", "specialite": "Katar et épée", "arme_principale": "Griffes", "arme_secondaire": "Épée"}'),
(13, 'Orion', 'Un combattant mystérieux dont l''origine est inconnue.', '{"force": 6, "genre": "Légende", "defense": 8, "origine": "Inconnue", "vitesse": 6, "dexterite": 4, "apparition": "2014", "specialite": "Lance et fusil", "arme_principale": "Lance", "arme_secondaire": "Fusil à plasma"}'),
(14, 'Bodvar', 'Un guerrier viking cherchant à prouver sa valeur dans le Valhalla.', '{"force": "8", "genre": "Légende", "defense": "5", "origine": "Viking", "vitesse": "5", "dexterite": "6", "apparition": "2014", "specialite": "Marteau et épée", "arme_principale": "Marteau", "arme_secondaire": "Épée"}');
------------------------------------------------------------------------------------------------------
les utilisateurs:
-- 1. Create an admin user in PostgreSQL
CREATE USER admin_user WITH PASSWORD '1234';

-- 2. Add the admin user to the admin.users table
INSERT INTO admin.users (username, password_hash, is_admin, postgres_username, postgres_password)
VALUES ('admin_user', 'hash_of_admin_password', TRUE, 'admin_user', '1234');

-- 3. Promote the user to a superuser
ALTER USER admin_user WITH SUPERUSER;

-- 4. Create a restricted user in PostgreSQL
CREATE USER restricted_user WITH PASSWORD '1234';

-- 5. Add the restricted user to the admin.users table
INSERT INTO admin.users (username, password_hash, is_admin, postgres_username, postgres_password)
VALUES ('restricted_user', 'hash_of_restricted_password', FALSE, 'restricted_user', '1234');

-- 6. Grant restricted privileges to the restricted user
GRANT SELECT ON ALL TABLES IN SCHEMA public TO restricted_user;
-----------------------------------------------------------------------------------------------------------------
tp2_API:localhost port 5051, user:postgres, password:1234

tp2_interface:la connection va se faire tout seul. entre le username de l'admin et password.

a la creation du bd, nommer le tp2 en minuscule.



