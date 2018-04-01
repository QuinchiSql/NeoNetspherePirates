-- phpMyAdmin SQL Dump
-- version 4.7.4
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: Apr 01, 2018 at 03:23 AM
-- Server version: 10.1.29-MariaDB
-- PHP Version: 7.2.0

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
SET AUTOCOMMIT = 0;
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `s4l`
--

-- --------------------------------------------------------

--
-- Table structure for table `accounts`
--

CREATE TABLE `accounts` (
  `Id` int(11) NOT NULL,
  `Username` varchar(40) CHARACTER SET utf8 NOT NULL,
  `Nickname` varchar(40) CHARACTER SET utf8 DEFAULT NULL,
  `Password` varchar(40) CHARACTER SET utf8 DEFAULT NULL,
  `Salt` varchar(40) CHARACTER SET utf8 DEFAULT NULL,
  `SecurityLevel` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `AuthToken` text COLLATE utf8_bin,
  `newToken` text COLLATE utf8_bin,
  `LoginToken` text COLLATE utf8_bin,
  `LastLogin` text COLLATE utf8_bin
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

-- --------------------------------------------------------

--
-- Table structure for table `bans`
--

CREATE TABLE `bans` (
  `Id` int(11) NOT NULL,
  `AccountId` int(11) NOT NULL,
  `Date` bigint(20) NOT NULL DEFAULT '0',
  `Duration` bigint(20) DEFAULT NULL,
  `Reason` varchar(255) CHARACTER SET utf8 DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

-- --------------------------------------------------------

--
-- Table structure for table `channels`
--

CREATE TABLE `channels` (
  `Id` int(11) NOT NULL,
  `Name` varchar(255) NOT NULL DEFAULT '',
  `Description` varchar(255) NOT NULL DEFAULT '',
  `PlayerLimit` tinyint(4) NOT NULL DEFAULT '0',
  `MinLevel` tinyint(4) NOT NULL DEFAULT '0',
  `MaxLevel` tinyint(4) NOT NULL DEFAULT '0',
  `Color` int(11) UNSIGNED NOT NULL DEFAULT '0',
  `TooltipColor` int(11) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Dumping data for table `channels`
--

INSERT INTO `channels` (`Id`, `Name`, `Description`, `PlayerLimit`, `MinLevel`, `MaxLevel`, `Color`, `TooltipColor`) VALUES
(0, 'CHANNELLIST', 'CHANNELLIST', 127, 0, 127, 0, 0),
(8, 'Free', 'Unlimited', 127, 0, 80, 4294938624, 0),
(9, 'Free Event', 'Level 8 - 80', 127, 8, 80, 4294938624, 0);

-- --------------------------------------------------------

--
-- Table structure for table `clubs`
--

CREATE TABLE `clubs` (
  `Id` int(11) UNSIGNED NOT NULL,
  `Name` varchar(255) CHARACTER SET utf8 NOT NULL,
  `Icon` varchar(255) CHARACTER SET utf8 NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `club_players`
--

CREATE TABLE `club_players` (
  `PlayerId` int(11) NOT NULL,
  `ClubId` int(11) UNSIGNED NOT NULL,
  `State` int(11) NOT NULL,
  `IsMod` tinyint(1) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

-- --------------------------------------------------------

--
-- Table structure for table `license_rewards`
--

CREATE TABLE `license_rewards` (
  `Id` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `ShopItemInfoId` int(11) NOT NULL,
  `ShopPriceId` int(11) NOT NULL,
  `Color` tinyint(3) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `login_history`
--

CREATE TABLE `login_history` (
  `Id` int(11) NOT NULL,
  `AccountId` int(11) NOT NULL,
  `Date` bigint(20) NOT NULL DEFAULT '0',
  `IP` varchar(15) COLLATE utf8_bin DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

-- --------------------------------------------------------

--
-- Table structure for table `nickname_history`
--

CREATE TABLE `nickname_history` (
  `Id` int(11) NOT NULL,
  `AccountId` int(11) NOT NULL,
  `Nickname` varchar(40) CHARACTER SET utf8 NOT NULL,
  `ExpireDate` bigint(20) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

-- --------------------------------------------------------

--
-- Table structure for table `players`
--

CREATE TABLE `players` (
  `Id` int(11) NOT NULL,
  `PlayTime` varchar(60) COLLATE utf8_bin NOT NULL DEFAULT '',
  `TutorialState` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `Level` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `TotalExperience` int(11) NOT NULL DEFAULT '0',
  `PEN` int(11) NOT NULL DEFAULT '0',
  `AP` int(11) NOT NULL DEFAULT '0',
  `Coins1` int(11) NOT NULL DEFAULT '0',
  `Coins2` int(11) NOT NULL DEFAULT '0',
  `CurrentCharacterSlot` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `TotalMatches` int(11) UNSIGNED NOT NULL DEFAULT '0',
  `TotalWins` int(11) UNSIGNED NOT NULL DEFAULT '0',
  `TotalLosses` int(11) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8 COLLATE=utf8_bin;

-- --------------------------------------------------------

--
-- Table structure for table `player_characters`
--

CREATE TABLE `player_characters` (
  `Id` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `Slot` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `Gender` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `BasicHair` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `BasicFace` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `BasicShirt` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `BasicPants` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `Weapon1Id` int(11) DEFAULT NULL,
  `Weapon2Id` int(11) DEFAULT NULL,
  `Weapon3Id` int(11) DEFAULT NULL,
  `SkillId` int(11) DEFAULT NULL,
  `HairId` int(11) DEFAULT NULL,
  `FaceId` int(11) DEFAULT NULL,
  `ShirtId` int(11) DEFAULT NULL,
  `PantsId` int(11) DEFAULT NULL,
  `GlovesId` int(11) DEFAULT NULL,
  `ShoesId` int(11) DEFAULT NULL,
  `AccessoryId` int(11) DEFAULT NULL,
  `PetId` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `player_deny`
--

CREATE TABLE `player_deny` (
  `Id` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `DenyPlayerId` int(11) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `player_items`
--

CREATE TABLE `player_items` (
  `Id` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `ShopItemInfoId` int(11) NOT NULL,
  `ShopPriceId` int(11) NOT NULL,
  `Effects` char(255) DEFAULT '0',
  `Color` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `PurchaseDate` bigint(20) NOT NULL DEFAULT '0',
  `Durability` int(11) NOT NULL DEFAULT '0',
  `Count` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `player_licenses`
--

CREATE TABLE `player_licenses` (
  `Id` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `License` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `FirstCompletedDate` bigint(20) NOT NULL DEFAULT '0',
  `CompletedCount` int(11) NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `player_mails`
--

CREATE TABLE `player_mails` (
  `Id` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `SenderPlayerId` int(11) NOT NULL,
  `SentDate` bigint(20) NOT NULL DEFAULT '0',
  `Title` varchar(100) NOT NULL DEFAULT '',
  `Message` varchar(500) NOT NULL DEFAULT '',
  `IsMailNew` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `IsMailDeleted` tinyint(3) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `player_settings`
--

CREATE TABLE `player_settings` (
  `Id` int(11) NOT NULL,
  `PlayerId` int(11) NOT NULL,
  `Setting` varchar(512) NOT NULL DEFAULT '',
  `Value` varchar(512) NOT NULL DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `shop_effects`
--

CREATE TABLE `shop_effects` (
  `Id` int(11) NOT NULL,
  `EffectGroupId` int(11) NOT NULL,
  `Effect` int(20) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `shop_effect_groups`
--

CREATE TABLE `shop_effect_groups` (
  `Id` int(11) NOT NULL,
  `Name` varchar(20) NOT NULL DEFAULT '',
  `Effect` int(11) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `shop_iteminfos`
--

CREATE TABLE `shop_iteminfos` (
  `Id` int(11) NOT NULL,
  `ShopItemId` int(11) UNSIGNED NOT NULL,
  `PriceGroupId` int(11) NOT NULL,
  `EffectGroupId` int(11) NOT NULL,
  `DiscountPercentage` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `IsEnabled` tinyint(3) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `shop_items`
--

CREATE TABLE `shop_items` (
  `Id` int(10) UNSIGNED NOT NULL,
  `RequiredGender` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `RequiredLicense` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `Colors` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `UniqueColors` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `RequiredLevel` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `LevelLimit` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `RequiredMasterLevel` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `IsOneTimeUse` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `IsDestroyable` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `MainTab` smallint(5) UNSIGNED NOT NULL DEFAULT '0',
  `SubTab` smallint(5) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `shop_prices`
--

CREATE TABLE `shop_prices` (
  `Id` int(11) NOT NULL,
  `PriceGroupId` int(11) NOT NULL,
  `PeriodType` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `Period` int(11) NOT NULL DEFAULT '0',
  `Price` int(11) NOT NULL DEFAULT '0',
  `IsRefundable` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `Durability` int(11) NOT NULL DEFAULT '0',
  `IsEnabled` tinyint(3) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `shop_price_groups`
--

CREATE TABLE `shop_price_groups` (
  `Id` int(11) NOT NULL,
  `Name` varchar(20) DEFAULT '',
  `PriceType` tinyint(3) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `shop_version`
--

CREATE TABLE `shop_version` (
  `Id` int(11) NOT NULL,
  `Version` varchar(40) NOT NULL DEFAULT ''
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- --------------------------------------------------------

--
-- Table structure for table `start_items`
--

CREATE TABLE `start_items` (
  `Id` int(11) NOT NULL,
  `ShopItemInfoId` int(11) NOT NULL,
  `ShopPriceId` int(11) NOT NULL DEFAULT '0',
  `ShopEffectId` int(11) NOT NULL,
  `Color` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
  `Count` int(11) NOT NULL DEFAULT '0',
  `RequiredSecurityLevel` tinyint(3) UNSIGNED NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `accounts`
--
ALTER TABLE `accounts`
  ADD PRIMARY KEY (`Id`),
  ADD UNIQUE KEY `Username` (`Username`),
  ADD UNIQUE KEY `Nickname` (`Nickname`);

--
-- Indexes for table `bans`
--
ALTER TABLE `bans`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `AccountId` (`AccountId`);

--
-- Indexes for table `channels`
--
ALTER TABLE `channels`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `clubs`
--
ALTER TABLE `clubs`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `club_players`
--
ALTER TABLE `club_players`
  ADD PRIMARY KEY (`PlayerId`),
  ADD KEY `PlayerId` (`PlayerId`),
  ADD KEY `ClubId` (`ClubId`);

--
-- Indexes for table `license_rewards`
--
ALTER TABLE `license_rewards`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `ShopItemInfoId` (`ShopItemInfoId`),
  ADD KEY `ShopPriceId` (`ShopPriceId`);

--
-- Indexes for table `login_history`
--
ALTER TABLE `login_history`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `AccountId` (`AccountId`);

--
-- Indexes for table `nickname_history`
--
ALTER TABLE `nickname_history`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `AccountId` (`AccountId`);

--
-- Indexes for table `players`
--
ALTER TABLE `players`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `player_characters`
--
ALTER TABLE `player_characters`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `PlayerId` (`PlayerId`),
  ADD KEY `Weapon1Id` (`Weapon1Id`),
  ADD KEY `Weapon2Id` (`Weapon2Id`),
  ADD KEY `Weapon3Id` (`Weapon3Id`),
  ADD KEY `SkillId` (`SkillId`),
  ADD KEY `HairId` (`HairId`),
  ADD KEY `FaceId` (`FaceId`),
  ADD KEY `ShirtId` (`ShirtId`),
  ADD KEY `PantsId` (`PantsId`),
  ADD KEY `GlovesId` (`GlovesId`),
  ADD KEY `ShoesId` (`ShoesId`),
  ADD KEY `AccessoryId` (`AccessoryId`),
  ADD KEY `PetId` (`PetId`);

--
-- Indexes for table `player_deny`
--
ALTER TABLE `player_deny`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `PlayerId` (`PlayerId`),
  ADD KEY `DenyPlayerId` (`DenyPlayerId`);

--
-- Indexes for table `player_items`
--
ALTER TABLE `player_items`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `PlayerId` (`PlayerId`),
  ADD KEY `ShopItemInfoId` (`ShopItemInfoId`),
  ADD KEY `ShopPriceId` (`ShopPriceId`);

--
-- Indexes for table `player_licenses`
--
ALTER TABLE `player_licenses`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `PlayerId` (`PlayerId`);

--
-- Indexes for table `player_mails`
--
ALTER TABLE `player_mails`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `PlayerId` (`PlayerId`),
  ADD KEY `SenderPlayerId` (`SenderPlayerId`);

--
-- Indexes for table `player_settings`
--
ALTER TABLE `player_settings`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `PlayerId` (`PlayerId`);

--
-- Indexes for table `shop_effects`
--
ALTER TABLE `shop_effects`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `EffectGroupId` (`EffectGroupId`);

--
-- Indexes for table `shop_effect_groups`
--
ALTER TABLE `shop_effect_groups`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `shop_iteminfos`
--
ALTER TABLE `shop_iteminfos`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `PriceGroupId` (`PriceGroupId`),
  ADD KEY `EffectGroupId` (`EffectGroupId`),
  ADD KEY `ShopItemId` (`ShopItemId`);

--
-- Indexes for table `shop_items`
--
ALTER TABLE `shop_items`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `shop_prices`
--
ALTER TABLE `shop_prices`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `PriceGroupId` (`PriceGroupId`);

--
-- Indexes for table `shop_price_groups`
--
ALTER TABLE `shop_price_groups`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `shop_version`
--
ALTER TABLE `shop_version`
  ADD PRIMARY KEY (`Id`);

--
-- Indexes for table `start_items`
--
ALTER TABLE `start_items`
  ADD PRIMARY KEY (`Id`),
  ADD KEY `ShopItemInfoId` (`ShopItemInfoId`),
  ADD KEY `ShopPriceId` (`ShopPriceId`),
  ADD KEY `ShopEffectId` (`ShopEffectId`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `accounts`
--
ALTER TABLE `accounts`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=237;

--
-- AUTO_INCREMENT for table `bans`
--
ALTER TABLE `bans`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `channels`
--
ALTER TABLE `channels`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=10;

--
-- AUTO_INCREMENT for table `clubs`
--
ALTER TABLE `clubs`
  MODIFY `Id` int(11) UNSIGNED NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `login_history`
--
ALTER TABLE `login_history`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3070;

--
-- AUTO_INCREMENT for table `nickname_history`
--
ALTER TABLE `nickname_history`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `player_mails`
--
ALTER TABLE `player_mails`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=13;

--
-- AUTO_INCREMENT for table `player_settings`
--
ALTER TABLE `player_settings`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `shop_effects`
--
ALTER TABLE `shop_effects`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=17;

--
-- AUTO_INCREMENT for table `shop_effect_groups`
--
ALTER TABLE `shop_effect_groups`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=14;

--
-- AUTO_INCREMENT for table `shop_iteminfos`
--
ALTER TABLE `shop_iteminfos`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2619;

--
-- AUTO_INCREMENT for table `shop_prices`
--
ALTER TABLE `shop_prices`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `shop_price_groups`
--
ALTER TABLE `shop_price_groups`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `shop_version`
--
ALTER TABLE `shop_version`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=19;

--
-- AUTO_INCREMENT for table `start_items`
--
ALTER TABLE `start_items`
  MODIFY `Id` int(11) NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `bans`
--
ALTER TABLE `bans`
  ADD CONSTRAINT `bans_ibfk_1` FOREIGN KEY (`AccountId`) REFERENCES `accounts` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `club_players`
--
ALTER TABLE `club_players`
  ADD CONSTRAINT `club_players_ibfk_1` FOREIGN KEY (`PlayerId`) REFERENCES `accounts` (`Id`),
  ADD CONSTRAINT `club_players_ibfk_2` FOREIGN KEY (`ClubId`) REFERENCES `clubs` (`Id`);

--
-- Constraints for table `license_rewards`
--
ALTER TABLE `license_rewards`
  ADD CONSTRAINT `license_rewards_ibfk_1` FOREIGN KEY (`ShopItemInfoId`) REFERENCES `shop_iteminfos` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `license_rewards_ibfk_2` FOREIGN KEY (`ShopPriceId`) REFERENCES `shop_prices` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `login_history`
--
ALTER TABLE `login_history`
  ADD CONSTRAINT `login_history_ibfk_1` FOREIGN KEY (`AccountId`) REFERENCES `accounts` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `nickname_history`
--
ALTER TABLE `nickname_history`
  ADD CONSTRAINT `nickname_history_ibfk_1` FOREIGN KEY (`AccountId`) REFERENCES `accounts` (`Id`) ON DELETE CASCADE;

--
-- Constraints for table `player_deny`
--
ALTER TABLE `player_deny`
  ADD CONSTRAINT `player_deny_ibfk_1` FOREIGN KEY (`PlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE,
  ADD CONSTRAINT `player_deny_ibfk_2` FOREIGN KEY (`DenyPlayerId`) REFERENCES `players` (`Id`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
