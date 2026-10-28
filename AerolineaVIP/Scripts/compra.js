jQuery(function ($) {
    'use strict';
    var cantidad = Number($('#seleccionCompra').data('cantidad'));
    $('.tramo-tab').on('click', function () {
        $('.tramo-tab').removeClass('active').attr('aria-selected', 'false');
        $(this).addClass('active').attr('aria-selected', 'true');
        $('.panel-tramo').hide().filter('[data-tramo="' + $(this).data('tramo') + '"]').show();
    }).first().trigger('click');
    $('.panel-tramo').each(function () {
        var panel = $(this), valores = panel.find('input[name="asientos"]').val().split(',').filter(Boolean);
        valores = valores.filter(function (numero) { return panel.find('.asiento-phenom:not(:disabled)[data-numero="' + numero + '"]').length > 0; });
        panel.data('seleccion', valores);
        panel.find('input[name="asientos"]').val(valores.join(','));
    });
    $('.asiento-phenom:not(:disabled)').on('click', function () {
        var panel = $(this).closest('.panel-tramo'), lista = panel.data('seleccion');
        if (!lista) return;
        var numero = String($(this).data('numero')), indice = lista.indexOf(numero);
        if (indice >= 0) lista.splice(indice, 1);
        else if (lista.length < cantidad) lista.push(numero);
        else { $('#avisoAsientos').text('Ya seleccionó todos los asientos de este tramo.'); return; }
        $(this).toggleClass('seleccionado', indice < 0).attr('aria-pressed', indice < 0 ? 'true' : 'false');
        panel.find('input[name="asientos"]').val(lista.join(','));
        $('#avisoAsientos').text('Seleccionados en este tramo: ' + lista.length + ' de ' + cantidad);
    });
    $('#formAsientos').on('submit', function (evento) {
        var completo = true;
        $('.panel-tramo').each(function () { if ($(this).data('seleccion').length !== cantidad) completo = false; });
        if (!completo) { evento.preventDefault(); $('#avisoAsientos').text('Seleccione todos los asientos en cada tramo.'); }
    });
    $('.tipo-documento').on('change', function () {
        var cedula = $(this).val() === 'CEDULA', documento = $(this).closest('.pasajero-compra').find('.documento');
        documento.attr('maxlength', cedula ? 10 : 20).attr('pattern', cedula ? '[0-9]{10}' : '[A-Za-z0-9]{5,20}');
        documento.attr('title', cedula ? 'Cédula: 10 dígitos.' : 'Pasaporte: 5 a 20 letras o números.');
        if ($.validator) documento.rules('add', { documentoCompra: true });
    });
    if ($.validator) $.validator.addMethod('documentoCompra', function (valor, elemento) {
        return $(elemento).closest('.pasajero-compra').find('.tipo-documento').val() === 'CEDULA'
            ? /^[0-9]{10}$/.test(valor) : /^[A-Za-z0-9]{5,20}$/.test(valor);
    }, 'Revise la longitud y el formato del documento.');
    $('.tipo-documento').trigger('change');
    function longitudTarjeta() {
        var marca = $('[name="Marca"]:checked').val(), longitud = marca === 'AMEX' ? 15 : marca === 'DINERS' ? 14 : 16;
        $('#Numero').attr('maxlength', longitud).attr('pattern', '[0-9]{' + longitud + '}');
        var longitudCodigo = marca === 'AMEX' ? 4 : 3;
        $('#CodigoSeguridad').attr('maxlength', longitudCodigo).attr('pattern', '[0-9]{' + longitudCodigo + '}');
        $('#longitudTarjeta').text(marca + ': ' + longitud + ' dígitos. Use una tarjeta ficticia.');
    }
    $('[name="Marca"]').on('change', longitudTarjeta);
    if ($('[name="Marca"]:checked').length) longitudTarjeta();
    $('.solo-digitos').on('input', function () { this.value = this.value.replace(/[^0-9]/g, '').slice(0, Number(this.maxLength)); });
    $('#Vencimiento').on('input', function () {
        var digitos = this.value.replace(/[^0-9]/g, '').slice(0, 4);
        this.value = digitos.length > 2 ? digitos.slice(0, 2) + '/' + digitos.slice(2) : digitos;
    });
    var hoy = new Date(), fechaHoy = hoy.getFullYear() + '-' + ('0' + (hoy.getMonth() + 1)).slice(-2) + '-' + ('0' + hoy.getDate()).slice(-2);
    $('.fecha-nacimiento').attr('max', fechaHoy);
    if ($.validator) {
        $.validator.addMethod('vencimientoCompra', function (valor) {
            if (!/^(0[1-9]|1[0-2])\/[0-9]{2}$/.test(valor)) return false;
            return new Date(2000 + Number(valor.slice(3)), Number(valor.slice(0, 2)) - 1, 1) >= new Date(hoy.getFullYear(), hoy.getMonth(), 1);
        }, 'Ingrese un vencimiento MM/AA válido que no esté vencido.');
        $.validator.addMethod('codigoCompra', function (valor) {
            return $('[name="Marca"]:checked').val() === 'AMEX' ? /^[0-9]{4}$/.test(valor) : /^[0-9]{3}$/.test(valor);
        }, 'El código debe tener 3 dígitos, o 4 para AMEX.');
        $.validator.addMethod('numeroCompra', function (valor) {
            var marca = $('[name="Marca"]:checked').val(), longitud = marca === 'AMEX' ? 15 : marca === 'DINERS' ? 14 : 16;
            return /^[0-9]+$/.test(valor) && valor.length === longitud;
        }, 'Revise la longitud del número según la marca.');
        $.validator.addMethod('nacimientoCompra', function (valor) { return !valor || valor <= fechaHoy; }, 'La fecha de nacimiento no puede ser futura.');
        $('.fecha-nacimiento').each(function () { $(this).rules('add', { nacimientoCompra: true }); });
        if ($('#formPago').length) {
            $('#Vencimiento').rules('add', { required: true, vencimientoCompra: true });
            $('#CodigoSeguridad').rules('add', { required: true, codigoCompra: true });
            $('#Numero').rules('add', { required: true, numeroCompra: true });
        }
    }
    $('#abrirPago').on('click', function () {
        var formulario = document.getElementById('formPago');
        if (formulario.reportValidity() && $('#formPago').valid()) bootstrap.Modal.getOrCreateInstance(document.getElementById('modalPago')).show();
    });
    $('#formPago').on('submit', function (evento) {
        if (!this.checkValidity() || !$(this).valid()) { evento.preventDefault(); return; }
        $('#enviarPago').prop('disabled', true).text('Procesando simulación…');
    });
});
