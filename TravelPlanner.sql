-- Database export via SQLPro (https://www.sqlprostudio.com/)
-- Exported by kuassitehou at 30-07-2025 14:28.
-- WARNING: This file may contain descructive statements such as DROPs.
-- Please ensure that you are running the script at the proper location.


-- BEGIN TABLE dbo.Activite
CREATE TABLE dbo.Activite (
	ActiviteID int NOT NULL IDENTITY(1,1),
	Nom nvarchar(100) NOT NULL,
	Description nvarchar(max) NULL,
	Localisation nvarchar(255) NULL
);
GO

ALTER TABLE dbo.Activite ADD CONSTRAINT PK__Activite__BE3FB865F23B2E86 PRIMARY KEY (ActiviteID);
GO

-- Inserting 24 rows into dbo.Activite
SET IDENTITY_INSERT dbo.Activite ON

INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (1, 'Visite guidée de la Sagrada Familia', 'Découverte de l''œuvre inachevée de Gaudí', 'Sagrada Familia, Barcelona, Spain');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (2, 'Randonnée glaciaire', 'Excursion avec crampons sur le glacier des Bossons', 'Glacier des Bossons, Chamonix, France');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (3, 'Surf à Malibu', 'Leçon de surf avec moniteur local', 'Malibu Beach, California, USA');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (4, 'Plongée avec masque et tuba', 'Exploration des fonds marins des Cyclades', 'Santorini, Cyclades, Greece');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (5, 'Safari photo', 'Recherche des Big Five avec guide expérimenté', 'Masai Mara, Kenya');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (6, 'Massage balinais', 'Détente en bord de plage', 'Ubud, Bali, Indonesia');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (7, 'Cérémonie du thé', 'Expérience traditionnelle dans un ryokan', 'Kyoto, Japan');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (8, 'Cours de cuisine toscane', 'Préparation des pâtes fraîches et tiramisu', 'Florence, Tuscany, Italy');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (9, 'Trail des volcans', 'Course à pied dans les paysages lunaires islandais', 'Landmannalaugar, Iceland');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (10, 'Balade à dos de dromadaire', 'Coucher de soleil dans les dunes du Sahara', 'Sahara Desert, Morocco');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (11, 'Dégustation de vins', 'Découverte des cépages locaux', 'Bordeaux, France');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (12, 'Visite de médina', 'Exploration des souks et places historiques', 'Fez Medina, Morocco');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (20, 'Safari éléphants Pendjari', 'Observation des derniers éléphants d''Afrique de l''Ouest', 'Parc National de la Pendjari, Bénin');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (21, 'Pirogue sur la rivière W', 'Navigation traditionnelle, observation hippopotames', 'Parc National du W, Bénin');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (22, 'Visite village Somba', 'Découverte architecture traditionnelle Tata Somba', 'Natitingou, Bénin');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (23, 'Migration des gnous', 'Spectacle de la grande migration au Serengeti', 'Parc National du Serengeti, Tanzanie');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (24, 'Safari Big Five Ngorongoro', 'Recherche des Big Five dans le cratère', 'Cratère de Ngorongoro, Tanzanie');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (25, 'Ascension Kilimandjaro', 'Trek vers le toit de l''Afrique', 'Mont Kilimandjaro, Tanzanie');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (26, 'Observation geysers Yellowstone', 'Old Faithful et geysers du parc', 'Yellowstone National Park, USA');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (27, 'Randonnée Grand Canyon', 'Descente dans le canyon par Bright Angel Trail', 'Grand Canyon National Park, USA');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (28, 'Escalade El Capitan', 'Initiation escalade sur la célèbre paroi', 'Yosemite National Park, USA');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (29, 'Plongée raies manta', 'Nage avec les raies manta géantes', 'Baa Atoll, Maldives');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (30, 'Snorkeling tortues géantes', 'Observation tortues d''Aldabra en milieu naturel', 'Aldabra, Seychelles');
INSERT INTO dbo.Activite (ActiviteID, Nom, Description, Localisation) VALUES (31, 'Tour du lagon Bora Bora', 'Excursion bateau dans le lagon mythique', 'Bora Bora, Polynésie Française');

SET IDENTITY_INSERT dbo.Activite OFF

-- END TABLE dbo.Activite

-- BEGIN TABLE dbo.ActiviteVoyage
CREATE TABLE dbo.ActiviteVoyage (
	ActiviteID int NOT NULL,
	VoyageID int NOT NULL
);
GO

ALTER TABLE dbo.ActiviteVoyage ADD CONSTRAINT PK_ActiviteVoyage PRIMARY KEY (ActiviteID, VoyageID);
GO

-- Inserting 24 rows into dbo.ActiviteVoyage
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (1, 1);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (2, 2);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (2, 9);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (3, 3);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (4, 6);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (5, 5);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (6, 6);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (7, 7);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (8, 8);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (9, 9);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (11, 8);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (20, 10);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (21, 11);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (22, 10);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (22, 11);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (23, 12);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (24, 13);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (25, 12);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (26, 14);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (27, 15);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (28, 16);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (29, 17);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (30, 18);
INSERT INTO dbo.ActiviteVoyage (ActiviteID, VoyageID) VALUES (31, 19);

-- END TABLE dbo.ActiviteVoyage

-- BEGIN TABLE dbo.ClassementVoyageur
CREATE TABLE dbo.ClassementVoyageur (
	ClassementID int NOT NULL IDENTITY(1,1),
	UtilisateurID int NOT NULL,
	Rang int NOT NULL,
	NombreVoyages int NOT NULL DEFAULT (0),
	DistanceTotale decimal(10,2) NOT NULL DEFAULT (0)
);
GO

ALTER TABLE dbo.ClassementVoyageur ADD CONSTRAINT PK__Classeme__63F085DD50156D31 PRIMARY KEY (ClassementID);
GO

-- Table dbo.ClassementVoyageur contains no data. No inserts have been generated.
-- END TABLE dbo.ClassementVoyageur

-- BEGIN TABLE dbo.GroupeVoyage
CREATE TABLE dbo.GroupeVoyage (
	GroupeID int NOT NULL IDENTITY(1,1),
	NomGroupe nvarchar(100) NOT NULL,
	DateCreation date NULL DEFAULT (getdate())
);
GO

ALTER TABLE dbo.GroupeVoyage ADD CONSTRAINT PK__GroupeVo__5C811B3078FA0CDF PRIMARY KEY (GroupeID);
GO

-- Inserting 5 rows into dbo.GroupeVoyage
SET IDENTITY_INSERT dbo.GroupeVoyage ON

INSERT INTO dbo.GroupeVoyage (GroupeID, NomGroupe, DateCreation) VALUES (1, 'Aventuriers du week-end', '2025-07-23 00:00:00');
INSERT INTO dbo.GroupeVoyage (GroupeID, NomGroupe, DateCreation) VALUES (2, 'Famille globe-trotteuse', '2025-07-23 00:00:00');
INSERT INTO dbo.GroupeVoyage (GroupeID, NomGroupe, DateCreation) VALUES (3, 'Amis passionnés de randonnée', '2025-07-23 00:00:00');
INSERT INTO dbo.GroupeVoyage (GroupeID, NomGroupe, DateCreation) VALUES (4, 'Couples voyageurs', '2025-07-23 00:00:00');
INSERT INTO dbo.GroupeVoyage (GroupeID, NomGroupe, DateCreation) VALUES (5, 'Explorateurs urbains', '2025-07-23 00:00:00');

SET IDENTITY_INSERT dbo.GroupeVoyage OFF

-- END TABLE dbo.GroupeVoyage

-- BEGIN TABLE dbo.Hebergement
CREATE TABLE dbo.Hebergement (
	HebergementID int NOT NULL IDENTITY(1,1),
	Nom nvarchar(100) NOT NULL,
	TypeHebergement nvarchar(50) NULL,
	Cout decimal(10,2) NULL,
	DateDebut date NULL,
	DateFin date NULL,
	Adresse nvarchar(255) NULL
);
GO

ALTER TABLE dbo.Hebergement ADD CONSTRAINT PK__Hebergem__35A3F6B1A87E8D30 PRIMARY KEY (HebergementID);
GO

-- Inserting 20 rows into dbo.Hebergement
SET IDENTITY_INSERT dbo.Hebergement ON

INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (1, 'Hôtel Casa Camper', 'Hôtel boutique', 180, '2025-08-07 00:00:00', '2025-08-09 00:00:00', 'Carrer de Elisabets, 11, Barcelona, Spain');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (2, 'Refuge du Montenvers', 'Auberge de montagne', 75, '2025-08-22 00:00:00', '2025-08-29 00:00:00', '74400 Chamonix-Mont-Blanc, France');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (3, 'Motel vintage Route 66', 'Motel thématique', 120, '2025-09-21 00:00:00', '2025-10-06 00:00:00', 'Route 66, Williams, Arizona, USA');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (4, 'Catamaran de luxe', 'Cabine croisière', 250, '2025-10-21 00:00:00', '2025-10-28 00:00:00', 'Marina de Naxos, Cyclades, Greece');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (5, 'Lodge safari', 'Tente de luxe', 300, '2025-11-20 00:00:00', '2025-11-30 00:00:00', 'Masai Mara National Reserve, Kenya');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (6, 'Bungalow sur pilotis', 'Hébergement plage', 400, '2025-09-06 00:00:00', '2025-09-13 00:00:00', 'Bora Bora, French Polynesia');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (7, 'Ryokan traditionnel', 'Auberge japonaise', 150, '2025-10-31 00:00:00', '2025-11-10 00:00:00', 'Gion District, Kyoto, Japan');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (8, 'Agriturismo toscan', 'Ferme viticole', 130, '2025-08-17 00:00:00', '2025-08-27 00:00:00', 'Val d''Orcia, Tuscany, Italy');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (9, 'Chalet islandais', 'Maison typique', 170, '2025-10-11 00:00:00', '2025-10-18 00:00:00', 'Reykjavik, Iceland');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (10, 'Riad marocain', 'Maison d''hôte', 90, '2025-09-11 00:00:00', '2025-09-21 00:00:00', 'Medina, Marrakech, Morocco');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (20, 'Lodge Pendjari', 'Eco-lodge safari', 85, '2025-12-15 00:00:00', '2025-12-22 00:00:00', 'Parc National Pendjari, Bénin');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (21, 'Campement du W', 'Campement brousse', 65, '2026-01-10 00:00:00', '2026-01-17 00:00:00', 'Parc National du W, Bénin');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (22, 'Serengeti Safari Camp', 'Tente de luxe safari', 350, '2026-02-20 00:00:00', '2026-03-05 00:00:00', 'Serengeti National Park, Tanzanie');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (23, 'Ngorongoro Crater Lodge', 'Lodge luxe cratère', 450, '2026-04-12 00:00:00', '2026-04-19 00:00:00', 'Ngorongoro Conservation Area, Tanzanie');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (24, 'Old Faithful Inn', 'Lodge historique', 200, '2026-05-25 00:00:00', '2026-06-08 00:00:00', 'Yellowstone National Park, Wyoming, USA');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (25, 'Grand Canyon Lodge', 'Lodge au bord du canyon', 180, '2026-07-15 00:00:00', '2026-07-25 00:00:00', 'Grand Canyon National Park, Arizona, USA');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (26, 'Ahwahnee Hotel Yosemite', 'Hôtel de luxe montagne', 320, '2026-08-20 00:00:00', '2026-08-30 00:00:00', 'Yosemite National Park, California, USA');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (27, 'Overwater Villa Maldives', 'Villa sur pilotis', 600, '2026-10-05 00:00:00', '2026-10-15 00:00:00', 'Baa Atoll, Maldives');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (28, 'Resort Praslin Seychelles', 'Resort de plage luxe', 450, '2026-11-12 00:00:00', '2026-11-22 00:00:00', 'Praslin Island, Seychelles');
INSERT INTO dbo.Hebergement (HebergementID, Nom, TypeHebergement, Cout, DateDebut, DateFin, Adresse) VALUES (29, 'Bungalow Bora Bora', 'Bungalow lagon', 550, '2027-01-08 00:00:00', '2027-01-18 00:00:00', 'Bora Bora, Polynésie Française');

SET IDENTITY_INSERT dbo.Hebergement OFF

-- END TABLE dbo.Hebergement

-- BEGIN TABLE dbo.HebergementVoyage
CREATE TABLE dbo.HebergementVoyage (
	VoyageID int NOT NULL,
	HebergementID int NOT NULL
);
GO

ALTER TABLE dbo.HebergementVoyage ADD CONSTRAINT PK_HebergementVoyage PRIMARY KEY (VoyageID, HebergementID);
GO

-- Inserting 18 rows into dbo.HebergementVoyage
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (1, 1);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (2, 2);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (3, 3);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (5, 5);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (6, 6);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (7, 7);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (8, 8);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (9, 9);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (10, 20);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (11, 21);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (12, 22);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (13, 23);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (14, 24);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (15, 25);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (16, 26);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (17, 27);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (18, 28);
INSERT INTO dbo.HebergementVoyage (VoyageID, HebergementID) VALUES (19, 29);

-- END TABLE dbo.HebergementVoyage

-- BEGIN TABLE dbo.MembreGroupe
CREATE TABLE dbo.MembreGroupe (
	MembreGroupeID int NOT NULL IDENTITY(1,1),
	UtilisateurID int NOT NULL,
	GroupeID int NOT NULL,
	Role nvarchar(50) NOT NULL,
	DateAdhesion date NOT NULL DEFAULT (getdate())
);
GO

ALTER TABLE dbo.MembreGroupe ADD CONSTRAINT PK__MembreGr__DED3D73B96357064 PRIMARY KEY (MembreGroupeID);
GO

-- Inserting 10 rows into dbo.MembreGroupe
SET IDENTITY_INSERT dbo.MembreGroupe ON

INSERT INTO dbo.MembreGroupe (MembreGroupeID, UtilisateurID, GroupeID, Role, DateAdhesion) VALUES (1, 1, 1, 'Organisateur', '2025-07-23 00:00:00');
INSERT INTO dbo.MembreGroupe (MembreGroupeID, UtilisateurID, GroupeID, Role, DateAdhesion) VALUES (2, 2, 1, 'Membre', '2025-07-23 00:00:00');
INSERT INTO dbo.MembreGroupe (MembreGroupeID, UtilisateurID, GroupeID, Role, DateAdhesion) VALUES (3, 3, 2, 'Organisateur', '2025-07-23 00:00:00');
INSERT INTO dbo.MembreGroupe (MembreGroupeID, UtilisateurID, GroupeID, Role, DateAdhesion) VALUES (4, 4, 2, 'Membre', '2025-07-23 00:00:00');
INSERT INTO dbo.MembreGroupe (MembreGroupeID, UtilisateurID, GroupeID, Role, DateAdhesion) VALUES (5, 5, 3, 'Organisateur', '2025-07-23 00:00:00');
INSERT INTO dbo.MembreGroupe (MembreGroupeID, UtilisateurID, GroupeID, Role, DateAdhesion) VALUES (6, 1, 3, 'Membre', '2025-07-23 00:00:00');
INSERT INTO dbo.MembreGroupe (MembreGroupeID, UtilisateurID, GroupeID, Role, DateAdhesion) VALUES (7, 2, 4, 'Organisateur', '2025-07-23 00:00:00');
INSERT INTO dbo.MembreGroupe (MembreGroupeID, UtilisateurID, GroupeID, Role, DateAdhesion) VALUES (8, 3, 4, 'Membre', '2025-07-23 00:00:00');
INSERT INTO dbo.MembreGroupe (MembreGroupeID, UtilisateurID, GroupeID, Role, DateAdhesion) VALUES (9, 4, 5, 'Organisateur', '2025-07-23 00:00:00');
INSERT INTO dbo.MembreGroupe (MembreGroupeID, UtilisateurID, GroupeID, Role, DateAdhesion) VALUES (10, 5, 5, 'Membre', '2025-07-23 00:00:00');

SET IDENTITY_INSERT dbo.MembreGroupe OFF

-- END TABLE dbo.MembreGroupe

-- BEGIN TABLE dbo.NiveauRecompense
CREATE TABLE dbo.NiveauRecompense (
	NiveauRecompenseID int NOT NULL IDENTITY(1,1),
	NomNiveau nvarchar(50) NOT NULL,
	PointsRequis int NOT NULL,
	Avantages nvarchar(max) NULL
);
GO

ALTER TABLE dbo.NiveauRecompense ADD CONSTRAINT PK__NiveauRe__04A74635554B36E3 PRIMARY KEY (NiveauRecompenseID);
GO

-- Table dbo.NiveauRecompense contains no data. No inserts have been generated.
-- END TABLE dbo.NiveauRecompense

-- BEGIN TABLE dbo.OrganisationVoyage
CREATE TABLE dbo.OrganisationVoyage (
	UtilisateurID int NOT NULL,
	VoyageID int NOT NULL
);
GO

ALTER TABLE dbo.OrganisationVoyage ADD CONSTRAINT PK_OrganisationVoyage PRIMARY KEY (UtilisateurID, VoyageID);
GO

-- Inserting 18 rows into dbo.OrganisationVoyage
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1, 1);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1, 6);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (2, 2);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (2, 7);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (3, 3);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (3, 8);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (4, 9);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (5, 5);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1001, 10);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1001, 11);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1001, 12);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1001, 13);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1001, 14);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1001, 15);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1001, 16);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1001, 17);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1001, 18);
INSERT INTO dbo.OrganisationVoyage (UtilisateurID, VoyageID) VALUES (1001, 19);

-- END TABLE dbo.OrganisationVoyage

-- BEGIN TABLE dbo.PointsRecompense
CREATE TABLE dbo.PointsRecompense (
	PointsRecompenseID int NOT NULL IDENTITY(1,1),
	PointsGagnes int NOT NULL,
	DateObtention date NOT NULL DEFAULT (getdate()),
	UtilisateurID int NOT NULL,
	NiveauRecompenseID int NULL
);
GO

ALTER TABLE dbo.PointsRecompense ADD CONSTRAINT PK__PointsRe__9A7FC267F35BD8D3 PRIMARY KEY (PointsRecompenseID);
GO

-- Table dbo.PointsRecompense contains no data. No inserts have been generated.
-- END TABLE dbo.PointsRecompense

-- BEGIN TABLE dbo.ReservationHebergement
CREATE TABLE dbo.ReservationHebergement (
	ReservationID int NOT NULL IDENTITY(1,1),
	HebergementID int NOT NULL,
	StatutReservation nvarchar(50) NOT NULL,
	NumConfirmation nvarchar(50) NOT NULL
);
GO

ALTER TABLE dbo.ReservationHebergement ADD CONSTRAINT PK__Reservat__B7EE5F04BCBEFAA4 PRIMARY KEY (ReservationID);
GO

-- Table dbo.ReservationHebergement contains no data. No inserts have been generated.
-- END TABLE dbo.ReservationHebergement

-- BEGIN TABLE dbo.Utilisateur
CREATE TABLE dbo.Utilisateur (
	UtilisateurID int NOT NULL IDENTITY(1,1),
	Nom nvarchar(100) NOT NULL,
	Prenom nvarchar(100) NOT NULL,
	Email nvarchar(255) NOT NULL,
	MotDePasse nvarchar(255) NOT NULL,
	PointsRecompenses int NOT NULL DEFAULT (0)
);
GO

ALTER TABLE dbo.Utilisateur ADD CONSTRAINT PK__Utilisat__6CB6AE1F1218C985 PRIMARY KEY (UtilisateurID);
GO

-- Inserting 6 rows into dbo.Utilisateur
SET IDENTITY_INSERT dbo.Utilisateur ON

INSERT INTO dbo.Utilisateur (UtilisateurID, Nom, Prenom, Email, MotDePasse, PointsRecompenses) VALUES (1, 'Martin', 'Sophie', 'sophie.martin@email.com', 'Vacances2023!', 120);
INSERT INTO dbo.Utilisateur (UtilisateurID, Nom, Prenom, Email, MotDePasse, PointsRecompenses) VALUES (2, 'Bernard', 'Thomas', 'thomas.bernard@email.com', 'Azerty123', 85);
INSERT INTO dbo.Utilisateur (UtilisateurID, Nom, Prenom, Email, MotDePasse, PointsRecompenses) VALUES (3, 'Petit', 'Camille', 'camille.petit@email.com', 'M0tDeP@sse', 210);
INSERT INTO dbo.Utilisateur (UtilisateurID, Nom, Prenom, Email, MotDePasse, PointsRecompenses) VALUES (4, 'Leroy', 'Nicolas', 'nicolas.leroy@email.com', 'NicoTravel45', 150);
INSERT INTO dbo.Utilisateur (UtilisateurID, Nom, Prenom, Email, MotDePasse, PointsRecompenses) VALUES (5, 'Moreau', 'Laura', 'laura.moreau@email.com', 'L0ndres2023', 95);
INSERT INTO dbo.Utilisateur (UtilisateurID, Nom, Prenom, Email, MotDePasse, PointsRecompenses) VALUES (1001, 'Tehou', 'Hasler', 'hasler.tehou@email.com', 'nLmGR36mtBLhVx+hj6/SEIMDmdh2L6h0SEQJUCId8cY=', 500);

SET IDENTITY_INSERT dbo.Utilisateur OFF

-- END TABLE dbo.Utilisateur

-- BEGIN TABLE dbo.Voyage
CREATE TABLE dbo.Voyage (
	VoyageID int NOT NULL IDENTITY(1,1),
	NomVoyage nvarchar(100) NOT NULL,
	Description nvarchar(max) NULL,
	DateDebut date NOT NULL,
	DateFin date NOT NULL,
	EstComplete bit NOT NULL DEFAULT (0),
	EstArchive bit NOT NULL DEFAULT (0),
	UtilisateurId int NOT NULL DEFAULT (1)
);
GO

ALTER TABLE dbo.Voyage ADD CONSTRAINT PK__Voyage__577D73A343C0B05F PRIMARY KEY (VoyageID);
GO

-- Inserting 18 rows into dbo.Voyage
SET IDENTITY_INSERT dbo.Voyage ON

INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (1, 'Week-end à Barcelone', 'Découverte de la Sagrada Familia et des tapas', '2025-08-05 00:00:00', '2025-08-21 00:00:00', 1, 1, 1);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (2, 'Randonnée dans les Alpes', 'Traversée du massif du Mont-Blanc', '2025-08-22 00:00:00', '2025-08-29 00:00:00', 1, 1, 1);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (3, 'Roadtrip en Californie', 'De San Francisco à Los Angeles par la Highway 1', '2025-09-21 00:00:00', '2025-10-06 00:00:00', 1, 1, 1);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (5, 'Safari en Tanzanie', 'Parcs nationaux du Serengeti et Ngorongoro', '2025-11-20 00:00:00', '2025-11-30 00:00:00', 1, 1, 1);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (6, 'Détente aux Maldives', 'Plongée et farniente sur les plages de sable blanc', '2025-09-06 00:00:00', '2025-09-13 00:00:00', 0, 0, 1);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (7, 'Tour culturel au Japon', 'Tokyo, Kyoto et Hiroshima en 10 jours', '2025-10-31 00:00:00', '2025-11-10 00:00:00', 0, 0, 1);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (8, 'Voyage gastronomique en Italie', 'Rome, Florence et Bologne', '2025-08-17 00:00:00', '2025-08-27 00:00:00', 1, 1, 1);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (9, 'Aventure islandaise', 'Route du Cercle d''Or et aurores boréales', '2025-10-11 00:00:00', '2025-10-18 00:00:00', 0, 0, 1);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (10, 'Safari Pendjari - Bénin', 'Exploration du plus grand parc national du Bénin, observation des éléphants et lions d''Afrique de l''Ouest', '2025-12-02 00:00:00', '2025-12-31 00:00:00', 0, 0, 1001);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (11, 'Réserve de biosphère du W', 'Découverte du parc transfrontalier du W, observation des hippopotames et crocodiles', '2026-01-10 00:00:00', '2026-01-17 00:00:00', 1, 1, 1001);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (12, 'Grande migration du Serengeti', 'Témoin de la plus grande migration animale au monde, Big Five safari', '2026-02-20 00:00:00', '2026-03-05 00:00:00', 0, 1, 1001);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (13, 'Cratère de Ngorongoro', 'Safari dans le plus grand cratère volcanique intact du monde, rhinocéros noirs', '2026-04-12 00:00:00', '2026-04-19 00:00:00', 1, 1, 1001);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (14, 'Merveilles de Yellowstone', 'Geysers, sources chaudes, observation des bisons et loups dans le premier parc national au monde', '2026-05-25 00:00:00', '2026-06-08 00:00:00', 1, 1, 1001);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (15, 'Grand Canyon Arizona', 'Randonnées sur les rims, survol en hélicoptère, couchers de soleil spectaculaires', '2026-07-15 00:00:00', '2026-07-25 00:00:00', 0, 1, 1001);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (16, 'Yosemite et ses géants', 'Séquoias géants, cascades El Capitan, escalade et randonnée en haute montagne', '2026-08-20 00:00:00', '2026-08-30 00:00:00', 1, 1, 1001);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (17, 'Paradis des Maldives', 'Atolls cristallins, plongée avec raies manta, détente en overwater bungalow', '2026-10-05 00:00:00', '2026-10-15 00:00:00', 1, 1, 1001);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (18, 'Joyaux des Seychelles', 'Praslin, La Digue, plages de rêve, tortues géantes d''Aldabra', '2026-11-12 00:00:00', '2026-11-22 00:00:00', 0, 0, 1001);
INSERT INTO dbo.Voyage (VoyageID, NomVoyage, Description, DateDebut, DateFin, EstComplete, EstArchive, UtilisateurId) VALUES (19, 'Perle de Bora Bora', 'Lagon turquoise, mont Otemanu, culture polynésienne authentique', '2027-01-08 00:00:00', '2027-01-18 00:00:00', 1, 1, 1001);

SET IDENTITY_INSERT dbo.Voyage OFF

-- END TABLE dbo.Voyage

IF OBJECT_ID('dbo.Voyage', 'U') IS NOT NULL AND OBJECT_ID('dbo.Voyage', 'U') IS NOT NULL
	ALTER TABLE dbo.ActiviteVoyage
	ADD CONSTRAINT FK_ActiviteVoyage_Voyage
	FOREIGN KEY (VoyageID)
	REFERENCES dbo.Voyage (VoyageID);

IF OBJECT_ID('dbo.Activite', 'U') IS NOT NULL AND OBJECT_ID('dbo.Activite', 'U') IS NOT NULL
	ALTER TABLE dbo.ActiviteVoyage
	ADD CONSTRAINT FK_ActiviteVoyage_Activite
	FOREIGN KEY (ActiviteID)
	REFERENCES dbo.Activite (ActiviteID);

IF OBJECT_ID('dbo.Utilisateur', 'U') IS NOT NULL AND OBJECT_ID('dbo.Utilisateur', 'U') IS NOT NULL
	ALTER TABLE dbo.ClassementVoyageur
	ADD CONSTRAINT FK_Classement_Utilisateur
	FOREIGN KEY (UtilisateurID)
	REFERENCES dbo.Utilisateur (UtilisateurID);

IF OBJECT_ID('dbo.Voyage', 'U') IS NOT NULL AND OBJECT_ID('dbo.Voyage', 'U') IS NOT NULL
	ALTER TABLE dbo.HebergementVoyage
	ADD CONSTRAINT FK_HebergementVoyage_Voyage
	FOREIGN KEY (VoyageID)
	REFERENCES dbo.Voyage (VoyageID);

IF OBJECT_ID('dbo.Hebergement', 'U') IS NOT NULL AND OBJECT_ID('dbo.Hebergement', 'U') IS NOT NULL
	ALTER TABLE dbo.HebergementVoyage
	ADD CONSTRAINT FK_HebergementVoyage_Hebergement
	FOREIGN KEY (HebergementID)
	REFERENCES dbo.Hebergement (HebergementID);

IF OBJECT_ID('dbo.GroupeVoyage', 'U') IS NOT NULL AND OBJECT_ID('dbo.GroupeVoyage', 'U') IS NOT NULL
	ALTER TABLE dbo.MembreGroupe
	ADD CONSTRAINT FK_Membre_Groupe
	FOREIGN KEY (GroupeID)
	REFERENCES dbo.GroupeVoyage (GroupeID);

IF OBJECT_ID('dbo.Utilisateur', 'U') IS NOT NULL AND OBJECT_ID('dbo.Utilisateur', 'U') IS NOT NULL
	ALTER TABLE dbo.MembreGroupe
	ADD CONSTRAINT FK_Membre_Utilisateur
	FOREIGN KEY (UtilisateurID)
	REFERENCES dbo.Utilisateur (UtilisateurID);

IF OBJECT_ID('dbo.Voyage', 'U') IS NOT NULL AND OBJECT_ID('dbo.Voyage', 'U') IS NOT NULL
	ALTER TABLE dbo.OrganisationVoyage
	ADD CONSTRAINT FK_OrganisationVoyage_Voyage
	FOREIGN KEY (VoyageID)
	REFERENCES dbo.Voyage (VoyageID);

IF OBJECT_ID('dbo.Utilisateur', 'U') IS NOT NULL AND OBJECT_ID('dbo.Utilisateur', 'U') IS NOT NULL
	ALTER TABLE dbo.OrganisationVoyage
	ADD CONSTRAINT FK_OrganisationVoyage_Utilisateur
	FOREIGN KEY (UtilisateurID)
	REFERENCES dbo.Utilisateur (UtilisateurID);

IF OBJECT_ID('dbo.NiveauRecompense', 'U') IS NOT NULL AND OBJECT_ID('dbo.NiveauRecompense', 'U') IS NOT NULL
	ALTER TABLE dbo.PointsRecompense
	ADD CONSTRAINT FK_Points_NiveauRecompense
	FOREIGN KEY (NiveauRecompenseID)
	REFERENCES dbo.NiveauRecompense (NiveauRecompenseID);

IF OBJECT_ID('dbo.Utilisateur', 'U') IS NOT NULL AND OBJECT_ID('dbo.Utilisateur', 'U') IS NOT NULL
	ALTER TABLE dbo.PointsRecompense
	ADD CONSTRAINT FK_Points_Utilisateur
	FOREIGN KEY (UtilisateurID)
	REFERENCES dbo.Utilisateur (UtilisateurID);

IF OBJECT_ID('dbo.Hebergement', 'U') IS NOT NULL AND OBJECT_ID('dbo.Hebergement', 'U') IS NOT NULL
	ALTER TABLE dbo.ReservationHebergement
	ADD CONSTRAINT FK_Reservation_Hebergement
	FOREIGN KEY (HebergementID)
	REFERENCES dbo.Hebergement (HebergementID);

IF OBJECT_ID('dbo.Utilisateur', 'U') IS NOT NULL AND OBJECT_ID('dbo.Utilisateur', 'U') IS NOT NULL
	ALTER TABLE dbo.Voyage
	ADD CONSTRAINT FK_Voyage_Utilisateur
	FOREIGN KEY (UtilisateurId)
	REFERENCES dbo.Utilisateur (UtilisateurID);

