using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HotelCostaAzulFinal.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Documento = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    TipoDocumento = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Habitaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PrecioPorNoche = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Capacidad = table.Column<int>(type: "int", nullable: false),
                    Disponible = table.Column<bool>(type: "bit", nullable: false),
                    Caracteristicas = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Habitaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FechaEntrada = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaSalida = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroHuespedes = table.Column<int>(type: "int", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaReserva = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    HabitacionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reservas_Habitaciones_HabitacionId",
                        column: x => x.HabitacionId,
                        principalTable: "Habitaciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Apellido", "Documento", "Email", "FechaRegistro", "Nombre", "Telefono", "TipoDocumento" },
                values: new object[,]
                {
                    { 1, "Pérez", "118520147", "juan.perez@email.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Juan", "+506 8888-1111", "Cédula" },
                    { 2, "González", "205670382", "maria.gonzalez@email.com", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "María", "+506 8888-2222", "Cédula" }
                });

            migrationBuilder.InsertData(
                table: "Habitaciones",
                columns: new[] { "Id", "Capacidad", "Caracteristicas", "Descripcion", "Disponible", "FechaCreacion", "Numero", "PrecioPorNoche", "Tipo" },
                values: new object[,]
                {
                    { 1, 2, "WiFi, TV, AC", "Habitación estándar con vista al jardín", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "101", 75.00m, "Standard" },
                    { 2, 3, "WiFi, TV, AC, Minibar", "Habitación deluxe con vista al mar", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "201", 120.00m, "Deluxe" },
                    { 3, 4, "WiFi, TV, AC, Minibar, Jacuzzi", "Suite presidencial con balcón privado", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "301", 200.00m, "Suite" },
                    { 4, 2, "WiFi, TV", "Habitación estándar económica", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "102", 65.00m, "Standard" },
                    { 5, 6, "WiFi, TV, AC, Minibar, Jacuzzi, Cocina", "Suite presidencial de lujo", true, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "401", 350.00m, "Presidential" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_ClienteId",
                table: "Reservas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_HabitacionId",
                table: "Reservas",
                column: "HabitacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Habitaciones");
        }
    }
}
