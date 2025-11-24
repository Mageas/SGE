Il faut installer dotnet-ef
```sh
dotnet tool install --global dotnet-ef  --version 9.0.10
```

Appliquer la migration
```sh
dotnet ef database update --project SGE.Infrastructure --startup-project SGE.API
```

Supprimer la BDD
```sh
dotnet ef database drop --project SGE.Infrastructure --startup-project SGE.API
```
