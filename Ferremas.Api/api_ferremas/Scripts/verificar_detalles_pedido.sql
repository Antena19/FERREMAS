-- Verificar items en detalles_pedido
SELECT 
    dp.id,
    dp.pedido_id,
    p.nombre as producto_nombre,
    dp.cantidad,
    dp.precio_unitario,
    dp.subtotal
FROM detalles_pedido dp
JOIN productos p ON dp.producto_id = p.id
WHERE dp.pedido_id = 1; 