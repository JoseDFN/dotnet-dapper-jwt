\c netdapperjwt;

INSERT INTO roles (name, created_at, updated_at)
VALUES ('Admin', NOW(), NOW());

INSERT INTO roles (name, created_at, updated_at)
VALUES ('User', NOW(), NOW());

-- Insert sample products for testing
INSERT INTO products (name, sku, price, stock, category, created_at, updated_at)
VALUES 
    ('Laptop Gaming', 'LAP001', 5500.00, 10, 'Electronics', NOW(), NOW()),
    ('Mouse Inalámbrico', 'MOU001', 25.00, 50, 'Electronics', NOW(), NOW()),
    ('Teclado Mecánico', 'KEY001', 150.00, 30, 'Electronics', NOW(), NOW()),
    ('Monitor 24"', 'MON001', 300.00, 15, 'Electronics', NOW(), NOW()),
    ('Auriculares', 'AUD001', 80.00, 25, 'Electronics', NOW(), NOW());