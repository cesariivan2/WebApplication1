using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApplication1.Migrations
{
    /// <inheritdoc />
    public partial class AgregarRestriccionesUnicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inscripciones_alumnoId",
                table: "Inscripciones");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_alumnoId_horarioTallerId",
                table: "Inscripciones",
                columns: new[] { "alumnoId", "horarioTallerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alumnos_Correo",
                table: "Alumnos",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alumnos_Matricula",
                table: "Alumnos",
                column: "Matricula",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Inscripciones_alumnoId_horarioTallerId",
                table: "Inscripciones");

            migrationBuilder.DropIndex(
                name: "IX_Alumnos_Correo",
                table: "Alumnos");

            migrationBuilder.DropIndex(
                name: "IX_Alumnos_Matricula",
                table: "Alumnos");

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_alumnoId",
                table: "Inscripciones",
                column: "alumnoId");
        }
    }
}
