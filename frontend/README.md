# Frontend React

Este directorio contiene una aplicación React construida con Vite que consume los endpoints del API Gateway.

Se organiza de forma sencilla siguiendo principios de **arquitectura limpia**:

- `src/api` contiene los servicios de acceso a datos.
- `src/contexts` mantiene el estado de autenticación y roles.
- `src/components` expone componentes reutilizables como `Navbar` y `PrivateRoute`.
- `src/pages` incluye las páginas de la aplicación.

La interfaz es dinámica según el rol obtenido desde el token JWT. Sólo el rol **Administrador** puede acceder a la gestión de usuarios.

Para desarrollar:

```bash
npm install  # (solo la primera vez)
npm run dev  # levanta Vite en el puerto 5173
```

La URL del API Gateway se toma de forma relativa (`/`), por lo que la aplicación puede servirse detrás del mismo dominio.
