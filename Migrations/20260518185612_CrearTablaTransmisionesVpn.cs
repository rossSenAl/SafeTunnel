using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeTunnel.Migrations
{
    /// <inheritdoc />
    public partial class CrearTablaTransmisionesVpn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransmisionesVpn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MensajeOriginal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MensajeCifrado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MensajeDescifrado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HashIntegridad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpRealSimulada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IpProtegidaSimulada = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TipoConexion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NivelSeguridad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Protocolo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ruta = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Recomendacion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransmisionesVpn", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransmisionesVpn");
        }
    }
}
