# SGE - Système de Gestion d'Entreprise

API REST développée avec ASP.NET Core pour la gestion d'entreprise.

## 🚀 Démarrage rapide

### Prérequis

- .NET 9.0 SDK
- Docker
- dotnet-ef CLI tools

### Installation des outils

```bash
dotnet tool install --global dotnet-ef --version 9.0.10
```

### Configuration

1. Configurez la connexion PostgreSQL dans `SGE.API/appsettings.json`
2. Configurez la clé JWT (minimum 32 caractères)

### Base de données

```bash
# Créer/Appliquer les migrations
dotnet ef database update --project SGE.Infrastructure --startup-project SGE.API

# Supprimer la base de données
dotnet ef database drop --project SGE.Infrastructure --startup-project SGE.API
```

### Lancer l'application

Utiliser Rider

## 🔐 Authentification

L'API utilise JWT (JSON Web Tokens). Au premier démarrage, deux comptes sont créés :

| Rôle    | Email              | Mot de passe  |
|---------|-------------------|---------------|
| Admin   | admin@sge.com     | Admin123!     |
| Manager | manager@sge.com   | Manager123!   |

### Endpoints d'authentification

- `POST /api/auth/register` - Inscription
- `POST /api/auth/login` - Connexion
- `POST /api/auth/refresh-token` - Rafraîchir le token
- `GET /api/auth/me` - Utilisateur courant
- `POST /api/auth/logout` - Déconnexion

## 📚 Documentation API

La collection Postman se trouve dans le dossier `./Postman`

## 🏗️ Architecture

```
SGE/
├── SGE.API/              # Contrôleurs et configuration
├── SGE.Application/      # Services et logique métier
├── SGE.Core/             # Entités et interfaces
└── SGE.Infrastructure/   # Accès données et repositories
```

## 🔒 Autorisations

- **Admin** : Accès complet (CRUD sur toutes les ressources)
- **Manager** : Lecture + Création/Modification
- **User** : Lecture seule

## 📝 Endpoints principaux

- `/api/employees` - Gestion des employés
- `/api/departments` - Gestion des départements
- `/api/positions` - Gestion des postes
- `/api/leaves` - Gestion des congés
- `/api/auth` - Authentification
