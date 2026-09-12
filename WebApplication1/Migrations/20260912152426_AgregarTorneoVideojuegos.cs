using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTorneoVideojuegos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TorneosVideojuegos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Videojuego = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Plataforma = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Formato = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Lugar = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CupoMaximo = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TorneosVideojuegos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParticipantesTorneo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TorneoVideojuegoId = table.Column<int>(type: "int", nullable: false),
                    AlumnoId = table.Column<int>(type: "int", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantesTorneo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantesTorneo_Alumnos_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParticipantesTorneo_TorneosVideojuegos_TorneoVideojuegoId",
                        column: x => x.TorneoVideojuegoId,
                        principalTable: "TorneosVideojuegos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PartidasTorneo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TorneoVideojuegoId = table.Column<int>(type: "int", nullable: false),
                    Participante1Id = table.Column<int>(type: "int", nullable: false),
                    Participante2Id = table.Column<int>(type: "int", nullable: true),
                    GanadorId = table.Column<int>(type: "int", nullable: true),
                    Ronda = table.Column<int>(type: "int", nullable: false),
                    Puntaje1 = table.Column<int>(type: "int", nullable: true),
                    Puntaje2 = table.Column<int>(type: "int", nullable: true),
                    FechaHora = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartidasTorneo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartidasTorneo_ParticipantesTorneo_GanadorId",
                        column: x => x.GanadorId,
                        principalTable: "ParticipantesTorneo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartidasTorneo_ParticipantesTorneo_Participante1Id",
                        column: x => x.Participante1Id,
                        principalTable: "ParticipantesTorneo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartidasTorneo_ParticipantesTorneo_Participante2Id",
                        column: x => x.Participante2Id,
                        principalTable: "ParticipantesTorneo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PartidasTorneo_TorneosVideojuegos_TorneoVideojuegoId",
                        column: x => x.TorneoVideojuegoId,
                        principalTable: "TorneosVideojuegos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantesTorneo_AlumnoId",
                table: "ParticipantesTorneo",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantesTorneo_TorneoVideojuegoId_AlumnoId",
                table: "ParticipantesTorneo",
                columns: new[] { "TorneoVideojuegoId", "AlumnoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartidasTorneo_GanadorId",
                table: "PartidasTorneo",
                column: "GanadorId");

            migrationBuilder.CreateIndex(
                name: "IX_PartidasTorneo_Participante1Id",
                table: "PartidasTorneo",
                column: "Participante1Id");

            migrationBuilder.CreateIndex(
                name: "IX_PartidasTorneo_Participante2Id",
                table: "PartidasTorneo",
                column: "Participante2Id");

            migrationBuilder.CreateIndex(
                name: "IX_PartidasTorneo_TorneoVideojuegoId",
                table: "PartidasTorneo",
                column: "TorneoVideojuegoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PartidasTorneo");

            migrationBuilder.DropTable(
                name: "ParticipantesTorneo");

            migrationBuilder.DropTable(
                name: "TorneosVideojuegos");
        }
    }
}
