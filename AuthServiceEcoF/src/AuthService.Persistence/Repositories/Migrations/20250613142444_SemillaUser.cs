using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthService.Persistence.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class SemillaUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Usuarios",
                columns: new[] { "UsuarioId", "ActualizadoEn", "Apellido", "CorreoElectronico", "CreadoEn", "EstaActivo", "HashContrasena", "Nombre", "NombreUsuario", "UltimoAcceso" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Sistema", "administrador@hotmail.com", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "$2a$12$wZszxjW5OJ4JpqVygftlPO51.xjbuTS1MHvo3JEXdMbuwElbkjyiS", "Administrador", "Administrador", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "RolesUsuario",
                columns: new[] { "RolUsuarioId", "CreadoEn", "RolId", "UsuarioId" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 1, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "RolesUsuario",
                keyColumn: "RolUsuarioId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Usuarios",
                keyColumn: "UsuarioId",
                keyValue: 1);
        }
    }
}
