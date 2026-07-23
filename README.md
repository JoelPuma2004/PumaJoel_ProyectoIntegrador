# Panel administrativo de Joel

## Ejecución nativa

Requiere el SDK de .NET 10. No usa Docker ni carga datos seed.

```bash
make restore
make build
make run
```

El panel queda disponible en `http://localhost:5203`. Para usar otro puerto:

```bash
make run PORT=5300
```

Por defecto consume estas APIs locales:

- Guacales: `http://localhost:18080/demo/api/v1/`
- UTNGolCoin: `http://localhost:5001/api/`

Se pueden reemplazar sin editar archivos:

```bash
ApiEstadisticas__BaseUrl=http://localhost:18080/demo/api/v1/ \
ApiUTNGolCoin__BaseUrl=http://localhost:5001/api/ \
make run
```

Las credenciales locales de demostración son
`admin@utn.edu.ec` / `Admin123!`. También admiten variables de entorno:

```bash
AdminDemo__Usuario=admin-local \
AdminDemo__Contrasena='otra-clave' \
make run
```

La carga manual de partidos está limitada a los octavos de final
(números FIFA 89 a 96). Los listados y cambios dependen siempre de las APIs
reales; si una no responde, el panel muestra el estado no disponible.
