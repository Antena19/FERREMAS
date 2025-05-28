-- Crear tabla pedido_items si no existe
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

-- Migrar datos de detalles_pedido a pedido_items
INSERT INTO pedido_items (pedido_id, producto_id, cantidad, precio_unitario, subtotal)
SELECT pedido_id, producto_id, cantidad, precio_unitario, subtotal
FROM detalles_pedido
WHERE pedido_id = 1;

-- Verificar la migración
SELECT 
    pi.id,
    pi.pedido_id,
    p.nombre as producto_nombre,
    pi.cantidad,
    pi.precio_unitario,
    pi.subtotal
FROM pedido_items pi
JOIN productos p ON pi.producto_id = p.id
WHERE pi.pedido_id = 1; 