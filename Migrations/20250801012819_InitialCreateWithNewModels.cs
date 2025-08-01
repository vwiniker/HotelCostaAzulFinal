using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HotelCostaAzulFinal.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithNewModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_AspNetUsers_ApplicationUserId",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_Clientes_ClienteId",
                table: "Reservas");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_ApplicationUserId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_ClienteId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "ClienteId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "MontoTotal",
                table: "Reservas");

            migrationBuilder.RenameColumn(
                name: "Observaciones",
                table: "Reservas",
                newName: "NotasEspeciales");

            migrationBuilder.RenameColumn(
                name: "FechaSalida",
                table: "Reservas",
                newName: "FechaInicio");

            migrationBuilder.RenameColumn(
                name: "FechaEntrada",
                table: "Reservas",
                newName: "FechaFin");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Reservas",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<decimal>(
                name: "Total",
                table: "Reservas",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Reservas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioPorNoche",
                table: "Habitaciones",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Caracteristicas",
                table: "Habitaciones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Habitaciones",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoDocumento",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Documento",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Habitaciones",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImagenUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Habitaciones",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImagenUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Habitaciones",
                keyColumn: "Id",
                keyValue: 3,
                column: "ImagenUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Habitaciones",
                keyColumn: "Id",
                keyValue: 4,
                column: "ImagenUrl",
                value: null);

            migrationBuilder.UpdateData(
                table: "Habitaciones",
                keyColumn: "Id",
                keyValue: 5,
                column: "ImagenUrl",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_UsuarioId",
                table: "Reservas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_AspNetUsers_UsuarioId",
                table: "Reservas",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_AspNetUsers_UsuarioId",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_UsuarioId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "Total",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Reservas");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Habitaciones");

            migrationBuilder.RenameColumn(
                name: "NotasEspeciales",
                table: "Reservas",
                newName: "Observaciones");

            migrationBuilder.RenameColumn(
                name: "FechaInicio",
                table: "Reservas",
                newName: "FechaSalida");

            migrationBuilder.RenameColumn(
                name: "FechaFin",
                table: "Reservas",
                newName: "FechaEntrada");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Reservas",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Reservas",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClienteId",
                table: "Reservas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MontoTotal",
                table: "Reservas",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "PrecioPorNoche",
                table: "Habitaciones",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Caracteristicas",
                table: "Habitaciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TipoDocumento",
                table: "AspNetUsers",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Documento",
                table: "AspNetUsers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TipoDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Apellido", "Documento", "Email", "FechaRegistro", "Nombre", "Telefono", "TipoDocumento" },
                values: new object[,]
                {
                    { 1, "Pérez", "118520147", "juan.perez@email.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Juan", "+506 8888-1111", "Cédula" },
                    { 2, "González", "205670382", "maria.gonzalez@email.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "María", "+506 8888-2222", "Cédula" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ApplicationUserId",
                table: "Reservas",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ClienteId",
                table: "Reservas",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_AspNetUsers_ApplicationUserId",
                table: "Reservas",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_Clientes_ClienteId",
                table: "Reservas",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
