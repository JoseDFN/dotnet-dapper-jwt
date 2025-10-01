# API .NET con Dapper y JWT

Una API REST desarrollada en .NET 9 que implementa autenticación JWT, gestión de usuarios, productos y órdenes utilizando Dapper como ORM y PostgreSQL como base de datos.

## 🚀 Características

- **Autenticación JWT**: Sistema de autenticación seguro con tokens JWT
- **Arquitectura Clean**: Separación en capas (Domain, Application, Infrastructure, API)
- **Dapper ORM**: Acceso a datos eficiente con Dapper
- **PostgreSQL**: Base de datos robusta y escalable
- **Swagger/OpenAPI**: Documentación automática de la API
- **BCrypt**: Encriptación segura de contraseñas
- **Unit of Work**: Patrón para gestión de transacciones
- **Roles y Autorización**: Sistema de roles (Admin, User)

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
- Visual Studio 2022 o VS Code

## 🛠️ Instalación

### 1. Clonar el repositorio
```bash
git clone <url-del-repositorio>
cd dotnet-dapper-jwt
```

### 2. Configurar la base de datos

Ejecutar los scripts SQL en PostgreSQL:

```bash
# Crear la base de datos y tablas
psql -U postgres -f scripts/tables.sql

# Crear procedimientos almacenados
psql -U postgres -d netdapperjwt -f scripts/procedures.sql
```

### 3. Configurar la conexión

Actualizar el archivo `appsettings.json` con tu cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=netdapperjwt;Username=postgres;Password=tu_password"
  }
}
```

### 4. Restaurar dependencias y ejecutar

```bash
dotnet restore
dotnet run --project ApiDotnetDapperJwt
```

La API estará disponible en: `https://localhost:7000` (HTTPS) o `http://localhost:5000` (HTTP)

## 📚 Documentación de la API

Una vez ejecutada la aplicación, puedes acceder a la documentación Swagger en:
- **Swagger UI**: `https://localhost:7000/swagger`

## 🔐 Autenticación

### Registro de Usuario
```http
POST /api/users
Content-Type: application/json

{
  "username": "usuario123",
  "password": "password123",
  "roleId": 1
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
  "username": "usuario123",
  "role": "user"
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

### Autenticación
- `POST /api/auth/login` - Iniciar sesión (público)

## 🗄️ Modelo de Datos

### Entidades Principales

**User**
- `Id`: Identificador único
- `Username`: Nombre de usuario único
- `PasswordHash`: Contraseña encriptada con BCrypt
- `RoleId`: Referencia al rol del usuario

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

## 🧪 Testing

Para probar la API, puedes usar:

1. **Swagger UI**: Interfaz web interactiva
2. **Postman**: Colección de requests
3. **Archivo HTTP**: `ApiDotnetDapperJwt.http` incluido en el proyecto

## 📝 Ejemplo de Uso Completo

```bash
# 1. Crear usuario
curl -X POST "https://localhost:7000/api/users" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123", "roleId": 1}'

# 2. Login
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "admin123"}'

# 3. Crear producto (con token)
curl -X POST "https://localhost:7000/api/products" \
  -H "Authorization: Bearer TU_TOKEN_AQUI" \
  -H "Content-Type: application/json" \
  -d '{"name": "Laptop", "sku": "LAP001", "price": 1500.00, "stock": 10, "category": "Electrónicos"}'
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

**Nota**: Asegúrate de cambiar las credenciales JWT y de base de datos antes de usar en producción.
