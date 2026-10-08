using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Llanteria.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaPais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Correo",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "IdTipoDocumento",
                table: "Usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SiglasPresentacion",
                table: "TipoDocumento",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Foto",
                table: "Clientes",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Pais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Prefijo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CodigoISO = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Pais__3214EC07", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Pais",
                columns: new[] { "Id", "CodigoISO", "Nombre", "Prefijo" },
                values: new object[,]
                {
                    { 1, "CO", "Colombia", "+57" },
                    { 2, "US", "Estados Unidos", "+1" }
                });

            migrationBuilder.InsertData(
                table: "Sexo",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Masculino" },
                    { 2, "Femenino" },
                    { 3, "Otro" }
                });

            migrationBuilder.InsertData(
                table: "TipoDocumento",
                columns: new[] { "Id", "Nombre", "SiglasPresentacion" },
                values: new object[,]
                {
                    { 1, "Cédula de Ciudadanía", "C.C." },
                    { 2, "Tarjeta de Identidad", "T.I." },
                    { 3, "Cédula de Extranjería", "C.E." },
                    { 4, "NIT", "NIT" },
                    { 5, "Pasaporte", "PAS" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_IdTipoDocumento",
                table: "Usuarios",
                column: "IdTipoDocumento");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_TipoDocumento_IdTipoDocumento",
                table: "Usuarios",
                column: "IdTipoDocumento",
                principalTable: "TipoDocumento",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_TipoDocumento_IdTipoDocumento",
                table: "Usuarios");

            migrationBuilder.DropTable(
                name: "Pais");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_IdTipoDocumento",
                table: "Usuarios");

            migrationBuilder.DeleteData(
                table: "Sexo",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Sexo",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Sexo",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TipoDocumento",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TipoDocumento",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TipoDocumento",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TipoDocumento",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TipoDocumento",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DropColumn(
                name: "Correo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IdTipoDocumento",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SiglasPresentacion",
                table: "TipoDocumento");

            migrationBuilder.DropColumn(
                name: "Foto",
                table: "Clientes");
        }
    }
}
