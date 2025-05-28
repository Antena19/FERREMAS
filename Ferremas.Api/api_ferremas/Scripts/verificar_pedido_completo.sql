-- Verificar datos completos del pedido
SELECT 
    p.id as pedido_id,
    p.fecha_pedido,
    p.estado,
    p.tipo_entrega,
    p.subtotal,
    p.costo_envio,
    p.impuestos,
    p.total,
    p.notas,
    
    -- Datos del usuario
    u.id as usuario_id,
    u.nombre as usuario_nombre,
    u.apellido as usuario_apellido,
    u.email as usuario_email,
    
    -- Datos de la sucursal
    s.id as sucursal_id,
    s.nombre as sucursal_nombre,
    s.direccion as sucursal_direccion,
    
    -- Datos de la dirección de entrega
    d.id as direccion_id,
    d.calle,
    d.comuna,
    d.region,
    
    -- Items del pedido
    pi.id as item_id,
    pr.nombre as producto_nombre,
    pi.cantidad,
    pi.precio_unitario,
    pi.subtotal as item_subtotal
FROM pedidos p
JOIN usuarios u ON p.usuario_id = u.id
LEFT JOIN sucursales s ON p.sucursal_id = s.id
LEFT JOIN direcciones d ON p.direccion_id = d.id
LEFT JOIN pedido_items pi ON p.id = pi.pedido_id
LEFT JOIN productos pr ON pi.producto_id = pr.id
WHERE p.id = 1;  -- Pedido existente 