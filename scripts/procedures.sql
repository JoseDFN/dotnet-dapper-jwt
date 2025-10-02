-- Función: auth_user
-- Valida credenciales (username + password_hash) y devuelve user_id y role_name si es válido.
CREATE OR REPLACE FUNCTION auth_user(p_username VARCHAR, p_password_hash VARCHAR)
RETURNS TABLE(user_id INT, role_name VARCHAR) AS $$
BEGIN
    -- Seleccionamos el id del usuario y el nombre del rol si coincide username y password_hash.
    -- Si no hay coincidencia, la función no devuelve filas (usar QueryFirstOrDefault/Query en Dapper).
    RETURN QUERY
    SELECT u.id, r.name
    FROM users u
    JOIN roles r ON u.role_id = r.id
    WHERE u.username = p_username
      AND u.password_hash = p_password_hash;
END;
$$ LANGUAGE plpgsql;


-- Función: create_order
-- Crea una orden para un usuario, inserta los order_items desde un JSON array,
-- decrementa el stock de cada producto y actualiza el total de la orden.
-- El JSON esperado es un array con objetos: [{ "product_id":1, "quantity":2, "unit_price":1500.00 }, ...]
CREATE OR REPLACE FUNCTION create_order(p_user_id INT, p_items JSON)
RETURNS INT AS $$
DECLARE
    -- Id de la orden creada
    v_order_id INT;
    -- Acumula el total calculado
    v_total NUMERIC(12,2) := 0;
    -- Variable para iterar cada elemento JSON
    v_item JSON;
    v_product_id INT;
    v_quantity INT;
    v_unit_price NUMERIC(12,2);
    -- Stock actual del producto (para validación/lock)
    v_current_stock INT;
BEGIN
    -- 1) Crear la orden inicial con total 0 (se actualizará al final)
    INSERT INTO orders (user_id, total, created_at, "Updated_At")
    VALUES (p_user_id, 0, NOW(), NOW())
    RETURNING id INTO v_order_id;

    -- 2) Iterar cada item en el JSON array
    FOR v_item IN SELECT * FROM json_array_elements(p_items)
    LOOP
        -- Extrae los campos desde el objeto JSON
        v_product_id := (v_item->>'product_id')::INT;
        v_quantity := (v_item->>'quantity')::INT;
        v_unit_price := (v_item->>'unit_price')::NUMERIC;

        -- Validaciones básicas:
        --   a) Verificar que el producto exista y tomar su stock con lock FOR UPDATE
        SELECT stock INTO v_current_stock
        FROM products
        WHERE id = v_product_id
        FOR UPDATE;

        IF v_current_stock IS NULL THEN
            -- Si no existe el producto, abortamos la operación completa
            RAISE EXCEPTION 'Product with id % not found', v_product_id;
        END IF;

        --   b) Verificar que haya stock suficiente
        IF v_current_stock < v_quantity THEN
            -- Abortamos la transacción si no hay stock suficiente
            RAISE EXCEPTION 'Insufficient stock for product id % (available: %, requested: %)',
                v_product_id, v_current_stock, v_quantity;
        END IF;

        -- 3) Insertar el item en order_items
        INSERT INTO order_items (order_id, product_id, quantity, unit_price, "Created_At", "Updated_At")
        VALUES (v_order_id, v_product_id, v_quantity, v_unit_price, NOW(), NOW());

        -- 4) Actualizar stock: descontar la cantidad vendida
        UPDATE products
        SET stock = stock - v_quantity, "Updated_At" = NOW()
        WHERE id = v_product_id;

        -- 5) Acumular el total
        v_total := v_total + (v_quantity * v_unit_price);
    END LOOP;

    -- 6) Actualizar el total de la orden y timestamp de actualización
    UPDATE orders
    SET total = v_total,
        "Updated_At" = NOW()
    WHERE id = v_order_id;

    -- 7) Devolver el id de la orden creada
    RETURN v_order_id;
END;
$$ LANGUAGE plpgsql;