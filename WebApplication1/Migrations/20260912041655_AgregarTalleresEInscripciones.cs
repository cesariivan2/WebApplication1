using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTalleresEInscripciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Talleres",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    instructor = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Talleres", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "HorariosTaller",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tallerId = table.Column<int>(type: "int", nullable: false),
                    fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    horaInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    horaFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    Espacio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CupoMatutino = table.Column<int>(type: "int", nullable: false),
                    CupoVespertino = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosTaller", x => x.id);
                    table.ForeignKey(
                        name: "FK_HorariosTaller_Talleres_tallerId",
                        column: x => x.tallerId,
                        principalTable: "Talleres",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Inscripciones",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    alumnoId = table.Column<int>(type: "int", nullable: false),
                    horarioTallerId = table.Column<int>(type: "int", nullable: false),
                    FechaInscripcion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    estado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inscripciones", x => x.id);
                    table.ForeignKey(
                        name: "FK_Inscripciones_Alumnos_alumnoId",
                        column: x => x.alumnoId,
                        principalTable: "Alumnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inscripciones_HorariosTaller_horarioTallerId",
                        column: x => x.horarioTallerId,
                        principalTable: "HorariosTaller",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HorariosTaller_tallerId",
                table: "HorariosTaller",
                column: "tallerId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_alumnoId",
                table: "Inscripciones",
                column: "alumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_horarioTallerId",
                table: "Inscripciones",
                column: "horarioTallerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Inscripciones");

            migrationBuilder.DropTable(
                name: "HorariosTaller");

            migrationBuilder.DropTable(
                name: "Talleres");
        }
    }
}
