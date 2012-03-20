DROP TABLE IF EXISTS summoner CASCADE;

CREATE TABLE summoner (
	id int AUTO_INCREMENT PRIMARY KEY,
	
	region ENUM('north_america', 'europe_west', 'europe_nordic_east') not null,
	
	-- Cannot be made unique because of the data from multiple regions being stored in the same table.
	account_id integer not null,
	summoner_id integer not null,

	summoner_name text not null,
	internal_name text not null,

	summoner_level integer not null,
	profile_icon integer not null,

	update_automatically bit not null,

	-- The time the profile was originally created (i.e. time of the first lookup), UTC.
	time_created timestamp not null,

	-- The last time the profile was last updated, UTC.
	time_updated timestamp not null
) Engine=InnoDB;

-- For lookups by account ID
CREATE INDEX summoner_account_id_index ON summoner (region, account_id);

-- For lookups by name (case-insensitive)
CREATE INDEX summoner_summoner_name_index ON summoner (region, summoner_name(50));

-- For the automatic updates
CREATE INDEX summoner_update_automatically_index ON summoner (update_automatically);

DROP TABLE IF EXISTS summoner_rating CASCADE;

CREATE TABLE summoner_rating
(
	summoner_id integer not null,

	rating_map ENUM('twisted_treeline', 'summoners_rift', 'dominion') not null,
	game_mode ENUM('custom', 'normal', 'bot', 'solo', 'premade') not null,

	wins integer not null,
	losses integer not null,
	leaves integer not null,

	-- May be null for normals
	current_rating integer,
	-- top rating for unranked Summoner's Rift is estimated from all the values recorded, may be null for normals
	top_rating integer,
	
	INDEX (summoner_id),
	FOREIGN KEY (summoner_id) REFERENCES summoner(id)
) Engine=InnoDB;

CREATE INDEX summoner_rating_summoner_id_index ON summoner_rating (summoner_id);

-- Required for updating irregular Elos below 1200 and also those in Summoner's Rift
CREATE INDEX summoner_rating_update_index ON summoner_rating (summoner_id, rating_map, game_mode);

DROP TABLE IF EXISTS summoner_ranked_statistics CASCADE;

-- This table holds the performance of a summoner with a particular champion in their ranked games.
-- It is obtained from the ranked stats in their profile.
CREATE TABLE summoner_ranked_statistics
(
	summoner_id integer not null,

	champion_id integer not null,

	wins integer not null,
	losses integer not null,

	kills integer not null,
	deaths integer not null,
	assists integer not null,

	minion_kills integer not null,

	gold integer not null,

	turrets_destroyed integer not null,

	damage_dealt integer not null,
	physical_damage_dealt integer not null,
	magical_damage_dealt integer not null,

	damage_taken integer not null,

	double_kills integer not null,
	triple_kills integer not null,
	quadra_kills integer not null,
	penta_kills integer not null,

	time_spent_dead integer not null,

	maximum_kills integer not null,
	maximum_deaths integer not null,
	
	INDEX (summoner_id),
	FOREIGN KEY (summoner_id) REFERENCES summoner(id)
) Engine=InnoDB;

CREATE INDEX summoner_ranked_statistics_summoner_id_index ON summoner_ranked_statistics (summoner_id);

DROP TABLE IF EXISTS team CASCADE;

CREATE TABLE team
(
	id int AUTO_INCREMENT PRIMARY KEY,
	-- blue vs. purple, 100 is blue, 200 is purple
	is_blue_team bit not null
) Engine=InnoDB;

DROP TABLE IF EXISTS missing_team_player CASCADE;

-- This table holds superficial data about players who participated in a game but have not been loaded yet as this is very expensive
CREATE TABLE missing_team_player
(
	team_id integer not null,
	champion_id integer not null,
	account_id integer not null,
	
	INDEX (team_id),
	FOREIGN KEY (team_id) REFERENCES team(id)
) Engine=InnoDB;

CREATE INDEX missing_team_player_index ON missing_team_player (team_id, account_id);

DROP TABLE IF EXISTS game_result CASCADE;

CREATE TABLE game_result
(
	id int AUTO_INCREMENT PRIMARY KEY,

	-- Cannot be made unique because of the data from multiple regions being stored in the same table
	game_id integer not null,

	result_map ENUM('twisted_treeline', 'summoners_rift', 'dominion') not null,
	game_mode ENUM('custom', 'normal', 'bot', 'solo', 'premade') not null,

	-- This is when the game was created.
	game_time timestamp not null,

	team1_won boolean not null,

	team1_id integer not null,
	team2_id integer not null,
	
	INDEX(team1_id),
	FOREIGN KEY (team1_id) REFERENCES team(id),
	INDEX(team2_id),
	FOREIGN KEY (team2_id) REFERENCES team(id)
) Engine=InnoDB;

CREATE INDEX game_result_game_time_index ON game_result (game_time desc);
CREATE INDEX game_result_team1_id_index ON game_result (team1_id);
CREATE INDEX game_result_team2_id_index ON game_result (team2_id);
CREATE INDEX game_result_map_mode_index ON game_result (result_map, game_mode);

DROP TABLE IF EXISTS team_player CASCADE;

-- This table holds the results for one player in a game retrieved from the recent match history.
CREATE TABLE team_player
(
	game_id integer not null,
	team_id integer not null,
	summoner_id integer not null,

	won boolean not null,

	ping integer not null,
	time_spent_in_queue integer not null,

	premade_size integer not null,

	-- This is an argument used in the Elo formula
	k_coefficient integer not null,
	probability_of_winning double precision not null,

	-- Elo may be left undefined as it is not available in custom games
	rating integer,
	rating_change integer,
	-- I'm still not entirely sure what this one means
	adjusted_rating integer,
	team_rating integer,

	experience_earned integer not null,
	boosted_experience_earned integer not null,

	ip_earned integer not null,
	boosted_ip_earned integer not null,

	summoner_level integer not null,

	summoner_spell1 integer not null,
	summoner_spell2 integer not null,

	champion_id integer not null,

	-- can be NULL, apparently
	skin_name text,
	skin_index integer not null,

	champion_level integer not null,

	items blob not null,

	kills integer not null,
	deaths integer not null,
	assists integer not null,

	minion_kills integer not null,

	gold integer not null,

	damage_dealt integer not null,
	physical_damage_dealt integer not null,
	magical_damage_dealt integer not null,

	damage_taken integer not null,
	physical_damage_taken integer not null,
	magical_damage_taken integer not null,

	total_healing_done integer not null,

	time_spent_dead integer not null,

	largest_multikill integer not null,
	largest_killing_spree integer not null,
	largest_critical_strike integer not null,

	-- Summoner's Rift/Twisted Treeline

	neutral_minions_killed integer,

	turrets_destroyed integer,
	inhibitors_destroyed integer,

	-- Dominion

	nodes_neutralised integer,
	node_neutralisation_assists integer,
	nodes_captured integer,

	victory_points integer,
	objectives integer,

	total_score integer,
	objective_score integer,
	combat_score integer,

	rank integer,
	
	INDEX(game_id),
	FOREIGN KEY (game_id) REFERENCES game_result(id),
	INDEX(team_id),
	FOREIGN KEY (team_id) REFERENCES team(id),
	INDEX(summoner_id),
	FOREIGN KEY (summoner_id) REFERENCES summoner(id)
);

CREATE INDEX team_player_game_id_index ON team_player (game_id);
CREATE INDEX team_player_team_id_index ON team_player (team_id);
CREATE INDEX team_player_summoner_id_index ON team_player (summoner_id);

-- No explicit indices are provided for the following two tables as they are just loaded once when the application starts
-- After that, the application performs the translation itself because it's probably faster and a very common operation

DROP TABLE IF EXISTS champion_name CASCADE;

CREATE TABLE champion_name
(
	champion_id integer not null PRIMARY KEY,
	champion_name text not null
);

DROP TABLE IF EXISTS item_information CASCADE;

CREATE TABLE item_information
(
	item_id integer not null PRIMARY KEY,
	item_name text not null,
	description text not null
);