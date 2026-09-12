using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AgregarComiteYVotaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PropuestasComite",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreadoPorUsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropuestasComite", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropuestasComite_AspNetUsers_CreadoPorUsuarioId",
                        column: x => x.CreadoPorUsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VotosPropuesta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropuestaComiteId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Opcion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaVoto = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VotosPropuesta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VotosPropuesta_AspNetUsers_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VotosPropuesta_PropuestasComite_PropuestaComiteId",
                        column: x => x.PropuestaComiteId,
                        principalTable: "PropuestasComite",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropuestasComite_CreadoPorUsuarioId",
                table: "PropuestasComite",
                column: "CreadoPorUsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_VotosPropuesta_PropuestaComiteId_UsuarioId",
                table: "VotosPropuesta",
                columns: new[] { "PropuestaComiteId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VotosPropuesta_UsuarioId",
                table: "VotosPropuesta",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VotosPropuesta");

            migrationBuilder.DropTable(
                name: "PropuestasComite");
        }
    }
}
