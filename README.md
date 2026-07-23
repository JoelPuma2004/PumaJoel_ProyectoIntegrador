# Panel administrativo de Joel

## Ejecución nativa

Requiere el SDK de .NET 10. No usa Docker ni carga datos seed.

## Windows con Visual Studio 2022

1. En `FrontendAdministrativo/appsettings.json`, reemplaza las URLs de
   `ApiEstadisticas:BaseUrl` y `ApiUTNGolCoin:BaseUrl` con las IP de Andrea y Mayra.
2. Abre `PumaJoel_ProyectoIntegrador.slnx`.
3. Selecciona `FrontendAdministrativo` como proyecto de inicio y el perfil
   **http** (`0.0.0.0:5203`).
4. Ejecuta con **F5** y permite el puerto `5203` en redes privadas.

## Linux

Después de editar las mismas dos URLs en `appsettings.json`:

```bash
dotnet restore FrontendAdministrativo/FrontendAdministrativo.csproj
dotnet run --project FrontendAdministrativo/FrontendAdministrativo.csproj \
  --no-launch-profile --urls http://0.0.0.0:5203
```

El panel queda accesible como `http://IP_DE_JOEL:5203`.

Las credenciales locales de demostración son
`admin@utn.edu.ec` / `Admin123!`.

La carga manual de partidos está limitada a los octavos de final
(números FIFA 89 a 96). Los listados y cambios dependen siempre de las APIs
reales; si una no responde, el panel muestra el estado no disponible.
