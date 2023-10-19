--
-- PostgreSQL database dump
--

-- Dumped from database version 15.4
-- Dumped by pg_dump version 15.4

-- Started on 2023-10-19 15:43:03

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- TOC entry 254 (class 1255 OID 33453)
-- Name: award_and_complete_quest(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.award_and_complete_quest(character_id_param integer) RETURNS TABLE(experience_awarded integer, money_awarded numeric, equipment_awarded_id integer)
    LANGUAGE plpgsql
    AS $$DECLARE
    base_exp_reward INT;
    base_money_reward DECIMAL(10,2);
    equipment_reward INT;
BEGIN
    -- Fetch base rewards for the quest
    SELECT q.reward_experience, q.reward_money, q.equipment_reward_id 
    INTO base_exp_reward, base_money_reward, equipment_reward
    FROM quests q
    JOIN character_quests cq ON q.id = cq.quest_id
    WHERE cq.character_id = character_id_param AND cq.status = 'assigned';

    -- Output the base experience and money being awarded
    RAISE NOTICE 'Base Experience being awarded: %', base_exp_reward;
    RAISE NOTICE 'Base Money being awarded: %', base_money_reward;

    -- Update the character's experience and money with base values
    UPDATE characters
    SET experience = COALESCE(experience, 0) + base_exp_reward,
        money = COALESCE(money,0) + base_money_reward
    WHERE id = character_id_param;

    -- If there's equipment reward, call the equip_better_equipment function
    IF equipment_reward IS NOT NULL THEN
        PERFORM equip_better_item(character_id_param, equipment_reward);
    END IF;

    RETURN QUERY SELECT base_exp_reward as experience_awarded, base_money_reward as money_awarded, equipment_reward as equipment_awarded_id;
    
END;
$$;


ALTER FUNCTION public.award_and_complete_quest(character_id_param integer) OWNER TO postgres;

--
-- TOC entry 252 (class 1255 OID 33430)
-- Name: cast_spell(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.cast_spell(character_id integer, spell_id integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    spell_effect_type VARCHAR(50);
    spell_effect_value INT;
    character_hp INT;
    character_mp INT;
BEGIN
    -- Fetch spell details
    SELECT effect_type, effect_value INTO spell_effect_type, spell_effect_value 
    FROM spells WHERE id = spell_id;

    -- Fetch character details
    SELECT hp, mp INTO character_hp, character_mp 
    FROM characters WHERE id = character_id;

    -- Cast the spell if it's a heal spell
    IF LOWER(spell_effect_type) = 'heal' THEN
        UPDATE characters 
        SET hp = hp + spell_effect_value
        WHERE id = character_id;

        -- Deduct the mana cost from character's MP
        UPDATE characters 
        SET mp = mp - (SELECT mana_cost FROM spells WHERE id = spell_id)
        WHERE id = character_id;
    END IF;

END;
$$;


ALTER FUNCTION public.cast_spell(character_id integer, spell_id integer) OWNER TO postgres;

--
-- TOC entry 253 (class 1255 OID 33458)
-- Name: check_and_level_down(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.check_and_level_down(character_id integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    char_level INT;
    char_strength INT;
    char_dexterity INT;
    threshold_exp INT;
    prev_strength_increment INT;
    prev_dexterity_increment INT;
BEGIN
    -- Fetch the character's level, strength, and dexterity
    SELECT level, strength, dexterity INTO char_level, char_strength, char_dexterity
    FROM characters WHERE id = character_id;

    IF char_level = 1 THEN 
        RETURN; -- If character is already at level 1, exit early.
    END IF;

    -- Determine the threshold experience and stat increments for the current level
    CASE
        WHEN char_level = 2 THEN 
            threshold_exp := 100;
            prev_strength_increment := 5;
            prev_dexterity_increment := 5;
        WHEN char_level = 3 THEN 
            threshold_exp := 250;
            prev_strength_increment := 10;
            prev_dexterity_increment := 10;
        ELSE 
            threshold_exp := (char_level - 1) * 200;
            prev_strength_increment := 15; 
            prev_dexterity_increment := 15;
    END CASE;

    -- Level down the character and decrease stats
    char_level := char_level - 1;
    char_strength := char_strength - prev_strength_increment;
    char_dexterity := char_dexterity - prev_dexterity_increment;

       -- Update the character's level, experience, strength, and dexterity in the database
    UPDATE characters
    SET level = char_level,
        experience = CASE
            WHEN experience < 0 THEN threshold_exp
            ELSE experience
        END,
        strength = char_strength,
        dexterity = char_dexterity
    WHERE id = character_id;


END;
$$;


ALTER FUNCTION public.check_and_level_down(character_id integer) OWNER TO postgres;

--
-- TOC entry 251 (class 1255 OID 33377)
-- Name: check_and_level_up(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.check_and_level_up(character_id integer) RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    char_level INT;
    char_exp INT;
    char_strength INT;
    char_dexterity INT;
    char_max_hp INT;
    threshold_exp INT;
    strength_increment INT;
    dexterity_increment INT;
    hp_increment INT;
BEGIN
    -- Fetch the character's level, experience, strength, dexterity, and max_hp
    SELECT level, experience, strength, dexterity, max_hp INTO char_level, char_exp, char_strength, char_dexterity, char_max_hp
    FROM characters WHERE id = character_id;

    LOOP
        -- Determine the threshold experience, stat increments, and hp increment for the next level
        CASE
			WHEN char_level = 1 THEN 
				threshold_exp := 100;
				strength_increment := 5;
				dexterity_increment := 5;
				hp_increment := 10;
			WHEN char_level = 2 THEN 
				threshold_exp := 250;
				strength_increment := 10;
				dexterity_increment := 10;
				hp_increment := 20;
			WHEN char_level = 3 THEN 
				threshold_exp := 500;
				strength_increment := 15;
				dexterity_increment := 15;
				hp_increment := 30;
			ELSE 
				threshold_exp := char_level * 200;
				strength_increment := 20;
				dexterity_increment := 20;
				hp_increment := 40;
		END CASE;


        -- Exit loop if the character hasn't exceeded the threshold
        IF char_exp < threshold_exp THEN
            EXIT;
        END IF;
		
        RAISE NOTICE 'Current char_exp: %', char_exp;
        -- Level up the character and increase stats
        char_level := char_level + 1;
		char_exp := GREATEST(0, char_exp - threshold_exp);
        char_strength := char_strength + strength_increment;
        char_dexterity := char_dexterity + dexterity_increment;
        char_max_hp := char_max_hp + hp_increment;  

    END LOOP;

    -- Update the character's level, experience, strength, dexterity, and max_hp in the database
    UPDATE characters
    SET level = char_level,
        experience = char_exp,
        strength = char_strength,
        dexterity = char_dexterity,
        max_hp = char_max_hp  
    WHERE id = character_id;

END;
$$;


ALTER FUNCTION public.check_and_level_up(character_id integer) OWNER TO postgres;

--
-- TOC entry 256 (class 1255 OID 33468)
-- Name: combat(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.combat(char_id integer, monster_id integer) RETURNS text
    LANGUAGE plpgsql
    AS $$DECLARE
    monster_type VARCHAR(255);
    char_hp INT;
    char_mp INT;
    mon_hp INT;
    char_strength INT;
    mon_strength INT;
    outcome TEXT;
    heal_spell_id INT;
    boss_multiplier DECIMAL;
    char_avg_stat INT;
BEGIN
    -- Fetch the character's current HP, MP, and strength from the characters table
    SELECT c.hp, c.mp, c.strength INTO char_hp, char_mp, char_strength
    FROM characters AS c WHERE c.id = char_id;

    -- Calculate average stats for character
    SELECT (strength + dexterity) / 2 INTO char_avg_stat
    FROM characters
    WHERE id = char_id;

    -- Get the specific monster for the combat scenario from the monsters table using the provided monster_id
    SELECT m.type, m.base_hp, m.base_strength INTO monster_type, mon_hp, mon_strength
    FROM monsters AS m
    WHERE m.id = monster_id;

    -- If it's a boss, adjust its stats based on character's stats
    IF monster_type = 'Boss' THEN
        boss_multiplier := 1 + (char_avg_stat * 0.1);
        mon_hp := mon_hp * boss_multiplier;
        mon_strength := mon_strength * boss_multiplier;
    END IF;

    -- Get the ID of the healing spell for the character from the character_spells table directly
    SELECT cs.spell_id INTO heal_spell_id 
    FROM character_spells AS cs
    WHERE cs.character_id = char_id AND cs.spell_id IN (SELECT s.id FROM spells AS s WHERE s.effect_type = 'heal');

    -- Combat loop
    WHILE char_hp > 0 AND mon_hp > 0 LOOP
        -- Character attacks first
        mon_hp := mon_hp - char_strength;
        
        -- Notify when the character attacks
        RAISE NOTICE 'Character attacks for % damage!', char_strength;

        -- If character's health is below 50%, try to heal
        IF char_hp <= 0.5 * (SELECT c1.max_hp FROM characters AS c1 WHERE c1.id = char_id) THEN
            IF heal_spell_id IS NOT NULL AND char_mp >= (SELECT s1.mana_cost FROM spells AS s1 WHERE s1.id = heal_spell_id) THEN
                PERFORM cast_spell(char_id, heal_spell_id);
                -- Output a notice indicating the heal spell has been cast
                RAISE NOTICE 'Character with ID % casted the heal spell.', char_id;
                -- Update char_hp and char_mp after casting the spell directly from the characters table
                SELECT c2.hp, c2.mp INTO char_hp, char_mp FROM characters AS c2 WHERE c2.id = char_id;
            END IF;
        END IF;

        -- If monster is still alive, it retaliates
        IF mon_hp > 0 THEN
            char_hp := GREATEST(0, char_hp - mon_strength);
            
            -- Notify when the character is attacked by the monster
            RAISE NOTICE 'Monster attacks for % damage!', mon_strength;
        END IF;
    END LOOP;

    -- Determine outcome and include the monster's type
    IF char_hp > 0 THEN
        outcome := 'Victory' || '!';
    ELSE
        outcome := 'Defeat'  || '!';
        char_hp := 0; -- Ensure HP doesn't go below 0
    END IF;

    -- Update character's HP after the battle directly in the characters table
    UPDATE characters AS c
    SET hp = char_hp
    WHERE c.id = char_id;

    RETURN outcome;
END;
$$;


ALTER FUNCTION public.combat(char_id integer, monster_id integer) OWNER TO postgres;

--
-- TOC entry 259 (class 1255 OID 33466)
-- Name: complete_quest_with_combat(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.complete_quest_with_combat(character_id_param integer, quest_id_param integer) RETURNS text
    LANGUAGE plpgsql
    AS $$DECLARE
    outcome TEXT;
    victories INT := 0;
    total_monsters INT;
    monsters_defeated INT := 0;
    monster_id_current INT; 
    calculated_exp INT;
    calculated_money DECIMAL(10,2);
    calculated_equipment_id INT;
	monster_name_current TEXT;
	    equipment_reward INT;
BEGIN
    -- Determine the total count of all monsters in the quest
    SELECT SUM(qm.monster_count) INTO total_monsters 
    FROM quest_monsters qm
    WHERE qm.quest_id = quest_id_param;
	
	DELETE FROM quest_temp_monsters;
	INSERT INTO quest_temp_monsters 
	SELECT * FROM quest_monsters 
	WHERE quest_id = quest_id_param;

	
    RAISE NOTICE 'Total monster count in the quest: %', total_monsters;

 
    WHILE monsters_defeated < total_monsters LOOP
        -- Get the next monster associated with the character's current quest
        SELECT qm.monster_id INTO monster_id_current 
        FROM quest_monsters qm 
        WHERE qm.quest_id = quest_id_param 
        LIMIT 1; 
		
		SELECT  m.type INTO  monster_name_current 
		FROM quest_monsters qm 
		JOIN monsters m ON m.id = qm.monster_id
		WHERE qm.quest_id = quest_id_param 
		LIMIT 1; 


		RAISE NOTICE 'Fighting monster';

        -- Call the combat function
        outcome := combat(character_id_param, monster_id_current);

        RAISE NOTICE 'Combat outcome: %', outcome;

        -- Check the outcome and increment victories if won
        IF POSITION('Victory' IN outcome) > 0 THEN
            victories := victories + 1;
            monsters_defeated := monsters_defeated + 1;

            -- Delete the defeated monster entry from quest_monsters
            DELETE FROM quest_monsters
            WHERE quest_id = quest_id_param AND monster_id = monster_id_current;

			RAISE NOTICE 'Defeated monster';
        ELSE 
            -- Refill character's HP and mana before penalizing
            UPDATE characters SET hp = max_hp, mp = max_mp WHERE id = character_id_param;

            -- Apply the penalty for failing the quest
			PERFORM penalty_for_failure(character_id_param); 
			--Update quest status to 'failed'
            UPDATE character_quests
            SET status = 'failed'
            WHERE character_id = character_id_param AND status = 'assigned';
            
            -- And then assign a new quest and return with a failure message
            PERFORM generate_and_assign_quest(character_id_param);
            
            RAISE NOTICE 'Failed to defeat monster. Assigning new quest.';
            
            RETURN 'Failed to complete quest. Defeated ' || victories || ' out of ' || total_monsters || ' monsters.';
        END IF;
    END LOOP;
	
	RAISE NOTICE 'Monsters defeated: %, Total monsters: %', monsters_defeated, total_monsters;
    -- If all monsters were defeated, apply the calculated rewards
    IF monsters_defeated = total_monsters THEN

       


        UPDATE character_quests
        SET status = 'complete'
        WHERE character_id = character_id_param AND status = 'assigned';

        -- Call the check and level up function
        PERFORM check_and_level_up(character_id_param);
        -- Assign a new quest upon successful completion
        PERFORM generate_and_assign_quest(character_id_param);
        PERFORM award_and_complete_quest(character_id_param);
        RAISE NOTICE 'All monsters defeated. Quest completed. Assigning new quest.';
        RETURN 'Successfully completed the quest by defeating all ' || total_monsters || ' monsters!';
    END IF;
	RETURN 'Unexpected outcome. Please check the quest details and combat scenarios.';


END;$$;


ALTER FUNCTION public.complete_quest_with_combat(character_id_param integer, quest_id_param integer) OWNER TO postgres;

--
-- TOC entry 257 (class 1255 OID 33446)
-- Name: equip_better_item(integer, integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.equip_better_item(character_id_param integer, equip_id_param integer) RETURNS void
    LANGUAGE plpgsql
    AS $$DECLARE
    equip_type VARCHAR(50);
    current_equipment_stat_bonus INT;
    new_equipment_stat_bonus INT;
    character_money DECIMAL;
    equipment_price DECIMAL;
    equip_category VARCHAR(50);
BEGIN
    -- Fetch the type, price, and category of the equipment
    SELECT type, price, category INTO equip_type, equipment_price, equip_category
    FROM equipment
    WHERE id = equip_id_param;

    -- Get stat bonus for the new equipment
    SELECT stat_bonus INTO new_equipment_stat_bonus
    FROM equipment
    WHERE id = equip_id_param;

    -- Check if character already has an equipment in the same category
    IF EXISTS (SELECT 1 FROM character_equipment ce WHERE ce.character_id = character_id_param AND ce.category = equip_category) THEN
        -- Get stat bonuses for the current equipment
        SELECT stat_bonus INTO current_equipment_stat_bonus
        FROM character_equipment ce
        JOIN equipment e ON ce.equipment_id = e.id
        WHERE ce.character_id = character_id_param AND ce.category = equip_category;

        -- If the new equipment is better, then check if the character can afford it
        IF new_equipment_stat_bonus > current_equipment_stat_bonus THEN
            
            -- Fetch character's current money
            SELECT money INTO character_money
            FROM characters WHERE id = character_id_param;

            -- If the character cannot afford the equipment, exit the function early
            IF character_money < equipment_price THEN
                RETURN;
            END IF;

            -- Equip the item and deduct the money
            UPDATE character_equipment
            SET equipment_id = equip_id_param
            WHERE character_id = character_id_param AND category = equip_category;

            UPDATE characters
            SET money = money - equipment_price
            WHERE id = character_id_param;

        END IF;
    ELSE

        -- Fetch character's current money
        SELECT money INTO character_money
        FROM characters WHERE id = character_id_param;

        -- If the character cannot afford the equipment, exit the function early
        IF character_money < equipment_price THEN
            RETURN;
        END IF;

        -- Equip the new item and deduct the money
        INSERT INTO character_equipment (character_id, equipment_id, slot, category)
        VALUES (character_id_param, equip_id_param, equip_type, equip_category);

        UPDATE characters
        SET money = money - equipment_price
        WHERE id = character_id_param;
    END IF;

    -- Apply stat changes based on equipment category
    IF equip_category = 'weapon' THEN
        UPDATE characters SET strength = strength + new_equipment_stat_bonus - COALESCE(current_equipment_stat_bonus, 0) WHERE id = character_id_param;
    ELSIF equip_category = 'armour' THEN
        UPDATE characters SET hp = LEAST(hp + new_equipment_stat_bonus - COALESCE(current_equipment_stat_bonus, 0), max_hp), 
                              max_hp = max_hp + new_equipment_stat_bonus - COALESCE(current_equipment_stat_bonus, 0) 
        WHERE id = character_id_param;
    ELSIF equip_category = 'helmet' THEN
        UPDATE characters SET mp = LEAST(mp + new_equipment_stat_bonus - COALESCE(current_equipment_stat_bonus, 0), max_mp), 
                              max_mp = max_mp + new_equipment_stat_bonus - COALESCE(current_equipment_stat_bonus, 0) 
        WHERE id = character_id_param;
    ELSIF equip_category = 'boots' THEN
        UPDATE characters SET dexterity = dexterity + new_equipment_stat_bonus - COALESCE(current_equipment_stat_bonus, 0) WHERE id = character_id_param;
    END IF;

END;
$$;


ALTER FUNCTION public.equip_better_item(character_id_param integer, equip_id_param integer) OWNER TO postgres;

--
-- TOC entry 258 (class 1255 OID 33485)
-- Name: generate_and_assign_quest(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.generate_and_assign_quest(character_id_param integer) RETURNS TABLE(generated_quest_id integer, description text)
    LANGUAGE plpgsql
    AS $$DECLARE
    char_level INT;
    boss_name TEXT;
    boss_id INT;
    suitable_equipment_id INT;
    equipment_condition VARCHAR(50);
    chosen_category TEXT;
    categories TEXT[] := ARRAY['weapon', 'armour', 'helmet', 'boots'];
    novice_count INT;
    champion_count INT;
    generated_quest_id INT; 
BEGIN
    -- Fetch the character's level
    SELECT c.level
    INTO char_level
    FROM characters AS c
    WHERE c.id = character_id_param;

    -- Determine the suitable equipment condition based on char_level
    equipment_condition := CASE
        WHEN char_level BETWEEN 1 AND 5 THEN 'Worn'
        WHEN char_level BETWEEN 6 AND 10 THEN 'Classic'
        ELSE 'Polished'
    END;

    -- Randomly choose a category
    chosen_category := categories[(RANDOM() * ARRAY_LENGTH(categories, 1))::INT + 1];

    -- Fetch a random piece of suitable equipment based on the chosen category
    SELECT e.id INTO suitable_equipment_id
    FROM equipment AS e
    WHERE e.condition = equipment_condition AND e.category = chosen_category
    ORDER BY RANDOM()
    LIMIT 1;

    -- If the character level is a multiple of 5, assign a boss quest
    IF char_level % 2 = 0 THEN
        -- Fetch a random boss
        SELECT type, id INTO boss_name, boss_id
		FROM monsters
		WHERE rank = 'Boss'
		ORDER BY RANDOM()
		LIMIT 1;


        -- Insert the boss quest
        INSERT INTO quests(description, reward_money, reward_experience, equipment_reward_id)
        VALUES (CONCAT('Defeat the Boss: ', boss_name), 200, 100, suitable_equipment_id)
        RETURNING id INTO generated_quest_id;

        -- Add the boss to the quest_monsters table
        INSERT INTO quest_monsters(quest_id, monster_id, monster_count)
        VALUES (generated_quest_id, boss_id, 1);

        RETURN QUERY SELECT generated_quest_id, CONCAT('Defeat the Boss: ', boss_name);

    -- If the character level is not a multiple of 5, proceed with the regular quest assignment
    ELSE 
        IF char_level BETWEEN 1 AND 5 THEN
            novice_count := 3;
            champion_count := 0;
        ELSIF char_level BETWEEN 6 AND 10 THEN
            novice_count := 2;
            champion_count := 1;
        ELSE
            novice_count := 0;
            champion_count := 3;
        END IF;

        -- Insert the standard quest
        INSERT INTO quests(description, reward_money, reward_experience, equipment_reward_id)
        VALUES (CONCAT('Defeat ', novice_count, ' Novice monsters, ', champion_count, ' Champion monsters.'), 100, 50, suitable_equipment_id)
        RETURNING id INTO generated_quest_id;

        -- Add distinct novice monsters to the quest_monsters table
        FOR i IN 1..novice_count LOOP
            INSERT INTO quest_monsters(quest_id, monster_id, monster_count)
            SELECT generated_quest_id, id, 1
            FROM monsters
            WHERE rank = 'Novice' 
            ORDER BY RANDOM()
            LIMIT 1;
        END LOOP;

        -- Add distinct champion monsters to the quest_monsters table
        FOR i IN 1..champion_count LOOP
            INSERT INTO quest_monsters(quest_id, monster_id, monster_count)
            SELECT generated_quest_id, id, 1
            FROM monsters
            WHERE rank = 'Champion' 
            ORDER BY RANDOM()
            LIMIT 1;
        END LOOP;

        RETURN QUERY SELECT generated_quest_id, CONCAT('Defeat ', novice_count, ' Novice monsters, ', champion_count, ' Champion monsters.');
    END IF;

    -- Assign the new quest to the character
    INSERT INTO character_quests(character_id, quest_id, status, level_when_assigned)
    VALUES(character_id_param, generated_quest_id, 'assigned', char_level);

    -- Check if the previous quest was incomplete, and if so, set it to 'failed'
    WITH previous_quest AS (
        SELECT cq.id
        FROM character_quests AS cq 
        WHERE cq.character_id = character_id_param AND cq.status = 'assigned'
        ORDER BY level_when_assigned DESC
        LIMIT 1 OFFSET 1
    )
    UPDATE character_quests AS cq
    SET status = 'failed'
    FROM previous_quest 
    WHERE cq.id = previous_quest.id 
    AND NOT EXISTS (
        SELECT 1 
        FROM character_quests AS completed_cq 
        WHERE completed_cq.character_id = character_id_param 
        AND completed_cq.status = 'complete'
        AND completed_cq.level_when_assigned > cq.level_when_assigned
    );

END;$$;


ALTER FUNCTION public.generate_and_assign_quest(character_id_param integer) OWNER TO postgres;

--
-- TOC entry 255 (class 1255 OID 33492)
-- Name: penalty_for_failure(integer); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.penalty_for_failure(character_id_param integer) RETURNS TABLE(penalty_exp integer, penalty_money numeric)
    LANGUAGE plpgsql
    AS $$DECLARE
    char_exp INT;
    char_money DECIMAL(10,2);
    base_exp_penalty INT;
    base_money_penalty DECIMAL(10,2);
BEGIN
    -- Fetch the current experience and money of the character
    SELECT experience, money INTO char_exp, char_money
    FROM characters 
    WHERE id = character_id_param;

    -- Calculate experience penalty as 50% of the character's current experience
    base_exp_penalty := (char_exp * 0.5)::INT;

    -- Calculate money penalty as 10% of the character's current money
    base_money_penalty := char_money * 0.1; 

    -- Update the character's experience and money with the penalties
    UPDATE characters
    SET experience = char_exp - base_exp_penalty,
        money = char_money - base_money_penalty
    WHERE id = character_id_param;

    -- Call the check and level down function, if the character's experience drops
    PERFORM check_and_level_down(character_id_param);

    RETURN QUERY SELECT base_exp_penalty as penalty_exp, base_money_penalty as penalty_money;
END;
$$;


ALTER FUNCTION public.penalty_for_failure(character_id_param integer) OWNER TO postgres;

--
-- TOC entry 262 (class 1255 OID 42168)
-- Name: populate_characters(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.populate_characters() RETURNS void
    LANGUAGE plpgsql
    AS $$
BEGIN
    INSERT INTO characters (id, name, class, level, experience, hp, mp, strength, dexterity, money, max_hp, max_mp)
    VALUES 
        (15, 'Orion', 'Gold Knight', 1, 0, 15, 10, 10, 10, 10, 500, 15),
        (16, 'Fait', 'Speedster', 1, 0, 10, 10, 10, 15, 500, 10, 10),
        (17, 'Thea', 'Mage', 1, 0, 10, 15, 10, 10, 500, 10, 15),
        (14, 'Xull', 'Org', 1, 50, 22, 11, 20, 10, 759, 22, 11);
END;
$$;


ALTER FUNCTION public.populate_characters() OWNER TO postgres;

--
-- TOC entry 261 (class 1255 OID 42167)
-- Name: populate_equipment(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.populate_equipment() RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    weapons text[] := ARRAY['Sword', 'Hammer', 'Spear', 'Blaster', 'Canon',
                            'Lance','Katar','Bow','Gauntlets','Boots'];
    variant text;
    condition text;
    statBonus integer;
BEGIN
    FOREACH variant IN ARRAY ARRAY['Val', 'Bodvar', 'Koji','Xull','Ulgrim',
                                   'Orion','Kaya','Lord Vraxx','Lucien',
                             'Ada' ] 
    LOOP
        -- Determine condition and stat bonus
        IF RANDOM() < 0.33 THEN 
            condition := 'Worn';
            statBonus := (RANDOM() * 5)::INTEGER; -- Random bonus between 0 and 5
        ELSIF RANDOM() < 0.66 THEN 
            condition := 'Classic';
            statBonus := (RANDOM() * 5 + 5)::INTEGER; -- Random bonus between 5 and 10
        ELSE 
            condition := 'Polished';
            statBonus := (RANDOM() * 5 + 10)::INTEGER; -- Random bonus between 10 and 15
        END IF;
        
        -- Insert values
        INSERT INTO equipment (type, material, condition, price, stat_bonus)
        SELECT weapon, variant, condition,
               (RANDOM() * 20 + 10)::INTEGER,  -- Random price between 10 and 30
               statBonus
        FROM unnest(weapons) AS weapon;
    END LOOP;
END;
$$;


ALTER FUNCTION public.populate_equipment() OWNER TO postgres;

--
-- TOC entry 260 (class 1255 OID 42166)
-- Name: populate_monsters(); Type: FUNCTION; Schema: public; Owner: postgres
--

CREATE FUNCTION public.populate_monsters() RETURNS void
    LANGUAGE plpgsql
    AS $$
DECLARE
    fighters text[] := ARRAY['Val', 'Bodvar', 'Koji', 'Diana', 'Wu Shang',
							 'Xull','Ulgrim','Orion','Kaya','Lord Vraxx','Lucien',
							 'Ada','Asuri','Azoth','Barraza','Brynn','Scarlet','Sir Roland',
							'Teros','Ember','Thatch','Hattori','Gnash','Queen Nai','Artemis']; 
    rank text;
BEGIN
    FOREACH rank IN ARRAY ARRAY['Novice', 'Champion', 'Boss']
    LOOP
        INSERT INTO monsters (type, rank, base_hp, base_strength) 
        SELECT 
            fighter, 
            rank, 
            CASE
                WHEN rank = 'Boss' THEN (RANDOM() * 5 + 50)::INTEGER  -- Random HP between 50 and 55 for Boss
                ELSE (RANDOM() * 10 + 10)::INTEGER  -- Random HP between 10 and 20 for others
            END,
            CASE
                WHEN rank = 'Boss' THEN (RANDOM() * 5 + 15)::INTEGER  -- Random strength between 15 and 20 for Boss
                ELSE (RANDOM() * 5 + 5)::INTEGER  -- Random strength between 5 and 10 for others
            END
        FROM unnest(fighters) AS fighter;
    END LOOP;
END;
$$;


ALTER FUNCTION public.populate_monsters() OWNER TO postgres;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- TOC entry 238 (class 1259 OID 33474)
-- Name: avg_monster_multiplier; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.avg_monster_multiplier (
    avg numeric
);


ALTER TABLE public.avg_monster_multiplier OWNER TO postgres;

--
-- TOC entry 227 (class 1259 OID 33290)
-- Name: character_equipment; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.character_equipment (
    id integer NOT NULL,
    character_id integer,
    equipment_id integer,
    slot character varying(50) NOT NULL,
    category character varying(50)
);


ALTER TABLE public.character_equipment OWNER TO postgres;

--
-- TOC entry 226 (class 1259 OID 33289)
-- Name: character_equipment_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.character_equipment_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.character_equipment_id_seq OWNER TO postgres;

--
-- TOC entry 3448 (class 0 OID 0)
-- Dependencies: 226
-- Name: character_equipment_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.character_equipment_id_seq OWNED BY public.character_equipment.id;


--
-- TOC entry 229 (class 1259 OID 33307)
-- Name: character_quests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.character_quests (
    id integer NOT NULL,
    character_id integer,
    quest_id integer,
    status character varying(50) DEFAULT 'assigned'::character varying,
    level_when_assigned integer
);


ALTER TABLE public.character_quests OWNER TO postgres;

--
-- TOC entry 228 (class 1259 OID 33306)
-- Name: character_quests_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.character_quests_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.character_quests_id_seq OWNER TO postgres;

--
-- TOC entry 3449 (class 0 OID 0)
-- Dependencies: 228
-- Name: character_quests_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.character_quests_id_seq OWNED BY public.character_quests.id;


--
-- TOC entry 233 (class 1259 OID 33334)
-- Name: character_spells; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.character_spells (
    id integer NOT NULL,
    character_id integer,
    spell_id integer,
    acquired_at timestamp without time zone DEFAULT now(),
    is_active boolean DEFAULT true
);


ALTER TABLE public.character_spells OWNER TO postgres;

--
-- TOC entry 232 (class 1259 OID 33333)
-- Name: character_spells_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.character_spells_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.character_spells_id_seq OWNER TO postgres;

--
-- TOC entry 3450 (class 0 OID 0)
-- Dependencies: 232
-- Name: character_spells_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.character_spells_id_seq OWNED BY public.character_spells.id;


--
-- TOC entry 219 (class 1259 OID 33256)
-- Name: characters; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.characters (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    class character varying(50),
    level integer DEFAULT 1,
    experience integer DEFAULT 0,
    hp integer,
    mp integer,
    strength integer,
    dexterity integer,
    money integer DEFAULT 0,
    max_hp integer,
    max_mp integer DEFAULT 10,
    CONSTRAINT check_positive_mp CHECK ((mp >= 0))
);


ALTER TABLE public.characters OWNER TO postgres;

--
-- TOC entry 218 (class 1259 OID 33255)
-- Name: characters_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.characters_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.characters_id_seq OWNER TO postgres;

--
-- TOC entry 3451 (class 0 OID 0)
-- Dependencies: 218
-- Name: characters_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.characters_id_seq OWNED BY public.characters.id;


--
-- TOC entry 221 (class 1259 OID 33266)
-- Name: equipment; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.equipment (
    id integer NOT NULL,
    type character varying(50) NOT NULL,
    material character varying(50),
    condition character varying(50),
    stat_bonus integer DEFAULT 0,
    price integer NOT NULL,
    category character varying(50)
);


ALTER TABLE public.equipment OWNER TO postgres;

--
-- TOC entry 220 (class 1259 OID 33265)
-- Name: equipment_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.equipment_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.equipment_id_seq OWNER TO postgres;

--
-- TOC entry 3452 (class 0 OID 0)
-- Dependencies: 220
-- Name: equipment_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.equipment_id_seq OWNED BY public.equipment.id;


--
-- TOC entry 236 (class 1259 OID 33434)
-- Name: heal_spell_id; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.heal_spell_id (
    id integer
);


ALTER TABLE public.heal_spell_id OWNER TO postgres;

--
-- TOC entry 237 (class 1259 OID 33469)
-- Name: monster_avg_rank; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.monster_avg_rank (
    avg numeric
);


ALTER TABLE public.monster_avg_rank OWNER TO postgres;

--
-- TOC entry 223 (class 1259 OID 33274)
-- Name: monsters; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.monsters (
    id integer NOT NULL,
    type character varying(50) NOT NULL,
    rank character varying(50),
    base_hp integer,
    base_strength integer
);


ALTER TABLE public.monsters OWNER TO postgres;

--
-- TOC entry 222 (class 1259 OID 33273)
-- Name: monsters_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.monsters_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.monsters_id_seq OWNER TO postgres;

--
-- TOC entry 3453 (class 0 OID 0)
-- Dependencies: 222
-- Name: monsters_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.monsters_id_seq OWNED BY public.monsters.id;


--
-- TOC entry 235 (class 1259 OID 33406)
-- Name: quest_monsters; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.quest_monsters (
    id integer NOT NULL,
    quest_id integer,
    monster_id integer,
    monster_count integer DEFAULT 1
);


ALTER TABLE public.quest_monsters OWNER TO postgres;

--
-- TOC entry 234 (class 1259 OID 33405)
-- Name: quest_monsters_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.quest_monsters_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.quest_monsters_id_seq OWNER TO postgres;

--
-- TOC entry 3454 (class 0 OID 0)
-- Dependencies: 234
-- Name: quest_monsters_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.quest_monsters_id_seq OWNED BY public.quest_monsters.id;


--
-- TOC entry 239 (class 1259 OID 41971)
-- Name: quest_temp_monsters; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.quest_temp_monsters (
    id integer,
    quest_id integer,
    monster_id integer,
    monster_count integer
);


ALTER TABLE public.quest_temp_monsters OWNER TO postgres;

--
-- TOC entry 225 (class 1259 OID 33281)
-- Name: quests; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.quests (
    id integer NOT NULL,
    description character varying(255) NOT NULL,
    reward_money integer DEFAULT 0,
    reward_experience integer DEFAULT 0,
    equipment_reward_id integer
);


ALTER TABLE public.quests OWNER TO postgres;

--
-- TOC entry 224 (class 1259 OID 33280)
-- Name: quests_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.quests_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.quests_id_seq OWNER TO postgres;

--
-- TOC entry 3455 (class 0 OID 0)
-- Dependencies: 224
-- Name: quests_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.quests_id_seq OWNED BY public.quests.id;


--
-- TOC entry 231 (class 1259 OID 33325)
-- Name: spells; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public.spells (
    id integer NOT NULL,
    name character varying(255) NOT NULL,
    description character varying(500),
    effect_type character varying(50),
    effect_value integer,
    mana_cost integer
);


ALTER TABLE public.spells OWNER TO postgres;

--
-- TOC entry 230 (class 1259 OID 33324)
-- Name: spells_id_seq; Type: SEQUENCE; Schema: public; Owner: postgres
--

CREATE SEQUENCE public.spells_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;


ALTER TABLE public.spells_id_seq OWNER TO postgres;

--
-- TOC entry 3456 (class 0 OID 0)
-- Dependencies: 230
-- Name: spells_id_seq; Type: SEQUENCE OWNED BY; Schema: public; Owner: postgres
--

ALTER SEQUENCE public.spells_id_seq OWNED BY public.spells.id;


--
-- TOC entry 3256 (class 2604 OID 33293)
-- Name: character_equipment id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_equipment ALTER COLUMN id SET DEFAULT nextval('public.character_equipment_id_seq'::regclass);


--
-- TOC entry 3257 (class 2604 OID 33310)
-- Name: character_quests id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_quests ALTER COLUMN id SET DEFAULT nextval('public.character_quests_id_seq'::regclass);


--
-- TOC entry 3260 (class 2604 OID 33337)
-- Name: character_spells id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_spells ALTER COLUMN id SET DEFAULT nextval('public.character_spells_id_seq'::regclass);


--
-- TOC entry 3245 (class 2604 OID 33259)
-- Name: characters id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.characters ALTER COLUMN id SET DEFAULT nextval('public.characters_id_seq'::regclass);


--
-- TOC entry 3250 (class 2604 OID 33269)
-- Name: equipment id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.equipment ALTER COLUMN id SET DEFAULT nextval('public.equipment_id_seq'::regclass);


--
-- TOC entry 3252 (class 2604 OID 33277)
-- Name: monsters id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.monsters ALTER COLUMN id SET DEFAULT nextval('public.monsters_id_seq'::regclass);


--
-- TOC entry 3263 (class 2604 OID 33409)
-- Name: quest_monsters id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.quest_monsters ALTER COLUMN id SET DEFAULT nextval('public.quest_monsters_id_seq'::regclass);


--
-- TOC entry 3253 (class 2604 OID 33284)
-- Name: quests id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.quests ALTER COLUMN id SET DEFAULT nextval('public.quests_id_seq'::regclass);


--
-- TOC entry 3259 (class 2604 OID 33328)
-- Name: spells id; Type: DEFAULT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.spells ALTER COLUMN id SET DEFAULT nextval('public.spells_id_seq'::regclass);


--
-- TOC entry 3277 (class 2606 OID 33295)
-- Name: character_equipment character_equipment_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_equipment
    ADD CONSTRAINT character_equipment_pkey PRIMARY KEY (id);


--
-- TOC entry 3279 (class 2606 OID 33313)
-- Name: character_quests character_quests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_quests
    ADD CONSTRAINT character_quests_pkey PRIMARY KEY (id);


--
-- TOC entry 3284 (class 2606 OID 33428)
-- Name: character_spells character_spell_unique; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_spells
    ADD CONSTRAINT character_spell_unique UNIQUE (character_id, spell_id);


--
-- TOC entry 3286 (class 2606 OID 33339)
-- Name: character_spells character_spells_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_spells
    ADD CONSTRAINT character_spells_pkey PRIMARY KEY (id);


--
-- TOC entry 3267 (class 2606 OID 33264)
-- Name: characters characters_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.characters
    ADD CONSTRAINT characters_pkey PRIMARY KEY (id);


--
-- TOC entry 3271 (class 2606 OID 33272)
-- Name: equipment equipment_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.equipment
    ADD CONSTRAINT equipment_pkey PRIMARY KEY (id);


--
-- TOC entry 3273 (class 2606 OID 33279)
-- Name: monsters monsters_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.monsters
    ADD CONSTRAINT monsters_pkey PRIMARY KEY (id);


--
-- TOC entry 3288 (class 2606 OID 33411)
-- Name: quest_monsters quest_monsters_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.quest_monsters
    ADD CONSTRAINT quest_monsters_pkey PRIMARY KEY (id);


--
-- TOC entry 3275 (class 2606 OID 33288)
-- Name: quests quests_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.quests
    ADD CONSTRAINT quests_pkey PRIMARY KEY (id);


--
-- TOC entry 3282 (class 2606 OID 33332)
-- Name: spells spells_pkey; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.spells
    ADD CONSTRAINT spells_pkey PRIMARY KEY (id);


--
-- TOC entry 3269 (class 2606 OID 33351)
-- Name: characters unique_character_name; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.characters
    ADD CONSTRAINT unique_character_name UNIQUE (name);


--
-- TOC entry 3280 (class 1259 OID 33447)
-- Name: idx_unique_active_character_quest; Type: INDEX; Schema: public; Owner: postgres
--

CREATE UNIQUE INDEX idx_unique_active_character_quest ON public.character_quests USING btree (character_id) WHERE ((status)::text = 'assigned'::text);


--
-- TOC entry 3290 (class 2606 OID 33296)
-- Name: character_equipment character_equipment_character_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_equipment
    ADD CONSTRAINT character_equipment_character_id_fkey FOREIGN KEY (character_id) REFERENCES public.characters(id);


--
-- TOC entry 3291 (class 2606 OID 33301)
-- Name: character_equipment character_equipment_equipment_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_equipment
    ADD CONSTRAINT character_equipment_equipment_id_fkey FOREIGN KEY (equipment_id) REFERENCES public.equipment(id);


--
-- TOC entry 3293 (class 2606 OID 33314)
-- Name: character_quests character_quests_character_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_quests
    ADD CONSTRAINT character_quests_character_id_fkey FOREIGN KEY (character_id) REFERENCES public.characters(id);


--
-- TOC entry 3294 (class 2606 OID 33319)
-- Name: character_quests character_quests_quest_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_quests
    ADD CONSTRAINT character_quests_quest_id_fkey FOREIGN KEY (quest_id) REFERENCES public.quests(id);


--
-- TOC entry 3296 (class 2606 OID 33340)
-- Name: character_spells character_spells_character_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_spells
    ADD CONSTRAINT character_spells_character_id_fkey FOREIGN KEY (character_id) REFERENCES public.characters(id);


--
-- TOC entry 3297 (class 2606 OID 33345)
-- Name: character_spells character_spells_spell_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_spells
    ADD CONSTRAINT character_spells_spell_id_fkey FOREIGN KEY (spell_id) REFERENCES public.spells(id);


--
-- TOC entry 3292 (class 2606 OID 33354)
-- Name: character_equipment fk_character_equipment; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_equipment
    ADD CONSTRAINT fk_character_equipment FOREIGN KEY (character_id) REFERENCES public.characters(id) ON DELETE CASCADE;


--
-- TOC entry 3295 (class 2606 OID 33359)
-- Name: character_quests fk_character_quests; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_quests
    ADD CONSTRAINT fk_character_quests FOREIGN KEY (character_id) REFERENCES public.characters(id) ON DELETE CASCADE;


--
-- TOC entry 3298 (class 2606 OID 33364)
-- Name: character_spells fk_character_spells; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.character_spells
    ADD CONSTRAINT fk_character_spells FOREIGN KEY (character_id) REFERENCES public.characters(id) ON DELETE CASCADE;


--
-- TOC entry 3299 (class 2606 OID 33419)
-- Name: quest_monsters quest_monsters_monster_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.quest_monsters
    ADD CONSTRAINT quest_monsters_monster_id_fkey FOREIGN KEY (monster_id) REFERENCES public.monsters(id);


--
-- TOC entry 3300 (class 2606 OID 33414)
-- Name: quest_monsters quest_monsters_quest_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.quest_monsters
    ADD CONSTRAINT quest_monsters_quest_id_fkey FOREIGN KEY (quest_id) REFERENCES public.quests(id);


--
-- TOC entry 3289 (class 2606 OID 33440)
-- Name: quests quests_equipment_reward_id_fkey; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public.quests
    ADD CONSTRAINT quests_equipment_reward_id_fkey FOREIGN KEY (equipment_reward_id) REFERENCES public.equipment(id);


-- Completed on 2023-10-19 15:43:03

--
-- PostgreSQL database dump complete
--

