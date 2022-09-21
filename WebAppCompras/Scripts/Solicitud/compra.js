$(document).ready(function () {
    var listProductosOC=[];
    var listOC = [];
    var datos = {};
    //btnCancelarOC
    $("#btnCancelarOC").click(function () {
        $("#table_productos_express tbody").empty();
    });

    $("#btnXml").click(function () {
        var selectFile = ($("#fileUpload"))[0].files[0];
        var dataString = new FormData();
        dataString.append("fileUpload", selectFile);
        if (!selectFile) {
            alert('No hay nada');
            $("#tblEmisor tbody").empty();
            $("#tblReceptor tbody").empty();
            $("#tblXml tbody").empty();
            $("#tblProductos tbody").empty();
        }
        else {
            $.ajax({
                url: '/Home/LoadFile/',
                type: 'POST',
                data: dataString,
                contentType: false,
                dataType: "json",
                processData: false,
                async: false,
                success: function (data) {
                    datos = data;
                    console.log('<<<<<<<<<<<<<>>>>>>>>>>>>>>>>>>>>>>>')
                    console.log('datos del controlador!')
                    console.log(data);
                    $("#tblEmisor tbody").empty();
                    $("#tblReceptor tbody").empty();
                    $("#tblXml tbody").empty();
                    $("#tblProductos tbody").empty();
                    $.each(data, function (i, item) {
                        console.log(data);

                        var rowsE = "<tr class='text-center'>"
                            + "<td>" + item.RfcE + "</td>"
                            + "<td id='nombreE'>" + item.NombreE + "</td>"
                            + "<td>" + item.Regimen + "</td>"
                        "</tr>";
                        $("#tblEmisor tbody").append(rowsE);

                        var rowsR = "<tr class='text-center'>"
                            + "<td>" + item.RfcR + "</td>"
                            + "<td>" + item.NombreR + "</td>"
                            + "<td>" + item.UsoCfdi + "</td>"
                        "</tr>";
                        $("#tblReceptor tbody").append(rowsR);

                        var rows = "<tr>"
                            + "<td>" + item.Fecha + "</td>"
                            + "<td>" + item.Moneda + "</td>"
                            + "<td>$" + item.Subtotal + "</td>"
                            + "<td id='descuentoF'>$" + item.Descuento + "</td>"
                            + "<td>$" + item.TotalImpuestosTrasladados + "</td>"
                            + "<td id='totalF'>$" + item.Total + "</td>"
                            + "<td class='text-center'>" + item.Formapago + "</td>"
                            + "<td class='text-center'>" + item.Metodopago + "</td>"
                            + "<td>" + item.Serie + "</td>"
                            + "<td>" + item.Folio + "</td>"
                            + "<td class='text-center'>" + item.TipoDeComprobante + "</td>"
                        "</tr>";
                        $("#tblXml tbody").append(rows);
                        listProductosOC = item.Productos;

                        for (var i = 0; i < item.Productos.length; i++) {
                            var rowsP = "<tr>"
                                + "<td>" + item.Productos[i].ClaveProdServ + "</td>"
                                + "<td id='productoOC'>" + item.Productos[i].Descripcion+ "</td>"
                                + "<td class='text-center'>" + item.Productos[i].ClaveUnidad + "</td>"
                                + "<td class='text-center'>" + item.Productos[i].Unidad + "</td>"
                                + "<td class='text-center' id='cantidadOC'>" + item.Productos[i].Cantidad + "</td>"
                                + "<td>$" + item.Productos[i].Valorunitario + "</td>"
                                + "<td>$" + item.Productos[i].Importe + "</td>"
                                + "<td>$" + item.Productos[i].DescuentoProducto + "</td>"
                                + "<td>" + item.Productos[i].TasaOCuota + "%</td>"
                                + "<td>$" + item.Productos[i].ImporteImpuesto + "</td>"
                            "</tr>";
                            $("#tblProductos tbody").append(rowsP);
                        }

                        console.log(item.Productos)
                        console.log(item.Productos[0]['ClaveProdServ'])
                    });
                }
            });
        }
    });

    $("#btnOC").click(function () {
        $("#table_productos_express tbody").empty();
        $("#modal_generar_oc_express").modal("show");
        console.log(listProductosOC)
        var ims = "";
        var iv = "";
        var ttiv = 0;
        for (var i = 0; i < listProductosOC.length; i++) {
            var desp = 0;
            for (var z = 0; z < listProductosOC[i].Impuestos.length; z++) {
                if (listProductosOC[i].Impuestos[z]._Impuesto !== "IVA 16%") {
                    ims += "|" + listProductosOC[i].Impuestos[z]._Impuesto;
                    ttiv += listProductosOC[i].Impuestos[z].Importe;
                }
                else {
                    iv = listProductosOC[i].Impuestos[z].Tasa;
                    ttiv = ttiv - listProductosOC[i].Impuestos[z].Importe;
                }
            }
            if (ims === "") { ims = "0"; }
            desp = ((listProductosOC[i].Importe) - ttiv) - (listProductosOC[i].DescuentoProducto);
            
            var rowsP = "<tr>"
                + "<td>" + listProductosOC[i].Descripcion.trim() + "</td>"
                + "<td>" + listProductosOC[i].Cantidad + "</td>"
                + "<td>$" + listProductosOC[i].Valorunitario + "</td>"
                + "<td>" + iv + " %</td>"
                + "<td>$" + listProductosOC[i].DescuentoProducto + "</td>"
                + "<td>" + ims + "</td>"
                + "<td>" + listProductosOC[i].Importe + "</td>"
                + "<td>" + (desp.toFixed(2)) + "</td>"
                + "<td>" + listProductosOC[i].Unidad + "</td>"
            "</tr>";
            $("#table_productos_express tbody").append(rowsP);
        }
        var nombreP = $("#nombreE").text();
        $("#s_Proveedor").val(nombreP);
        $("#totalOC").val($("#totalF").text().replace("$", ""));
        $("#descuentoOC").val($("#descuentoF").text().replace("$", ""));
        $("#penalizacion").val("0");
    });

    $("#btnAgregarOC").click(function (el) {
        var tr = $("#trPagos").closest("tr");

        var tabla = $("#tbl_condiciones_pago_express").closest("table");
        var tr_tabla = $("#trPagos")
        console.log(tr)
        console.log(tabla)
        console.log(tr_tabla)
        //$("#tbl_condiciones_pago_express>tbody").append("<tr><td>Test Row Append</td></tr>");
        var ocd = {};
        $.each(tr.find("td"), function (k, e) {
            var td = "";
            var valor = $(e).find("input").val() || $(e).find("select").val();
            console.log("Valor de k", k)
            console.log("Valor de e", e)
            console.log(`agrega condiciones de pago`, valor);
            //console.log(valor);
            if (typeof valor != "undefined") {
                if (valor == "") {
                    vacio = true;
                    return false;
                }
                listOC.push(valor);
                td = "<td>" + valor + "</td>";
            } else {
                td = '<td><button data-toggle="tooltip" title="Eliminar adjunto" type="button" class="btn btn-danger fa fa-trash-alt" onclick="eliminarRegitroTabla(this)" ></button></td>';
            }
            $("#tbl_condiciones_pago_express>tbody").append(td)
            //tr_tabla.append(td);
            console.log(valor);
        })
        //console.log(listOC)
    });

    $("#btnGuardarOC").click(function () {
        console.log('Datos ->')
        console.log("Fecha OC: ", $("#fechaOC").val());
        var objOC = {
            "FechaEntrega": $("#fechaOC").val(),
            "FechaProgramada": listOC[0],
            "Concepto": listOC[1],
            "Penalizacion": listOC[2],
            "PorcentajePenalizacion": "0"
        }
        //console.log(objOC);
        $.ajax({
            type: 'POST',
            url: '/Home/generaOC/',
            content: "application/json; charset=utf-8",
//            dataType: "json",
            data: { f: datos.Data, oc: objOC },
            success: function (data) {
                window.location.href='/Home/Compra'
            }
        })
        
        //console.log(datos.Data.NombreE)
    });
   
});