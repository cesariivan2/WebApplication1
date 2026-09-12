using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;

namespace WebApplication1.Data
{
    public static class DatosIniciales
    {
        public static async Task Inicializar(
            ApplicationDbContext context)
        {
            // Si ya existen talleres, no volvemos a crearlos
            if (!await context.Talleres.AnyAsync())
            {

                var tallerIA = new Taller
                {
                    nombre = "Introducción a Inteligencia Artificial",
                    descripcion = "Fundamentos y aplicaciones prácticas de IA",
                    instructor = "Ing. Ana López"
                };

                var tallerWeb = new Taller
                {
                    nombre = "Desarrollo Web",
                    descripcion = "Creación de aplicaciones web modernas",
                    instructor = "Ing. Carlos García"
                };

                var tallerCiberseguridad = new Taller
                {
                    nombre = "Ciberseguridad",
                    descripcion = "Principios básicos de seguridad informática",
                    instructor = "Ing. María Torres"
                };

                context.Talleres.AddRange(
                    tallerIA,
                    tallerWeb,
                    tallerCiberseguridad
                );

                await context.SaveChangesAsync();

                var horarios = new List<HorarioTaller>
            {
                new HorarioTaller
                {
                    tallerId = tallerIA.id,
                    fecha = DateTime.Today.AddDays(1),
                    horaInicio = new TimeSpan(9, 0, 0),
                    horaFin = new TimeSpan(11, 0, 0),
                    Espacio = "Laboratorio 1",
                    CupoMatutino = 15,
                    CupoVespertino = 15
                },

                new HorarioTaller
                {
                    tallerId = tallerWeb.id,
                    fecha = DateTime.Today.AddDays(1),
                    horaInicio = new TimeSpan(11, 30, 0),
                    horaFin = new TimeSpan(13, 30, 0),
                    Espacio = "Laboratorio 2",
                    CupoMatutino = 15,
                    CupoVespertino = 15
                },

                new HorarioTaller
                {
                    tallerId = tallerCiberseguridad.id,
                    fecha = DateTime.Today.AddDays(2),
                    horaInicio = new TimeSpan(10, 0, 0),
                    horaFin = new TimeSpan(12, 0, 0),
                    Espacio = "Aula 5",
                    CupoMatutino = 15,
                    CupoVespertino = 15
                }
            };

                context.HorariosTaller.AddRange(horarios);

                await context.SaveChangesAsync();
                if (!await context.EventosDataCode.AnyAsync())
                {
                    var evento = new EventoDataCode
                    {
                        Nombre = "DATA CODE 2.0",

                        Descripcion =
                            "Evento tecnológico universitario con talleres, torneos y retos de desarrollo.",

                        ModalidadReto = "Buildathon",

                        CoberturaTerritorial = "Local",

                        Sede = "UAdeO Unidad Regional Guamúchil",

                        FechaInicio = DateTime.Today,

                        FechaFin = DateTime.Today.AddDays(1),

                        TieneCosto = false,

                        Costo = 0,

                        Moneda = "MXN",

                        RegistroAbierto = true,

                        FechaLimiteRegistro =
                            DateTime.Today.AddDays(7)
                    };

                    context.EventosDataCode.Add(evento);

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
    
