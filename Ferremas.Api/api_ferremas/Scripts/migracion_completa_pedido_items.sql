-- 1. Crear tabla pedido_items
CREATE TABLE IF NOT EXISTS `pedido_items` (
  `id` int NOT NULL AUTO_INCREMENT,
  `pedido_id` int NOT NULL,
  `producto_id` int NOT NULL,
  `cantidad` int NOT NULL,
  `precio_unitario` decimal(10,2) NOT NULL,
  `subtotal` decimal(10,2) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `pedido_items_pedido_id_foreign` (`pedido_id`),
  KEY `pedido_items_producto_id_foreign` (`producto_id`),
  CONSTRAINT `pedido_items_pedido_id_foreign` FOREIGN KEY (`pedido_id`) REFERENCES `pedidos` (`id`) ON DELETE CASCADE,
  CONSTRAINT `pedido_items_producto_id_foreign` FOREIGN KEY (`producto_id`) REFERENCES `productos` (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- 2. Migrar todos los datos
INSERT INTO pedido_items (pedido_id, producto_id, cantidad, precio_unitario, subtotal)
SELECT pedido_id, producto_id, cantidad, precio_unitario, subtotal
FROM detalles_pedido;

-- 3. Verificar la migración
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

-- 4. Verificar totales
SELECT 'Verificación de totales:' as mensaje;
SELECT 
    pedido_id,
    COUNT(*) as total_items,
    SUM(subtotal) as subtotal_pedido
FROM pedido_items
GROUP BY pedido_id;

-- 5. Eliminar tabla detalles_pedido (comentado por seguridad)
-- DROP TABLE IF EXISTS detalles_pedido; 