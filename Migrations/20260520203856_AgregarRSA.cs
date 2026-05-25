using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeTunnel.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRSA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CifradoRSA",
                table: "Simulaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirmaDigital",
                table: "Simulaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HuellaRSA",
                table: "Simulaciones",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CifradoRSA",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "FirmaDigital",
                table: "Simulaciones");

            migrationBuilder.DropColumn(
                name: "HuellaRSA",
                table: "Simulaciones");
        }
    }
}
