// Carga los destinos según el origen y evita envíos repetidos del formulario de búsqueda
$(function () {
    var $origen = $("#OrigenId");
    var $destino = $("#DestinoId");
    var $boton = $("#btnBuscar");

    $origen.on("change", function () {
        $destino.empty().append($("<option>").val("").text("Seleccionar destino"));
        if (!$origen.val()) {
            return;
        }
        $.getJSON($origen.data("url-destinos"), { origenId: $origen.val() }, function (destinos) {
            if (destinos.length === 0) {
                $destino.empty().append($("<option>").val("").text("Sin destinos disponibles"));
            }
            $.each(destinos, function (i, d) {
                $destino.append($("<option>").val(d.id).text(d.texto));
            });
            $boton.prop("disabled", destinos.length === 0);
        });
    });

    $("#formBusqueda").on("submit", function () {
        if ($(this).valid()) {
            $boton.prop("disabled", true).text("Buscando…");
        }
    });
});
