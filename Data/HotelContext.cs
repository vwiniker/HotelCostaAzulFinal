using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HotelCostaAzulFinal.Models;

namespace HotelCostaAzulFinal.Data
{
    public class HotelContext : IdentityDbContext<ApplicationUser>
    {
        public HotelContext(DbContextOptions<HotelContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<Habitacion> Habitaciones { get; set; }
        public DbSet<Reserva> Reservas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Apellido).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Documento).HasMaxLength(20).IsRequired();
                entity.Property(e => e.TipoDocumento).HasMaxLength(50).IsRequired();
            });

            // Configuración de relaciones
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Reservas)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Habitacion)
                .WithMany(h => h.Reservas)
                .HasForeignKey(r => r.HabitacionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de decimal para precios
            modelBuilder.Entity<Habitacion>()
                .Property(h => h.PrecioPorNoche)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Reserva>()
                .Property(r => r.Total)
                .HasColumnType("decimal(18,2)");

            // Seed data inicial para habitaciones
            modelBuilder.Entity<Habitacion>().HasData(
                new Habitacion
                {
                    Id = 1,
                    Numero = "101",
                    Tipo = "Standard",
                    Descripcion = "Habitación estándar con vista al jardín",
                    PrecioPorNoche = 75000.00m,
                    Capacidad = 2,
                    Caracteristicas = "WiFi, TV, AC",
                    Disponible = true,
                    FechaCreacion = new DateTime(2025, 1, 1)
                },
                new Habitacion
                {
                    Id = 2,
                    Numero = "201",
                    Tipo = "Deluxe",
                    Descripcion = "Habitación deluxe con vista al mar",
                    PrecioPorNoche = 120000.00m,
                    Capacidad = 3,
                    Caracteristicas = "WiFi, TV, AC, Minibar",
                    Disponible = true,
                    FechaCreacion = new DateTime(2025, 1, 1)
                },
                new Habitacion
                {
                    Id = 3,
                    Numero = "301",
                    Tipo = "Suite",
                    Descripcion = "Suite presidencial con balcón privado",
                    PrecioPorNoche = 200000.00m,
                    Capacidad = 4,
                    Caracteristicas = "WiFi, TV, AC, Minibar, Jacuzzi",
                    Disponible = true,
                    FechaCreacion = new DateTime(2025, 1, 1)
                },
                new Habitacion
                {
                    Id = 4,
                    Numero = "102",
                    Tipo = "Standard",
                    Descripcion = "Habitación estándar económica",
                    PrecioPorNoche = 65000.00m,
                    Capacidad = 2,
                    Caracteristicas = "WiFi, TV",
                    Disponible = true,
                    FechaCreacion = new DateTime(2025, 1, 1)
                },
                new Habitacion
                {
                    Id = 5,
                    Numero = "401",
                    Tipo = "Presidential",
                    Descripcion = "Suite presidencial de lujo",
                    PrecioPorNoche = 350000.00m,
                    Capacidad = 6,
                    Caracteristicas = "WiFi, TV, AC, Minibar, Jacuzzi, Cocina",
                    Disponible = true,
                    FechaCreacion = new DateTime(2025, 1, 1)
                }
            );
        }
    }
}