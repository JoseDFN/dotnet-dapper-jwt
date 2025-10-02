# API .NET con Dapper y JWT

Una API REST desarrollada en .NET 9 que implementa autenticación JWT con refresh tokens, gestión de usuarios, productos y órdenes utilizando Dapper como ORM y PostgreSQL como base de datos.

## 🚀 Características

- **Autenticación JWT**: Sistema de autenticación seguro con tokens JWT y refresh tokens
- **Arquitectura Clean**: Separación en capas (Domain, Application, Infrastructure, API)
- **Entity Framework Core**: Para migraciones y estructura de base de datos
- **Dapper ORM**: Acceso a datos eficiente con Dapper para consultas complejas
- **PostgreSQL**: Base de datos robusta y escalable
- **Swagger/OpenAPI**: Documentación automática de la API con autenticación integrada
- **BCrypt**: Encriptación segura de contraseñas
- **Unit of Work**: Patrón para gestión de transacciones
- **Roles y Autorización**: Sistema de roles (Admin, User)
- **Procedimientos Almacenados**: Funciones PostgreSQL para operaciones complejas

## 🏗️ Arquitectura del Proyecto

```
dotnet-dapper-jwt/
├── ApiDotnetDapperJwt/          # Capa de presentación (API)
├── Application/                 # Capa de aplicación (DTOs, Interfaces)
├── Domain/                      # Capa de dominio (Entidades)
├── Infrastructure/              # Capa de infraestructura (Datos, Servicios)
└── scripts/                     # Scripts SQL (Tablas y procedimientos)
```

## 📋 Requisitos

- .NET 9.0 SDK
- PostgreSQL 12+
- Visual Studio 2022, VS Code o cualquier editor compatible con .NET

## 🛠️ Instalación

### 1. Clonar el repositorio
```bash
git clone https://github.com/JoseDFN/dotnet-dapper-jwt
cd dotnet-dapper-jwt
```

### 2. Configurar la base de datos

#### Opción 1: Usando Entity Framework (Recomendado)

El proyecto utiliza Entity Framework Core con migraciones para la gestión de la base de datos. Las migraciones se encuentran en `Infrastructure/Data/Migrations/`:

```bash
# Crear una nueva migración (si no existe)
dotnet ef migrations add InitialCreate -p Infrastructure -s ApiDotnetDapperJwt -o Data/Migrations

# Aplicar las migraciones a la base de datos
dotnet ef database update -p Infrastructure -s ApiDotnetDapperJwt
```

#### Opción 2: Usando Scripts SQL (Alternativo)

Si prefieres usar los scripts SQL directamente:

```bash
# Crear la base de datos y tablas
psql -U postgres -f scripts/tables.sql

# Crear funciones almacenadas (procedimientos)
psql -U postgres -d netdapperjwt -f scripts/procedures.sql
```

### 3. Configurar la conexión

Actualizar el archivo `appsettings.json` con tu cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=netdapperjwt;Username=postgres;Password=tu_password"
  }
}
```

### 4. Ejecutar la aplicación
```bash
dotnet restore
dotnet run --project ApiDotnetDapperJwt
```

**URLs disponibles:**
- HTTP: `http://localhost:5092`
- HTTPS: `https://localhost:7025`
- Swagger: `http://localhost:5092/swagger`

## 🚀 Primer Usuario - Guía Rápida

### Paso 1: Crear tu primer usuario
```bash
curl -X POST "http://localhost:5092/api/users" \
  -H "Content-Type: application/json" \
  -d '{"username": "mi_usuario", "password": "mi_password123"}'
```

### Paso 2: Iniciar sesión
```bash
curl -X POST "http://localhost:5092/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "mi_usuario", "password": "mi_password123"}'
```

### Paso 3: Usar Swagger (Recomendado)
1. Abre `http://localhost:5092/swagger` en tu navegador
2. Haz clic en "Authorize" (🔒)
3. Pega tu token JWT en el campo "Value"
4. ¡Explora la API interactivamente!

### Paso 4: Crear un producto (requiere rol Admin)
Para crear productos necesitas un usuario con rol Admin. Puedes:
1. Modificar directamente la base de datos: `UPDATE users SET role_id = 1 WHERE username = 'mi_usuario';`
2. O crear un nuevo usuario con `roleId: 1` en el registro

## 📚 Documentación de la API

Una vez ejecutada la aplicación, puedes acceder a la documentación Swagger en:
- **Swagger UI**: `http://localhost:5092/swagger`

## 🔐 Autenticación

### Registro de Admin
```http
POST /api/users
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123",
  "roleId": 1
}
```

### Registro de Usuario
```http
POST /api/users
Content-Type: application/json

{
  "username": "usuario123",
  "password": "password123",
  "roleId": 2
}
```

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "usuario123",
  "password": "password123"
}
```

**Respuesta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "refresh_token_aqui",
  "username": "usuario123",
  "role": "user"
}
```

### Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "refresh_token_aqui"
}
```

### Revocar Token
```http
POST /api/auth/revoke
Content-Type: application/json

{
  "refreshToken": "refresh_token_aqui"
}
```

### Uso del Token
Incluir el token en el header Authorization:
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 🛍️ Endpoints Principales

### Usuarios
- `POST /api/users` - Crear usuario (público)
- `GET /api/users/{id}` - Obtener usuario por ID (requiere autenticación)

### Productos
- `GET /api/products` - Listar productos (público)
  - Query params: `?category=ropa&name=camisa`
- `POST /api/products` - Crear producto (requiere rol Admin)
- `PUT /api/products/{id}` - Actualizar producto (requiere rol Admin)
- `DELETE /api/products/{id}` - Eliminar producto (requiere rol Admin)

### Órdenes
- `POST /api/orders` - Crear orden (requiere autenticación)
- `GET /api/orders` - Listar órdenes del usuario (requiere autenticación)
- `GET /api/orders/{id}` - Obtener orden por ID (requiere autenticación)

### Autenticación
- `POST /api/auth/login` - Iniciar sesión (público)
- `POST /api/auth/refresh` - Renovar token (público)
- `POST /api/auth/revoke` - Revocar token (público)

## 🗄️ Modelo de Datos

### Entidades Principales

**User**
- `Id`: Identificador único
- `Username`: Nombre de usuario único
- `PasswordHash`: Contraseña encriptada con BCrypt
- `RoleId`: Referencia al rol del usuario
- `RefreshToken`: Token para renovar JWT
- `RefreshTokenExpiresAt`: Fecha de expiración del refresh token

**Role**
- `Id`: Identificador único
- `Name`: Nombre del rol (Admin, User)

**Product**
- `Id`: Identificador único
- `Name`: Nombre del producto
- `Sku`: Código único del producto
- `Price`: Precio del producto
- `Stock`: Cantidad en inventario
- `Category`: Categoría del producto

**Order**
- `Id`: Identificador único
- `UserId`: Referencia al usuario
- `Total`: Total de la orden
- `CreatedAt`: Fecha de creación

**OrderItem**
- `Id`: Identificador único
- `OrderId`: Referencia a la orden
- `ProductId`: Referencia al producto
- `Quantity`: Cantidad del producto
- `UnitPrice`: Precio unitario

## 🔧 Configuración JWT

El JWT está configurado en `appsettings.json`:

```json
{
  "JWT": {
    "Key": "iavW6pwHvU7mREst3toTNxaJUZCM1gHqtGck/tbasT4=",
    "Issuer": "ApiHabita",
    "Audience": "ApiHabitaUsers",
    "DurationInMinutes": 30
  }
}
```

## 🚀 Funcionalidades Avanzadas

### Procedimientos Almacenados

**auth_user(username, password_hash)**
- Valida credenciales y retorna user_id y role_name

**create_order(user_id, items_json)**
- Crea una orden completa con validación de stock
- Procesa múltiples items desde JSON
- Actualiza automáticamente el stock de productos

### Unit of Work Pattern
- Gestión centralizada de transacciones
- Repositorios por entidad
- Operaciones atómicas

### Arquitectura Híbrida EF + Dapper
- **Entity Framework Core**: Para migraciones y estructura de base de datos
- **Dapper**: Para consultas complejas y operaciones de alto rendimiento
- **DbContext**: Configurado para migraciones pero no usado en runtime
- **IDbConnection**: Registrado para uso directo con Dapper en los repositorios
- **Migraciones**: Ubicadas en `Infrastructure/Data/Migrations/`

## 🧪 Testing

Para probar la API, puedes usar:

1. **Swagger UI**: Interfaz web interactiva
2. **Postman**: Colección de requests
3. **Archivo HTTP**: `ApiDotnetDapperJwt.http` incluido en el proyecto

## 📝 Ejemplo de Uso Completo

```bash
# 1. Crear usuario (rol User por defecto)
curl -X POST "http://localhost:5092/api/users" \
  -H "Content-Type: application/json" \
  -d '{"username": "usuario123", "password": "password123"}'

# 2. Login
curl -X POST "http://localhost:5092/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "usuario123", "password": "password123"}'

# 3. Crear producto (requiere rol Admin)
curl -X POST "http://localhost:5092/api/products" \
  -H "Authorization: Bearer TU_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{"name": "Laptop", "sku": "LAP001", "price": 1500.00, "stock": 10, "category": "Electrónicos"}'

# 4. Crear orden
curl -X POST "http://localhost:5092/api/orders" \
  -H "Authorization: Bearer TU_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{"items": [{"productId": 1, "quantity": 2}]}'
```

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto está bajo la Licencia MIT. Ver el archivo `LICENSE` para más detalles.

## 👨‍💻 Autor

Desarrollado como parte de la prueba técnica para Backend Developer.

---

## 🔄 Gestión de Migraciones (Opcional)

Este proyecto usa **Entity Framework solo para migraciones**. El runtime usa **Dapper** para todas las operaciones de base de datos.

### Si necesitas modificar la estructura de la base de datos:

```bash
# Crear nueva migración
dotnet ef migrations add NombreDeLaMigracion -p Infrastructure -s ApiDotnetDapperJwt -o Data/Migrations

# Aplicar migración
dotnet ef database update -p Infrastructure -s ApiDotnetDapperJwt

# Ver migraciones aplicadas
dotnet ef migrations list -p Infrastructure -s ApiDotnetDapperJwt
```

### Para desarrollo rápido:
Usa los scripts SQL directamente en `scripts/` para modificar la base de datos.

---

**⚠️ Importante**: 
- Cambia las credenciales JWT y de base de datos antes de usar en producción
- Los roles por defecto son: Admin (id=1) y User (id=2)
- El proyecto está configurado para usar `appsettings.Development.json` en desarrollo
