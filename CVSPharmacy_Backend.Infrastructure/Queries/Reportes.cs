namespace CVSPharmacy_Backend.Infrastructure.Queries;

public static class Reportes
{
    public const string RankingProductosPorProveedorMySql = """
        WITH TotalesPorProducto AS (
            SELECT oc.proveedor_id, l.producto_id, SUM(l.cantidad_recibida) AS total
            FROM lotes AS l
            INNER JOIN recepciones AS r ON r.id = l.recepcion_id
            INNER JOIN ordenes_compra AS oc ON oc.id = r.orden_id
            GROUP BY oc.proveedor_id, l.producto_id
        ),
        Ranking AS (
            SELECT proveedor_id, producto_id, total,
                   ROW_NUMBER() OVER (
                       PARTITION BY proveedor_id ORDER BY total DESC, producto_id ASC
                   ) AS posicion
            FROM TotalesPorProducto
        )
        SELECT rk.proveedor_id AS ProveedorId, pv.razon_social AS RazonSocial,
               rk.posicion AS PosicionEnProveedor, rk.producto_id AS ProductoId,
               p.nombre AS NombreProducto, rk.total AS TotalUnidadesRecibidas
        FROM Ranking AS rk
        INNER JOIN proveedores AS pv ON pv.id = rk.proveedor_id
        INNER JOIN productos AS p ON p.id = rk.producto_id
        WHERE rk.posicion <= 3
        ORDER BY rk.proveedor_id, rk.posicion
        """;
}
