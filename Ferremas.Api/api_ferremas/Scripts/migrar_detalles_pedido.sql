-- Migrar datos de detalles_pedido a pedido_items
INSERT INTO pedido_items (pedido_id, producto_id, cantidad, precio_unitario, subtotal)
SELECT pedido_id, producto_id, cantidad, precio_unitario, subtotal
FROM detalles_pedido;

-- Verificar la migración
SELECT 'Detalles migrados:' as mensaje;
SELECT COUNT(*) as total_migrados FROM pedido_items;

-- Mostrar los datos migrados
SELECT 
    pi.id,
    pi.pedido_id,
    p.nombre as nombre_producto,
    pi.cantidad,
    pi.precio_unitario,
    pi.subtotal
FROM pedido_items pi
JOIN productos p ON pi.producto_id = p.id; 