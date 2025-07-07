# GestionCombustible

Este repositorio contiene los microservicios de autenticación y gestión de vehículos para el proyecto *GestionCombustible*. Ahora se añade un API Gateway REST que se apoya en **YARP** para exponer los servicios de forma unificada para un front-end.

## Microservicios existentes
- **AuthServiceEcoF**: Servicio de autenticación (puerto 8081/8080).
- **VehicleServiceEcoF**: Servicio de gestión de vehículos (puerto 8082).

## Nuevo API Gateway
El directorio `ApiGatewayEcoF` contiene un microservicio adicional construido con .NET 9. El gateway expone rutas REST que internamente consumen los microservicios gRPC y también actúa como proxy mediante YARP. Endpoints principales:

- `POST /auth/login` → login en `AuthServiceEcoF`.
- `POST /auth/register` → registro de usuario en `AuthServiceEcoF`.
- `GET /vehicle` → listado de vehículos desde `VehicleServiceEcoF`.
- `GET /vehicle/{id}` → detalle de un vehículo.

El gateway escucha en el puerto **8085** (configurable en `docker-compose.yml`).

Para construir la imagen Docker:

```bash
docker compose -f ApiGatewayEcoF/docker-compose.yml build
```

## Levantar todos los servicios

Desde la raíz del repositorio se puede iniciar la base de datos de autenticación,
el microservicio de vehículos y el API Gateway con un único comando:

```bash
docker compose up --build
```

Esto utiliza el archivo `docker-compose.yml` situado en la raíz.

## Estructura
- `AuthServiceEcoF/` – microservicio de autenticación.
- `VehicleServiceEcoF/` – microservicio de vehículos.
- `ApiGatewayEcoF/` – API Gateway REST que unifica los anteriores.
