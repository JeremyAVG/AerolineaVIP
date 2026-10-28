// Regla cliente para [DebeSerVerdadero]
$.validator.addMethod("debeserverdadero", function (value, element) {
    return $(element).is(":checked");
});
$.validator.unobtrusive.adapters.addBool("debeserverdadero");

// Regla cliente para [FechaNoAnteriorAHoy] (valor yyyy-mm-dd del input date)
$.validator.addMethod("fechanoanterior", function (value, element) {
    if (!value) {
        return true;
    }
    var hoy = new Date();
    var textoHoy = hoy.getFullYear() + "-" + ("0" + (hoy.getMonth() + 1)).slice(-2) + "-" + ("0" + hoy.getDate()).slice(-2);
    return value >= textoHoy;
});
$.validator.unobtrusive.adapters.addBool("fechanoanterior");
