-- 1. Migrar todos los datos
INSERT INTO pedido_items (pedido_id, producto_id, cantidad, precio_unitario, subtotal)
SELECT pedido_id, producto_id, cantidad, precio_unitario, subtotal
FROM detalles_pedido;

-- 2. Verificar la migración
SELECT 'Verificación de datos migrados:' as mensaje;
SELECT 
    pi.id,
    pi.pedido_id,
    p.nombre as producto_nombre,
    pi.cantidad,
    pi.precio_unitario,
    pi.subtotal,
    ped.fecha_pedido,
    ped.estado
FROM pedido_items pi
JOIN productos p ON pi.producto_id = p.id
JOIN pedidos ped ON pi.pedido_id = ped.id
ORDER BY pi.pedido_id;

-- 3. Verificar totales
SELECT 'Verificación de totales:' as mensaje;
SELECT 
    pedido_id,
    COUNT(*) as total_items,
    SUM(subtotal) as subtotal_pedido
FROM pedido_items
GROUP BY pedido_id; 