using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeTunnel.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSignalRYAtaques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AlertaAtaque",
                table: "Simulaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LatenciaAtaqueMs",
                table: "Simulaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoAtaqueEjecutado",
                table: "Simulaciones",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlertaAtaque",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "LatenciaAtaqueMs",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "TipoAtaqueEjecutado",
                table: "Simulaciones");
        }
    }
}
