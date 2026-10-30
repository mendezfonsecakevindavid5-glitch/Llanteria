// Lee un Excel (.xlsx) subido por el usuario y precarga el formulario de Producto con lo que encuentre.
// No guarda nada por sí solo: el usuario revisa los datos y presiona "Guardar" como siempre.
(function () {
    "use strict";

    var boton = document.getElementById("btnImportarExcel");
    var input = document.getElementById("archivoExcelProducto");
    var avisos = document.getElementById("avisosImportarExcel");
    var form = document.getElementById("formProducto");
    if (!boton || !input || !form) return;

    function mostrarAviso(texto, esError) {
        if (!avisos) return;
        avisos.textContent = texto || "";
        avisos.className = "small mt-1 " + (esError ? "text-danger" : "text-success");
    }

    function establecerValor(nombre, valor) {
        if (valor === null || valor === undefined || valor === "") return;
        var campo = form.querySelector('[name="' + nombre + '"]');
        if (!campo) return;
        campo.value = valor;
    }

    boton.addEventListener("click", function () {
        var archivo = input.files && input.files[0];
        if (!archivo) {
            mostrarAviso("Selecciona primero un archivo .xlsx.", true);
            return;
        }

        var token = form.querySelector('input[name="__RequestVerificationToken"]');
        var datosFormulario = new FormData();
        datosFormulario.append("archivo", archivo);
        if (token) datosFormulario.append("__RequestVerificationToken", token.value);

        boton.disabled = true;
        mostrarAviso("Leyendo archivo…", false);

        fetch("/Producto/ImportarExcel", { method: "POST", body: datosFormulario })
            .then(function (resp) { return resp.json().then(function (json) { return { ok: resp.ok, json: json }; }); })
            .then(function (resultado) {
                if (!resultado.ok || !resultado.json.ok) {
                    mostrarAviso(resultado.json.error || "No se pudo leer el archivo.", true);
                    return;
                }

                var d = resultado.json.datos;
                establecerValor("Nombre", d.nombre);
                establecerValor("Categoria", d.categoria);
                establecerValor("Descripcion", d.descripcion);
                establecerValor("IdProveedor", d.idProveedor);
                establecerValor("IdBodega", d.idBodega);
                establecerValor("PrecioCompra", d.precioCompra);
                establecerValor("PrecioVenta", d.precioVenta);
                establecerValor("DetalleProducto.IdMarca", d.idMarca);
                establecerValor("DetalleProducto.TipoVehiculo", d.tipoVehiculo);
                establecerValor("DetalleProducto.Ancho", d.ancho);
                establecerValor("DetalleProducto.Perfil", d.perfil);
                establecerValor("DetalleProducto.Diametro", d.diametro);
                establecerValor("DetalleProducto.GarantiaMeses", d.garantiaMeses);
                establecerValor("DetalleProducto.Viscosidad", d.viscosidad);
                establecerValor("DetalleProducto.TipoAceite", d.tipoAceite);
                establecerValor("DetalleProducto.Amperaje", d.amperaje);
                establecerValor("DetalleProducto.Polaridad", d.polaridad);
                establecerValor("DetalleProducto.Voltaje", d.voltaje);
                establecerValor("DetalleProducto.TecnologiaBateria", d.tecnologiaBateria);
                establecerValor("DetalleProducto.AplicacionLubricante", d.aplicacionLubricante);
                establecerValor("DetalleProducto.PresentacionLubricante", d.presentacionLubricante);
                establecerValor("DetalleProducto.TipoAccesorio", d.tipoAccesorio);
                establecerValor("DetalleProducto.UbicacionAccesorio", d.ubicacionAccesorio);

                var listaAvisos = resultado.json.avisos || [];
                mostrarAviso(listaAvisos.length ? ("Formulario precargado. " + listaAvisos.join(" ")) : "Formulario precargado correctamente.", listaAvisos.length > 0);
            })
            .catch(function () {
                mostrarAviso("No se pudo leer el archivo. Intenta de nuevo.", true);
            })
            .finally(function () {
                boton.disabled = false;
            });
    });
})();
