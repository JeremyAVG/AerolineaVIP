# Inventario de revisión Razor

Todas las vistas fuente se revisan, incluidas parciales, plantillas de Identity y área Admin. Los if/foreach de presentación, controles por rol y formatos de fecha/moneda no son cálculos de negocio. Importes, porcentajes, estados, elegibilidad, consultas y reglas se resuelven fuera de las vistas.

| Vista | Modelo | Revisión |
|---|---|---|
| AerolineaVIP/Areas/Admin/Views/_ViewStart.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Aeronaves/Create.cshtml | `Aeronave` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Aeronaves/Delete.cshtml | `Aeronave` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Aeronaves/Details.cshtml | `Aeronave` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Aeronaves/Edit.cshtml | `Aeronave` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Aeronaves/Index.cshtml | `IEnumerable<Aeronave>` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Aeropuertos/Create.cshtml | `Aeropuerto` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Aeropuertos/Delete.cshtml | `Aeropuerto` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Aeropuertos/Details.cshtml | `Aeropuerto` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Aeropuertos/Edit.cshtml | `Aeropuerto` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Aeropuertos/Index.cshtml | `IEnumerable<Aeropuerto>` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Catalogos/Index.cshtml | `Dictionary<string, List<KeyValuePair<string, string>>>` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Frecuencias/Create.cshtml | `Frecuencia` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Frecuencias/Delete.cshtml | `Frecuencia` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Frecuencias/Details.cshtml | `Frecuencia` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Frecuencias/Edit.cshtml | `Frecuencia` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Frecuencias/Index.cshtml | `IEnumerable<Frecuencia>` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Gestion/_DetalleReserva.cshtml | `Reserva` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Gestion/_PestanaPasajeros.cshtml | `GestionViewModel` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Gestion/_PestanaReservas.cshtml | `GestionViewModel` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Gestion/_PestanaUsuarios.cshtml | `GestionViewModel` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Gestion/EliminarReserva.cshtml | `Reserva` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Gestion/Index.cshtml | `GestionViewModel` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Gestion/Reserva.cshtml | `Reserva` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Itinerarios/Create.cshtml | `Itinerario` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Itinerarios/Delete.cshtml | `Itinerario` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Itinerarios/Details.cshtml | `Itinerario` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Itinerarios/Edit.cshtml | `Itinerario` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Itinerarios/Index.cshtml | `IEnumerable<Itinerario>` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Panel/Index.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Pasajeros/Create.cshtml | `Pasajero` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Pasajeros/Delete.cshtml | `Pasajero` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Pasajeros/Details.cshtml | `Pasajero` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Pasajeros/Edit.cshtml | `Pasajero` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Pasajeros/Index.cshtml | `IEnumerable<Pasajero>` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Reportes/_MenuReportes.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Reportes/Compras.cshtml | `ReporteComprasViewModel` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Reportes/Ocupacion.cshtml | `ReporteOcupacionViewModel` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Reportes/Reservas.cshtml | `ReporteReservasViewModel` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Rutas/Create.cshtml | `Ruta` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Rutas/Delete.cshtml | `Ruta` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Rutas/Details.cshtml | `Ruta` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Rutas/Edit.cshtml | `Ruta` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Rutas/Index.cshtml | `IEnumerable<Ruta>` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Shared/_MenuAdmin.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Areas/Admin/Views/TramosItinerario/Create.cshtml | `TramoItinerario` | Conforme |
| AerolineaVIP/Areas/Admin/Views/TramosItinerario/Delete.cshtml | `TramoItinerario` | Conforme |
| AerolineaVIP/Areas/Admin/Views/TramosItinerario/Details.cshtml | `TramoItinerario` | Conforme |
| AerolineaVIP/Areas/Admin/Views/TramosItinerario/Edit.cshtml | `TramoItinerario` | Conforme |
| AerolineaVIP/Areas/Admin/Views/TramosItinerario/Index.cshtml | `IEnumerable<TramoItinerario>` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Vuelos/Create.cshtml | `Vuelo` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Vuelos/Delete.cshtml | `Vuelo` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Vuelos/Details.cshtml | `Vuelo` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Vuelos/Edit.cshtml | `Vuelo` | Conforme |
| AerolineaVIP/Areas/Admin/Views/Vuelos/Index.cshtml | `FiltroVuelosViewModel` | Conforme |
| AerolineaVIP/Views/_ViewStart.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Views/Account/_ExternalLoginsListPartial.cshtml | `ExternalLoginListViewModel` | Conforme |
| AerolineaVIP/Views/Account/ConfirmEmail.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Views/Account/ExternalLoginConfirmation.cshtml | `ExternalLoginConfirmationViewModel` | Conforme |
| AerolineaVIP/Views/Account/ExternalLoginFailure.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Views/Account/ForgotPassword.cshtml | `ForgotPasswordViewModel` | Conforme |
| AerolineaVIP/Views/Account/ForgotPasswordConfirmation.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Views/Account/Login.cshtml | `LoginViewModel` | Conforme |
| AerolineaVIP/Views/Account/Register.cshtml | `RegisterViewModel` | Conforme |
| AerolineaVIP/Views/Account/ResetPassword.cshtml | `ResetPasswordViewModel` | Conforme |
| AerolineaVIP/Views/Account/ResetPasswordConfirmation.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Views/Account/SendCode.cshtml | `SendCodeViewModel` | Conforme |
| AerolineaVIP/Views/Account/VerifyCode.cshtml | `VerifyCodeViewModel` | Conforme |
| AerolineaVIP/Views/CheckIn/Index.cshtml | `BusquedaCheckInViewModel` | Conforme |
| AerolineaVIP/Views/Compra/_DetalleReserva.cshtml | `FinalCompra` | Conforme |
| AerolineaVIP/Views/Compra/Asientos.cshtml | `ResumenCompra` | Conforme |
| AerolineaVIP/Views/Compra/Comprobante.cshtml | `FinalCompra` | Conforme |
| AerolineaVIP/Views/Compra/Confirmar.cshtml | `ResumenCompra` | Conforme |
| AerolineaVIP/Views/Compra/Final.cshtml | `FinalCompra` | Conforme |
| AerolineaVIP/Views/Compra/Pagar.cshtml | `PagoCompra` | Conforme |
| AerolineaVIP/Views/Compra/Pasajeros.cshtml | `PasajerosCompra` | Conforme |
| AerolineaVIP/Views/Compra/VerReserva.cshtml | `FinalCompra` | Conforme |
| AerolineaVIP/Views/Comprobantes/Index.cshtml | `ComprobantesViewModel` | Conforme |
| AerolineaVIP/Views/Comprobantes/VistaPrevia.cshtml | `ComprobanteVista` | Conforme |
| AerolineaVIP/Views/Home/Index.cshtml | `HomeIndexViewModel` | Conforme |
| AerolineaVIP/Views/Manage/AddPhoneNumber.cshtml | `AddPhoneNumberViewModel` | Conforme |
| AerolineaVIP/Views/Manage/ChangePassword.cshtml | `ChangePasswordViewModel` | Conforme |
| AerolineaVIP/Views/Manage/Index.cshtml | `IndexViewModel` | Conforme |
| AerolineaVIP/Views/Manage/ManageLogins.cshtml | `ManageLoginsViewModel` | Conforme |
| AerolineaVIP/Views/Manage/SetPassword.cshtml | `SetPasswordViewModel` | Conforme |
| AerolineaVIP/Views/Manage/VerifyPhoneNumber.cshtml | `VerifyPhoneNumberViewModel` | Conforme |
| AerolineaVIP/Views/Reservas/Asistencia.cshtml | `AsistenciaViewModel` | Conforme |
| AerolineaVIP/Views/Reservas/Cancelaciones.cshtml | `List<GestionReservaViewModel>` | Conforme |
| AerolineaVIP/Views/Reservas/Gestion.cshtml | `GestionReservaViewModel` | Conforme |
| AerolineaVIP/Views/Reservas/MisReservas.cshtml | `MisReservasViewModel` | Conforme |
| AerolineaVIP/Views/Reservas/NuevaCompra.cshtml | `NuevaCompraClienteViewModel` | Conforme |
| AerolineaVIP/Views/Shared/_FormularioBusqueda.cshtml | `BusquedaVuelosViewModel` | Conforme |
| AerolineaVIP/Views/Shared/_Layout.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Views/Shared/_LoginPartial.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Views/Shared/_MapaAsientos.cshtml | `MapaCompra` | Conforme |
| AerolineaVIP/Views/Shared/_MenuPrincipal.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Views/Shared/_PasosCompra.cshtml | `List<PasoCompra>` | Conforme |
| AerolineaVIP/Views/Shared/_TarjetaVuelo.cshtml | `OpcionVuelo` | Conforme |
| AerolineaVIP/Views/Shared/AccesoDenegado.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Views/Shared/EnConstruccion.cshtml | `Sin modelo (layout/estática)` | Conforme |
| AerolineaVIP/Views/Shared/Error.cshtml | `HandleErrorInfo` | Conforme |
| AerolineaVIP/Views/Shared/Lockout.cshtml | `HandleErrorInfo` | Conforme |
| AerolineaVIP/Views/Vuelos/Buscar.cshtml | `BusquedaVuelosViewModel` | Conforme |
| AerolineaVIP/Views/Vuelos/Disponibilidad.cshtml | `DisponibilidadCompra` | Conforme |
| AerolineaVIP/Views/Vuelos/Resultados.cshtml | `ResultadosCompra` | Conforme |
