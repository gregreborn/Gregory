-- SCRIPT FOR PgAdmin: TP2

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

-- 3. Define functions and procedures in the 'admin' schema

-- Function: Create a restricted user
CREATE OR REPLACE FUNCTION admin.create_restricted_user(username VARCHAR, password VARCHAR)
RETURNS void AS $$
BEGIN
    EXECUTE format('CREATE USER %I WITH PASSWORD %L', username, password);
    EXECUTE format('GRANT SELECT ON ALL TABLES IN SCHEMA public TO %I', username);
    -- Additional privileges can be added here
END;
$$ LANGUAGE plpgsql;

-- Function: Promote user to admin
CREATE OR REPLACE FUNCTION admin.promote_to_admin(username VARCHAR)
RETURNS void AS $$
BEGIN
    EXECUTE format('ALTER USER %I WITH SUPERUSER', username);
END;
$$ LANGUAGE plpgsql;

-- Procedure: Delete a restricted user
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

-- Insert data into 'connaissance' table
INSERT INTO public.connaissance (id, titre, description, champs) VALUES
(15, 'Ember', 'L''archère de la forêt, protectrice des animaux et de la nature.', '{"force": 4, "genre": "Légende", "defense": 4, "origine": "", "vitesse": 8, "dexterite": 7, "apparition": "2015", "specialite": "Arc et Katars", "arme_principale": "Arc", "arme_secondaire": "Katars"}'),
(18, 'Xull', 'ogre of despair', '{"force": "9", "genre": "Warrior", "defense": "6", "origine": "Ogre galaxy", "vitesse": "5", "dexterite": "5", "apparition": "2018", "specialite": "canon and axe", "arme_principale": "canon", "arme_secondaire": "axe"}'),
(12, 'Asuri', 'Un personnage agile et rapide, expert en arts martiaux.', '{"force": 5, "genre": "Légende", "defense": 4, "origine": "La jungle inexplorée", "vitesse": 7, "dexterite": 8, "apparition": "2015", "specialite": "Katar et épée", "arme_principale": "Griffes", "arme_secondaire": "Épée"}'),
(13, 'Orion', 'Un combattant mystérieux dont l''origine est inconnue.', '{"force": 6, "genre": "Légende", "defense": 8, "origine": "Inconnue", "vitesse": 6, "dexterite": 4, "apparition": "2014", "specialite": "Lance et fusil", "arme_principale": "Lance", "arme_secondaire": "Fusil à plasma"}'),
(14, 'Bodvar', 'Un guerrier viking cherchant à prouver sa valeur dans le Valhalla.', '{"force": "8", "genre": "Légende", "defense": "5", "origine": "Viking", "vitesse": "5", "dexterite": "6", "apparition": "2014", "specialite": "Marteau et épée", "arme_principale": "Marteau", "arme_secondaire": "Épée"}');
-- User Management in separate transactions using DO command

-- Ensure pgcrypto is available
CREATE EXTENSION IF NOT EXISTS pgcrypto;
-- Create an admin user with last_login_date
DO $$
DECLARE
    v_admin_pass TEXT := '1234';
    v_hashed_admin_pass TEXT;
    v_encrypted_admin_pass TEXT;
    v_restricted_pass TEXT := '1234';
    v_hashed_restricted_pass TEXT;
    v_encrypted_restricted_pass TEXT;
BEGIN
    -- Hashing admin password
    v_hashed_admin_pass := crypt(v_admin_pass, gen_salt('bf'));
    v_encrypted_admin_pass := v_hashed_admin_pass; -- Assign the hashed password directly

    -- Hashing restricted user password
    v_hashed_restricted_pass := crypt(v_restricted_pass, gen_salt('bf'));
    v_encrypted_restricted_pass := v_hashed_restricted_pass; -- Assign the hashed password directly

    -- Creating the admin_user PostgreSQL user
    EXECUTE format('CREATE USER %I WITH PASSWORD %L', 'admin_user', v_hashed_admin_pass);

    -- Insert admin user into admin.users
    INSERT INTO admin.users (username, password_hash, is_admin, last_login_date, postgres_username, postgres_password)
    VALUES ('admin_user', v_hashed_admin_pass, TRUE, CURRENT_TIMESTAMP, 'admin_user', v_encrypted_admin_pass);

    -- Make admin user a PostgreSQL superuser
    EXECUTE format('ALTER USER %I WITH SUPERUSER', 'admin_user');

    -- Creating the restricted_user PostgreSQL user
    EXECUTE format('CREATE USER %I WITH PASSWORD %L', 'restricted_user', v_hashed_restricted_pass);

    -- Insert restricted user into admin.users
    INSERT INTO admin.users (username, password_hash, is_admin,last_login_date, postgres_username, postgres_password)
    VALUES ('restricted_user', v_hashed_restricted_pass, FALSE, CURRENT_TIMESTAMP, 'restricted_user', v_encrypted_restricted_pass);

    -- Grant necessary privileges for the restricted user
    EXECUTE format('GRANT SELECT ON ALL TABLES IN SCHEMA public TO %I', 'restricted_user');
END $$;

-- End of script

