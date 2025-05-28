-- Verificar todos los registros en detalles_pedido
SELECT 
    dp.id,
    dp.pedido_id,
    p.nombre as producto_nombre,
    dp.cantidad,
    dp.precio_unitario,
    dp.subtotal,
    ped.fecha_pedido,
    ped.estado
FROM detalles_pedido dp
JOIN productos p ON dp.producto_id = p.id
JOIN pedidos ped ON dp.pedido_id = ped.id
ORDER BY dp.pedido_id; 