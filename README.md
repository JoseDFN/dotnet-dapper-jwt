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

# IMPORTANTE: Ejecutar scripts adicionales requeridos
psql -U postgres -d netdapperjwt -f scripts/DML.sql
psql -U postgres -d netdapperjwt -f scripts/procedures.sql
```

#### Opción 2: Usando Scripts SQL (Alternativo)

Si prefieres usar los scripts SQL directamente, copia y ejecuta estos scripts en tu herramienta de PostgreSQL (pgAdmin, DBeaver, psql, etc.):

**Script 1: Crear base de datos y tablas**
```sql
CREATE DATABASE netdapperjwt;
\c netdapperjwt;

CREATE TABLE roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) UNIQUE NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(100) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role_id INT NOT NULL,
    refresh_token VARCHAR(500),
    refresh_token_expires_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_users_roles FOREIGN KEY (role_id) REFERENCES roles(id) ON DELETE CASCADE
);

CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    sku VARCHAR(50) UNIQUE NOT NULL,
    price NUMERIC(12,2) NOT NULL,
    stock INT NOT NULL DEFAULT 0,
    category VARCHAR(50),
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE orders (
    id SERIAL PRIMARY KEY,
    user_id INT NOT NULL,
    total NUMERIC(12,2) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_orders_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE TABLE order_items (
    id SERIAL PRIMARY KEY,
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    unit_price NUMERIC(12,2) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_orderitems_orders FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    CONSTRAINT fk_orderitems_products FOREIGN KEY (product_id) REFERENCES products(id) ON DELETE CASCADE
);

CREATE INDEX idx_users_role_id ON users(role_id);
CREATE INDEX idx_users_refresh_token ON users(refresh_token);
CREATE INDEX idx_orders_user_id ON orders(user_id);
CREATE INDEX idx_order_items_order_id ON order_items(order_id);
CREATE INDEX idx_order_items_product_id ON order_items(product_id);
```

**Script 2: Insertar roles iniciales (REQUERIDO)**
```sql
\c netdapperjwt;

INSERT INTO roles (name, created_at, updated_at)
VALUES ('Admin', NOW(), NOW());

INSERT INTO roles (name, created_at, updated_at)
VALUES ('User', NOW(), NOW());
```

**Script 3: Crear funciones almacenadas (REQUERIDO)**
```sql
\c netdapperjwt;

-- Función: auth_user
CREATE OR REPLACE FUNCTION auth_user(p_username VARCHAR, p_password_hash VARCHAR)
RETURNS TABLE(user_id INT, role_name VARCHAR) AS $$
BEGIN
    RETURN QUERY
    SELECT u.id, r.name
    FROM users u
    JOIN roles r ON u.role_id = r.id
    WHERE u.username = p_username
      AND u.password_hash = p_password_hash;
END;
$$ LANGUAGE plpgsql;

-- Función: create_order
CREATE OR REPLACE FUNCTION create_order(p_user_id INT, p_items JSON)
RETURNS INT AS $$
DECLARE
    v_order_id INT;
    v_total NUMERIC(12,2) := 0;
    v_item JSON;
    v_product_id INT;
    v_quantity INT;
    v_unit_price NUMERIC(12,2);
    v_current_stock INT;
BEGIN
    -- Crear la orden inicial
    INSERT INTO orders (user_id, total, created_at, updated_at)
    VALUES (p_user_id, 0, NOW(), NOW())
    RETURNING id INTO v_order_id;

    -- Procesar cada item
    FOR v_item IN SELECT * FROM json_array_elements(p_items)
    LOOP
        v_product_id := (v_item->>'product_id')::INT;
        v_quantity := (v_item->>'quantity')::INT;
        v_unit_price := (v_item->>'unit_price')::NUMERIC;

        -- Validar producto y stock
        SELECT stock INTO v_current_stock
        FROM products
        WHERE id = v_product_id
        FOR UPDATE;

        IF v_current_stock IS NULL THEN
            RAISE EXCEPTION 'Product with id % not found', v_product_id;
        END IF;

        IF v_current_stock < v_quantity THEN
            RAISE EXCEPTION 'Insufficient stock for product id % (available: %, requested: %)',
                v_product_id, v_current_stock, v_quantity;
        END IF;

        -- Insertar item y actualizar stock
        INSERT INTO order_items (order_id, product_id, quantity, unit_price, created_at, updated_at)
        VALUES (v_order_id, v_product_id, v_quantity, v_unit_price, NOW(), NOW());

        UPDATE products
        SET stock = stock - v_quantity, updated_at = NOW()
        WHERE id = v_product_id;

        v_total := v_total + (v_quantity * v_unit_price);
    END LOOP;

    -- Actualizar total de la orden
    UPDATE orders
    SET total = v_total, updated_at = NOW()
    WHERE id = v_order_id;

    RETURN v_order_id;
END;
$$ LANGUAGE plpgsql;
```

**⚠️ Importante**: Los scripts `DML.sql` y `procedures.sql` son **obligatorios** para el correcto funcionamiento de la aplicación:

#### Scripts Requeridos:
- **`DML.sql`**: Crea los roles iniciales
  - Admin (id=1) - Para gestión de productos
  - User (id=2) - Para usuarios regulares
- **`procedures.sql`**: Crea funciones PostgreSQL esenciales
  - `auth_user()` - Autenticación de usuarios
  - `create_order()` - Creación de órdenes con validación de stock

### 3. Configurar la conexión

Actualizar el archivo `appsettings.Development.json` con tu cadena de conexión:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=netdapperjwt;Username=postgres;Password=tu_password"
  }
}
```

### 4. Ejecutar la aplicación

#### Opción 1: Solo HTTP (Desarrollo rápido)
```bash
dotnet restore
dotnet run --project ApiDotnetDapperJwt
```
**URLs disponibles:**
- API: `http://localhost:5092`
- Swagger: `http://localhost:5092/swagger`

#### Opción 2: HTTP + HTTPS (Recomendado)
```bash
dotnet restore
dotnet run --project ApiDotnetDapperJwt --launch-profile https
```
**URLs disponibles:**
- API HTTP: `http://localhost:5092`
- API HTTPS: `https://localhost:7025`
- Swagger HTTP: `http://localhost:5092/swagger`
- Swagger HTTPS: `https://localhost:7025/swagger`

> **Nota:** La opción 2 elimina la advertencia de redirección HTTPS y proporciona mayor seguridad.

> **⚠️ Problema con certificado SSL:** Si encuentras errores de certificado SSL en desarrollo, puedes:
> 1. **Confiar en el certificado de desarrollo:**
>    ```bash
>    dotnet dev-certs https --trust
>    ```
> 2. **O usar solo HTTP** (Opción 1) para desarrollo local.

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
