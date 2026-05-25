using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeTunnel.Migrations
{
    /// <inheritdoc />
    public partial class Agregar2FA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo2FA",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Codigo2FAExpiracion",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Codigo2FA",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Codigo2FAExpiracion",
                table: "Usuarios");
        }
    }
}
