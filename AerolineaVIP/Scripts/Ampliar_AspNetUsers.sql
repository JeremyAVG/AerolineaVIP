/* Agrega Nombres y Apellidos a la tabla AspNetUsers de la base de Identity.
   Ejecutar en SSMS una sola vez, después de Script_BD_AerolineaVIP.sql
   y antes de ejecutar la aplicación. Es re-ejecutable. */

USE AerolineaVIP_Identity;
GO

IF COL_LENGTH(N'dbo.AspNetUsers', N'Nombres') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD Nombres NVARCHAR(100) NULL;
GO

IF COL_LENGTH(N'dbo.AspNetUsers', N'Apellidos') IS NULL
    ALTER TABLE dbo.AspNetUsers ADD Apellidos NVARCHAR(100) NULL;
GO

IF COL_LENGTH(N'dbo.AspNetUsers', N'Nombres') IS NOT NULL
   AND COL_LENGTH(N'dbo.AspNetUsers', N'Apellidos') IS NOT NULL
    PRINT N'OK  AspNetUsers ampliada con Nombres y Apellidos.';
ELSE
    PRINT N'ERROR  No se pudieron agregar las columnas.';
GO
