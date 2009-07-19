/*
MySQL Data Transfer
Source Host: localhost
Source Database: demogame
Target Host: localhost
Target Database: demogame
Date: 7/19/2009 11:52:17 AM
*/

SET FOREIGN_KEY_CHECKS=0;
-- ----------------------------
-- Table structure for alliance
-- ----------------------------
CREATE TABLE `alliance` (
  `id` tinyint(3) unsigned NOT NULL,
  `name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for alliance_attackable
-- ----------------------------
CREATE TABLE `alliance_attackable` (
  `alliance_id` tinyint(3) unsigned NOT NULL,
  `attackable_id` tinyint(3) unsigned NOT NULL,
  KEY `attackable_id` (`attackable_id`),
  KEY `alliance_id` (`alliance_id`),
  CONSTRAINT `alliance_attackable_ibfk_3` FOREIGN KEY (`attackable_id`) REFERENCES `alliance` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `alliance_attackable_ibfk_4` FOREIGN KEY (`alliance_id`) REFERENCES `alliance` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for alliance_hostile
-- ----------------------------
CREATE TABLE `alliance_hostile` (
  `alliance_id` tinyint(3) unsigned NOT NULL,
  `hostile_id` tinyint(3) unsigned NOT NULL,
  KEY `hostile_id` (`hostile_id`),
  KEY `alliance_id` (`alliance_id`),
  CONSTRAINT `alliance_hostile_ibfk_3` FOREIGN KEY (`hostile_id`) REFERENCES `alliance` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `alliance_hostile_ibfk_4` FOREIGN KEY (`alliance_id`) REFERENCES `alliance` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for character
-- ----------------------------
CREATE TABLE `character` (
  `id` int(10) unsigned NOT NULL,
  `template_id` smallint(5) unsigned DEFAULT NULL,
  `name` varchar(50) NOT NULL,
  `password` varchar(50) NOT NULL,
  `map` smallint(5) unsigned NOT NULL DEFAULT '1',
  `x` float NOT NULL DEFAULT '100',
  `y` float NOT NULL DEFAULT '100',
  `respawn_map` smallint(5) unsigned NOT NULL,
  `respawn_x` float NOT NULL,
  `respawn_y` float NOT NULL,
  `body` smallint(5) unsigned NOT NULL DEFAULT '1',
  `cash` int(10) unsigned NOT NULL DEFAULT '0',
  `level` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `exp` int(10) unsigned NOT NULL DEFAULT '0',
  `statpoints` int(10) unsigned NOT NULL DEFAULT '0',
  `hp` smallint(5) unsigned NOT NULL DEFAULT '50',
  `mp` smallint(5) unsigned NOT NULL DEFAULT '50',
  `maxhp` smallint(5) unsigned NOT NULL DEFAULT '50',
  `maxmp` smallint(5) unsigned NOT NULL DEFAULT '50',
  `minhit` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `maxhit` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `acc` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `agi` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `armor` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `bra` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `defence` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `dex` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `evade` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `imm` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `int` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `perc` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `recov` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `regen` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `str` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `tact` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `ws` tinyint(3) unsigned NOT NULL,
  PRIMARY KEY (`id`,`respawn_map`),
  UNIQUE KEY `idx_name` (`name`),
  KEY `template_id` (`template_id`),
  CONSTRAINT `character_ibfk_1` FOREIGN KEY (`template_id`) REFERENCES `character_template` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for character_equipped
-- ----------------------------
CREATE TABLE `character_equipped` (
  `character_id` int(10) unsigned NOT NULL,
  `item_id` int(10) unsigned NOT NULL,
  `slot` tinyint(3) unsigned NOT NULL,
  KEY `item_id` (`item_id`),
  KEY `character_id` (`character_id`),
  CONSTRAINT `character_equipped_ibfk_3` FOREIGN KEY (`item_id`) REFERENCES `item` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `character_equipped_ibfk_4` FOREIGN KEY (`character_id`) REFERENCES `character` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for character_inventory
-- ----------------------------
CREATE TABLE `character_inventory` (
  `character_id` int(10) unsigned NOT NULL,
  `item_id` int(10) unsigned NOT NULL,
  KEY `item_id` (`item_id`),
  KEY `character_id` (`character_id`),
  CONSTRAINT `character_inventory_ibfk_3` FOREIGN KEY (`item_id`) REFERENCES `item` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `character_inventory_ibfk_4` FOREIGN KEY (`character_id`) REFERENCES `character` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for character_template
-- ----------------------------
CREATE TABLE `character_template` (
  `id` smallint(5) unsigned NOT NULL,
  `alliance_id` tinyint(3) unsigned NOT NULL,
  `name` varchar(50) NOT NULL DEFAULT 'New NPC',
  `ai` varchar(255) NOT NULL,
  `body` smallint(5) unsigned NOT NULL DEFAULT '1',
  `respawn` smallint(5) unsigned NOT NULL DEFAULT '5',
  `level` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `exp` int(10) unsigned NOT NULL,
  `statpoints` int(10) unsigned NOT NULL,
  `give_exp` smallint(5) unsigned NOT NULL DEFAULT '0',
  `give_cash` smallint(5) unsigned NOT NULL DEFAULT '0',
  `maxhp` smallint(5) unsigned NOT NULL DEFAULT '50',
  `maxmp` smallint(5) unsigned NOT NULL DEFAULT '50',
  `defence` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `minhit` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `maxhit` tinyint(3) unsigned NOT NULL DEFAULT '2',
  `acc` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `agi` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `armor` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `bra` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `dex` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `evade` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `imm` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `int` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `perc` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `recov` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `regen` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `str` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `tact` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `ws` tinyint(3) unsigned NOT NULL DEFAULT '1',
  PRIMARY KEY (`id`),
  KEY `alliance_id` (`alliance_id`),
  CONSTRAINT `character_template_ibfk_2` FOREIGN KEY (`alliance_id`) REFERENCES `alliance` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for character_template_equipped
-- ----------------------------
CREATE TABLE `character_template_equipped` (
  `character_id` smallint(5) unsigned NOT NULL,
  `item_id` smallint(5) unsigned NOT NULL,
  `chance` smallint(5) unsigned NOT NULL,
  KEY `item_id` (`item_id`),
  KEY `character_id` (`character_id`),
  CONSTRAINT `character_template_equipped_ibfk_3` FOREIGN KEY (`item_id`) REFERENCES `item_template` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `character_template_equipped_ibfk_4` FOREIGN KEY (`character_id`) REFERENCES `character_template` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for character_template_inventory
-- ----------------------------
CREATE TABLE `character_template_inventory` (
  `character_id` smallint(5) unsigned NOT NULL,
  `item_id` smallint(5) unsigned NOT NULL,
  `min` tinyint(3) unsigned NOT NULL,
  `max` tinyint(3) unsigned NOT NULL,
  `chance` smallint(5) unsigned NOT NULL,
  KEY `item_id` (`item_id`),
  KEY `character_id` (`character_id`),
  CONSTRAINT `character_template_inventory_ibfk_1` FOREIGN KEY (`character_id`) REFERENCES `character_template` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `character_template_inventory_ibfk_2` FOREIGN KEY (`item_id`) REFERENCES `item_template` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for item
-- ----------------------------
CREATE TABLE `item` (
  `id` int(10) unsigned NOT NULL,
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `width` tinyint(3) unsigned NOT NULL DEFAULT '16',
  `height` tinyint(3) unsigned NOT NULL DEFAULT '16',
  `name` varchar(255) NOT NULL,
  `description` varchar(255) NOT NULL,
  `amount` tinyint(3) unsigned NOT NULL DEFAULT '1',
  `graphic` smallint(5) unsigned NOT NULL DEFAULT '0',
  `value` int(11) NOT NULL DEFAULT '0',
  `agi` smallint(5) unsigned NOT NULL DEFAULT '0',
  `armor` smallint(5) unsigned NOT NULL DEFAULT '0',
  `bra` smallint(5) unsigned NOT NULL DEFAULT '0',
  `defence` smallint(5) unsigned NOT NULL DEFAULT '0',
  `dex` smallint(5) unsigned NOT NULL DEFAULT '0',
  `evade` smallint(5) unsigned NOT NULL DEFAULT '0',
  `hp` smallint(5) unsigned NOT NULL DEFAULT '0',
  `imm` smallint(5) unsigned NOT NULL DEFAULT '0',
  `int` smallint(5) unsigned NOT NULL DEFAULT '0',
  `maxhit` smallint(5) unsigned NOT NULL DEFAULT '0',
  `maxhp` smallint(5) unsigned NOT NULL DEFAULT '0',
  `maxmp` smallint(5) unsigned NOT NULL DEFAULT '0',
  `minhit` smallint(5) unsigned NOT NULL DEFAULT '0',
  `mp` smallint(5) unsigned NOT NULL DEFAULT '0',
  `perc` smallint(5) unsigned NOT NULL DEFAULT '0',
  `reqacc` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqagi` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqarmor` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqbra` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqdex` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqevade` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqimm` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqint` tinyint(3) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Table structure for item_template
-- ----------------------------
CREATE TABLE `item_template` (
  `id` smallint(5) unsigned NOT NULL,
  `type` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `width` tinyint(3) unsigned NOT NULL,
  `height` tinyint(3) unsigned NOT NULL,
  `name` varchar(255) NOT NULL,
  `description` varchar(255) NOT NULL,
  `graphic` smallint(5) unsigned NOT NULL,
  `value` int(11) NOT NULL DEFAULT '0',
  `agi` smallint(5) unsigned NOT NULL DEFAULT '0',
  `armor` smallint(5) unsigned NOT NULL DEFAULT '0',
  `bra` smallint(5) unsigned NOT NULL DEFAULT '0',
  `defence` smallint(5) unsigned NOT NULL DEFAULT '0',
  `dex` smallint(5) unsigned NOT NULL DEFAULT '0',
  `evade` smallint(5) unsigned NOT NULL DEFAULT '0',
  `imm` smallint(5) unsigned NOT NULL DEFAULT '0',
  `int` smallint(5) unsigned NOT NULL DEFAULT '0',
  `hp` smallint(5) unsigned NOT NULL DEFAULT '0',
  `mp` smallint(5) unsigned NOT NULL DEFAULT '0',
  `minhit` smallint(5) unsigned NOT NULL DEFAULT '0',
  `maxhit` smallint(5) unsigned NOT NULL DEFAULT '0',
  `maxhp` smallint(5) unsigned NOT NULL DEFAULT '0',
  `maxmp` smallint(5) unsigned NOT NULL DEFAULT '0',
  `perc` smallint(5) unsigned NOT NULL DEFAULT '0',
  `reqacc` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqagi` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqarmor` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqbra` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqdex` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqevade` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqimm` tinyint(3) unsigned NOT NULL DEFAULT '0',
  `reqint` tinyint(3) unsigned NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- ----------------------------
-- Records 
-- ----------------------------
INSERT INTO `alliance` VALUES ('0', 'user');
INSERT INTO `alliance` VALUES ('1', 'monster');
INSERT INTO `alliance_attackable` VALUES ('0', '1');
INSERT INTO `alliance_attackable` VALUES ('1', '0');
INSERT INTO `alliance_hostile` VALUES ('0', '1');
INSERT INTO `alliance_hostile` VALUES ('1', '0');
INSERT INTO `character` VALUES ('1', null, 'Spodi', 'asdf', '2', '418', '466', '1', '500', '200', '1', '1', '14', '419', '50', '50', '50', '50', '50', '5', '11', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1', '1');
INSERT INTO `character` VALUES ('2', '1', 'Test A', '', '2', '930', '530', '2', '800', '250', '1', '3012', '12', '810', '527', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5');
INSERT INTO `character` VALUES ('3', '1', 'Test B', '', '2', '901.2', '530', '2', '500', '250', '1', '3012', '12', '810', '527', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5', '5');
INSERT INTO `character_template` VALUES ('1', '1', 'A Test NPC', 'TestAI', '1', '2', '0', '0', '0', '5', '5', '5', '5', '0', '0', '0', '0', '1', '0', '1', '1', '0', '0', '1', '0', '0', '0', '1', '0', '0');
INSERT INTO `character_template_equipped` VALUES ('1', '3', '10000');
INSERT INTO `character_template_equipped` VALUES ('1', '4', '5000');
INSERT INTO `character_template_equipped` VALUES ('1', '5', '5000');
INSERT INTO `character_template_inventory` VALUES ('1', '1', '0', '5', '10000');
INSERT INTO `character_template_inventory` VALUES ('1', '2', '0', '1', '10000');
INSERT INTO `character_template_inventory` VALUES ('1', '3', '1', '1', '5000');
INSERT INTO `character_template_inventory` VALUES ('1', '4', '1', '2', '5000');
INSERT INTO `character_template_inventory` VALUES ('1', '5', '0', '2', '10000');
INSERT INTO `item_template` VALUES ('1', '1', '9', '16', 'Healing Potion', 'A healing potion', '127', '10', '0', '0', '0', '0', '0', '0', '0', '0', '25', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `item_template` VALUES ('2', '1', '9', '16', 'Mana Potion', 'A mana potion', '128', '10', '0', '0', '0', '0', '0', '0', '0', '0', '0', '25', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `item_template` VALUES ('3', '2', '24', '24', 'Titanium Sword', 'A sword made out of titanium', '126', '100', '1', '0', '1', '0', '1', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `item_template` VALUES ('4', '4', '22', '22', 'Crystal Armor', 'Body armor made out of crystal', '130', '50', '0', '0', '0', '2', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
INSERT INTO `item_template` VALUES ('5', '3', '11', '16', 'Crystal Helmet', 'A helmet made out of crystal', '132', '50', '0', '0', '0', '1', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0', '0');
