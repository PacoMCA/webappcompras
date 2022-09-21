select * from [tbl_solicitudes]

select * from cat_proveedores
select * from cat_Producto
select * from [tbl_OrdenCompra]
select * from tbl_OrdenCompra_CondPago
select * from tbl_cotizacionproducto

select ISNULL(max(id),1) as id from cat_Producto

select id, s_descripcion, s_RFC from cat_proveedores

select * from cat_proveedores WHERE s_RFC = 'MEM760401DJ7';
select * from cat_Producto 
where s_NombreProducto=' SERVICIO DE MANTENIMIENTO MENSUAL A ELEVADORES MARCA MITSUBISHI CORRESP ONDIENTE AL MES DE ABRIL DEL 2022'

update cat_Producto set s_NombreProducto='SERVICIO DE MANTENIMIENTO MENSUAL A ELEVADORES MARCA MITSUBISHI CORRESP ONDIENTE AL MES DE ABRIL DEL 2022'
where id=1

delete [tbl_OrdenCompra] where id=3
delete tbl_OrdenCompra_CondPago where id=3

insert into cat_Producto (s_ClaveEmpleado,s_NombreProducto,idCategoriaProducto,s_contenido,s_clavesat,s_Unidad,s_ClavePrincipal,s_Impuestos) 
values (,'ce2222',' SERVICIO DE MANTENIMIENTO MENSUAL A ELEVADORES MARCA MITSUBISHI CORRESP ONDIENTE AL MES DE ABRIL DEL 2022',1,'1','72101506','SR',
'WS0','16')




select * from cat_Producto
select ISNULL(max(id),1) as id from cat_Producto
select max(id) as id from cat_Producto

SELECT * FROM cat_Impuesto

select * from [tbl_solicitudes]
select * from tbl_cotizacionproducto

select * from [tbl_solicitudes] where idsolicitud='ce2222-201'
select * from tbl_OrdenCompra where s_FolioOrdenCompra='OC22158'
select * from tbl_OrdenCompra_CondPago where s_FolioOrdenCompra ='OC22158'
select * from tbl_cotizacionproducto where s_FolioOrdenCompra ='OC22158'

select idsolicitud,s_FolioOrdenCompra,s_Proveedor,s_Unidad,s_ClaveEmpleado,d_FechaEntrega,cancelaciones from tbl_OrdenCompra where s_FolioOrdenCompra='OC22158'
select s_FolioOrdenCompra,n_NumeroPago,d_FechaProgramada,s_Concepto,d_Monto,s_Penalizacion,d_PorcentajePenalizacion,s_ClaveEmpleado,b_FacturaGeneral,d_DescuentoTotal from tbl_OrdenCompra_CondPago where s_FolioOrdenCompra ='OC22158'
select idProducto,s_FolioOrdenCompra,idProveedor,n_Cantidad,d_PrecioUnitario,d_Descuento,d_IVA,d_PrecioTotal,d_Total,s_Observaciones,d_Retencion from tbl_cotizacionproducto where s_FolioOrdenCompra ='OC22158'

insert into cat_Producto (s_ClaveEmpleado,s_NombreProducto,idCategoriaProducto,s_contenido,s_clavesat,s_Unidad,s_ClavePrincipal,s_Impuestos) 
values ('ce2222',' SERVICIO DE MANTENIMIENTO MENSUAL A ELEVADORES MARCA MITSUBISHI CORRESP ONDIENTE AL MES DE ABRIL DEL 2022',1,'1','72101506','SR','WS1','16')

INSERT INTO tbl_OrdenCompra (idsolicitud,s_FolioOrdenCompra,s_Proveedor,s_Unidad,s_ClaveEmpleado,d_FechaEntrega,cancelaciones) 
VALUES ('ce2222-1','OC1','MITSUBISHI ELECTRIC DE MEXICO, S.A. DE C.V.','SR','ce2222','2022-09-02',1)

INSERT INTO tbl_OrdenCompra_CondPago (s_FolioOrdenCompra,n_NumeroPago,d_FechaProgramada,s_Concepto,d_Monto,s_Penalizacion,d_PorcentajePenalizacion,s_ClaveEmpleado,b_FacturaGeneral,d_DescuentoTotal)
VALUES ('OC1',1,'2022-09-02','PAGO',10841.28,'Ninguna',0,'ce2222',0,0)

INSERT INTO tbl_cotizacionproducto (IdCotizacion,idProducto,s_FolioOrdenCompra,idProveedor,n_Cantidad,d_PrecioUnitario,d_Descuento,d_IVA,d_PrecioTotal,d_Total,s_Observaciones,d_Retencion) 
VALUES ('22-1',1,'OC1',0,1,9345.93,0,16,10841.2788,9345.93,'SR', 0)

SELECT * FROM [tbl_OrdenCompra]
SELECT * FROM [tbl_OrdenCompra_CondPago]
--IDSOLICITUD,USUARIO,fecha,tipo solicitud, monto, fecha, empresa, etapa, responsable,proveedor
alter procedure sp_get_oc
as
begin
SELECT oc.idsolicitud as Id,'USUARIO PRUEBAS' AS Usuario, left(CONVERT(nvarchar,oc.d_FechaInsercionSistema, 22),14) as Dilacion, 'Compra express' as TipoSolicitud,
left(CONVERT(nvarchar,oc.d_FechaInsercionSistema, 22),14) as Fecha , 'PRUEBA CORPORATIVO MUÑOZ' as Empresa, 'Compra autorizada' as Etapa,
'Tesorería' as Responsable, oc.s_Proveedor as Proveedor, ocp.d_Monto as Monto FROM [tbl_OrdenCompra] oc
inner join [tbl_OrdenCompra_CondPago] ocp on oc.s_FolioOrdenCompra=ocp.s_FolioOrdenCompra
end

exec sp_get_oc

select CONVERT(nvarchar(30),getdate(), 120) as USAformat
select CONVERT(nvarchar(30),getdate(), 101) as USAformat
select CONVERT(nvarchar(30),getdate(), 22) as USAformat
select left(CONVERT(nvarchar,GETDATE(), 120),16) as fecha;
select left(CONVERT(nvarchar,GETDATE(), 22),14) as fecha;