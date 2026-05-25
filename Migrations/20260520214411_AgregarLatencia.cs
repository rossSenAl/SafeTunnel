using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeTunnel.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLatencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CalidadConexion",
                table: "Simulaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LatenciaMs",
                table: "Simulaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaquetesPerdidos",
                table: "Simulaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Retransmisiones",
                table: "Simulaciones",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CalidadConexion",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "LatenciaMs",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "PaquetesPerdidos",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "Retransmisiones",
                table: "Simulaciones");
        }
    }
}
