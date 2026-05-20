using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeTunnel.Migrations
{
    /// <inheritdoc />
    public partial class AgregarAtaqueEIntegridad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AtaqueSimulado",
                table: "Simulaciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HashOriginal",
                table: "Simulaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "HashRecibido",
                table: "Simulaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IntegridadValida",
                table: "Simulaciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MensajeInterceptado",
                table: "Simulaciones",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AtaqueSimulado",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "HashOriginal",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "HashRecibido",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "IntegridadValida",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "MensajeInterceptado",
                table: "Simulaciones");
        }
    }
}
