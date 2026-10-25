using AuthenticationModule.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
namespace AuthenticationModule.Data
{
    /// <summary>
    /// Clase encargada de sembrar datos de prueba para el módulo de proyectos.
    /// Se ejecuta al iniciar la aplicación, después de aplicar las migraciones.
    /// </summary>
    public static class SeedDataProyectos
    {
        /// <summary>
        /// Siembra datos de prueba para el módulo de proyectos.
        /// Es idempotente: si ya existen datos, no los duplica.
        /// </summary>
        /// <param name="context">Contexto de base de datos.</param>
        /// <param name="logger">Logger para registrar el proceso.</param>
        public static async Task SeedProyectosAsync(AuthDbContext context, ILogger logger)
        {
            try
            {
                // Verificar si ya existen datos
                if (await context.Proyectos.AnyAsync())
                {
                    logger.LogInformation("Los datos de proyectos ya existen. Se omite el seed.");
                    return;
                }
                logger.LogInformation("Iniciando seed de datos de proyectos...");
                // ============================================================
                // 1. CREAR DOCENTES
                // ============================================================
                var docentes = new List<Docente>
{
new Docente
{
Nombre = "Juan Carlos",
ApellidoPaterno = "Pérez",
ApellidoMaterno = "Gómez",
Especialidad = "Ingeniería de Software"
},
new Docente
{
Nombre = "María Elena",
ApellidoPaterno = "Rodríguez",
ApellidoMaterno = "López",
Especialidad = "Gestión de Proyectos"
},
new Docente
{
Nombre = "Carlos Andrés",
ApellidoPaterno = "Mamani",
ApellidoMaterno = "Quispe",
Especialidad = "Emprendimiento e Innovación"
}
};
                await context.Docentes.AddRangeAsync(docentes);
                await context.SaveChangesAsync();
                logger.LogInformation("Se crearon {Count} docentes", docentes.Count);
                // ============================================================
                // 2. CREAR ESTUDIANTES (9 en total)
                // ============================================================
                var estudiantes = new List<Estudiante>
{
// Curso 5to Año A
new Estudiante
{
Nombre = "Ana Belén",
ApellidoPaterno = "Flores",
ApellidoMaterno = "Vargas",
Curso = "5to Año A"
},
new Estudiante
{
Nombre = "Luis Fernando",
ApellidoPaterno = "Chávez",
ApellidoMaterno = "Rojas",
Curso = "5to Año A"
},
new Estudiante
{
Nombre = "Carla Sofía",
ApellidoPaterno = "Gutiérrez",
ApellidoMaterno = "Mendoza",
Curso = "5to Año A"
},
// Curso 5to Año B
new Estudiante
{
Nombre = "Diego Alejandro",
ApellidoPaterno = "Herrera",
ApellidoMaterno = "Castro",
Curso = "5to Año B"
},
new Estudiante
{
Nombre = "Valeria Nicole",
ApellidoPaterno = "Jiménez",
ApellidoMaterno = "Ortiz",
Curso = "5to Año B"
},
new Estudiante
{
Nombre = "Sebastián Mateo",
ApellidoPaterno = "Romero",
ApellidoMaterno = "Silva",
Curso = "5to Año B"
},
// Curso 6to Año A
new Estudiante
{
Nombre = "Camila Andrea",
ApellidoPaterno = "Torres",
ApellidoMaterno = "Navarro",
Curso = "6to Año A"
},
new Estudiante
{
Nombre = "Gabriel Ignacio",
ApellidoPaterno = "Vargas",
ApellidoMaterno = "Peña",
Curso = "6to Año A"
},
new Estudiante
{
Nombre = "Isabella Fernanda",
ApellidoPaterno = "Morales",
ApellidoMaterno = "Ríos",
Curso = "6to Año A"
}
};
    await context.Estudiantes.AddRangeAsync(estudiantes);
    await context.SaveChangesAsync();
    logger.LogInformation("Se crearon {Count} estudiantes", estudiantes.Count);
    // ============================================================
    // 3. CREAR PROYECTOS SOCIOCOMUNITARIOS (3)
    // ============================================================
    // --- Proyecto Sociocomunitario 1: Sistema de riego ---
    var socio1 = new ProyectoSociocomunitario
    {
        Titulo = "Sistema de Riego Comunitario en la Zona Sur",
        Problematica = "La comunidad de la zona sur enfrenta escasez de agua para sus cultivos, lo que afecta la producción agrícola y los " +
"ingresos de las familias.",
        ObjetivoGeneral = "Implementar un sistema de riego automatizado que optimice " +
"el uso del agua y mejore la producción agrícola de la zona.",
        DescripcionPrototipo = "Sistema de riego por goteo con sensores de humedad " +
"y control automático mediante Arduino y panel solar.",
        RecursosTecnologias = "Arduino, sensores de humedad, panel solar, " +
"válvulas solenoides, tuberías PVC",
        TipoProyecto = TipoProyecto.SocioComunitario,
        CreatedAt = DateTime.UtcNow,
        Beneficiarios = "50 familias productoras de la zona sur",
        UbicacionGeografica = "Zona Sur, Comunidad de Huayllani",
        ActoresInvolucrados = "Gobierno Municipal, Asociación de Regantes, " +
"Universidad, Comunidad",
        ImpactoSocialEsperado = "Mejora del 40% en la producción agrícola y " +
"reducción del 30% en el consumo de agua",
        EstrategiasSostenibilidad = "Capacitación a los comunarios, " +
"mantenimiento comunitario, fondo de reserva"
    };
    // --- Proyecto Sociocomunitario 2: Reciclaje ---
    var socio2 = new ProyectoSociocomunitario
    {
        Titulo = "Centro de Reciclaje Comunitario",
        Problematica = "La comunidad genera grandes cantidades de residuos sólidos " +
"sin un manejo adecuado, causando contaminación ambiental.",
        ObjetivoGeneral = "Establecer un centro de reciclaje comunitario que " +
"reduzca la contaminación y genere ingresos.",
        DescripcionPrototipo = "Planta de clasificación de residuos con " +
"contenedores diferenciados y prensa hidráulica.",
        RecursosTecnologias = "Contenedores diferenciados, prensa hidráulica, " +
"báscula industrial, software de gestión",
        TipoProyecto = TipoProyecto.SocioComunitario,
        CreatedAt = DateTime.UtcNow.AddDays(-15),
        Beneficiarios = "200 familias del barrio San Antonio",
        UbicacionGeografica = "Barrio San Antonio, Calle 12",
        ActoresInvolucrados = "Junta Vecinal, Alcaldía, Empresas recicladoras",
        ImpactoSocialEsperado = "Reducción del 60% de residuos sólidos y " +
"generación de 5 empleos directos",
        EstrategiasSostenibilidad = "Venta de materiales reciclados, " +
"convenios con empresas, educación ambiental"
    };
    // --- Proyecto Sociocomunitario 3: Biblioteca digital ---
    var socio3 = new ProyectoSociocomunitario
    {
        Titulo = "Biblioteca Digital Comunitaria",
        Problematica = "Los estudiantes de la comunidad no tienen acceso a " +
"material bibliográfico actualizado para sus estudios.",
        ObjetivoGeneral = "Crear una biblioteca digital accesible para todos los " +
"estudiantes de la comunidad.",
        DescripcionPrototipo = "Plataforma web con repositorio de libros " +
"digitales y acceso mediante tablets.",
        RecursosTecnologias = "Servidor web, tablets, software de gestión " +
"bibliotecaria, conexión a internet",
        TipoProyecto = TipoProyecto.SocioComunitario,
        CreatedAt = DateTime.UtcNow.AddDays(-30),
        Beneficiarios = "150 estudiantes de primaria y secundaria",
        UbicacionGeografica = "Comunidad de Villa Adela",
        ActoresInvolucrados = "Dirección de Educación, Universidad, " +
"Padres de familia",
        ImpactoSocialEsperado = "Mejora del rendimiento académico en un 25% y " +
"reducción de la deserción escolar",
        EstrategiasSostenibilidad = "Donaciones de libros, convenios con " +
"editoriales, voluntariado universitario"
    };
    await context.Proyectos.AddRangeAsync(socio1, socio2, socio3);
    await context.SaveChangesAsync();
    logger.LogInformation("Se crearon 3 proyectos sociocomunitarios");
    // ============================================================
    // 4. CREAR PROYECTOS DE EMPRENDIMIENTO (3)
    // ============================================================
    // --- Proyecto Emprendimiento 1: App de delivery ---
    var empr1 = new ProyectoEmprendimiento
    {
        Titulo = "App de Delivery para Restaurantes Locales",
        Problematica = "Los restaurantes locales no tienen presencia digital y " +
"pierden clientes frente a plataformas internacionales.",
        ObjetivoGeneral = "Desarrollar una aplicación móvil de delivery que " +
"conecte restaurantes locales con clientes.",
        DescripcionPrototipo = "Aplicación móvil con registro de restaurantes, " +
"menú digital, pedidos y seguimiento en tiempo real.",
        RecursosTecnologias = "Flutter, Firebase, Google Maps API, " +
"pasarela de pagos",
        TipoProyecto = TipoProyecto.Emprendimiento,
        CreatedAt = DateTime.UtcNow,
        SegmentoMercado = "Restaurantes locales y clientes de 20-45 años " +
"en zonas urbanas",
        PropuestaValor = "Comisión reducida (10% vs 30% de competidores) " +
"y soporte personalizado a restaurantes locales",
        AnalisisCompetencia = "Competidores: PedidosYa, Glovo. " +
"Diferenciación: comisión reducida y enfoque local",
        ModeloIngresos = "Comisión del 10% por pedido + publicidad destacada",
        EstimacionInversionInicial = 15000,
        ViabilidadEconomica = "Punto de equilibrio en 8 meses con 500 pedidos " +
"diarios. ROI estimado: 18 meses",
        EstrategiaComercializacion = "Alianzas con cámaras de restaurantes, " +
"marketing digital, referidos"
    };
    // --- Proyecto Emprendimiento 2: Tienda de café ---
    var empr2 = new ProyectoEmprendimiento
    {
        Titulo = "Tienda Virtual de Café Boliviano",
        Problematica = "Los productores de café boliviano no tienen canales " +
"directos de comercialización y pierden margen.",
        ObjetivoGeneral = "Crear una tienda virtual que conecte productores " +
"de café con consumidores nacionales e internacionales.",
        DescripcionPrototipo = "E-commerce con catálogo de cafés, suscripciones " +
        "mensuales y envío a domicilio.",
        RecursosTecnologias = "Shopify, pasarela de pagos, logística de envíos",
        TipoProyecto = TipoProyecto.Emprendimiento,
        CreatedAt = DateTime.UtcNow.AddDays(-10),
        SegmentoMercado = "Amantes del café de 25-50 años, cafeterías " +
"especializadas, mercado de exportación",
        PropuestaValor = "Café de especialidad boliviano con trazabilidad " +
"directa del productor al consumidor",
        AnalisisCompetencia = "Competidores: tiendas físicas y marketplaces. " +
"Diferenciación: origen directo y precios justos",
        ModeloIngresos = "Venta directa + suscripciones mensuales + " +
"ventas al por mayor",
        EstimacionInversionInicial = 8000,
        ViabilidadEconomica = "Margen del 35% por venta. Punto de equilibrio " +
"en 6 meses con 200 pedidos mensuales",
        EstrategiaComercializacion = "Redes sociales, influencers, ferias " +
"de café, alianzas con cafeterías"
    };
    // --- Proyecto Emprendimiento 3: Servicio de tutorías ---
    var empr3 = new ProyectoEmprendimiento
    {
        Titulo = "Plataforma de Tutorías Académicas Online",
        Problematica = "Los estudiantes tienen dificultades para encontrar " +
"tutores calificados para materias específicas.",
        ObjetivoGeneral = "Crear una plataforma que conecte estudiantes con " +
"tutores especializados en diferentes materias.",
        DescripcionPrototipo = "Plataforma web con perfiles de tutores, " +
"sistema de reservas y videollamadas.",
        RecursosTecnologias = "ASP.NET Core, WebRTC, Stripe, PostgreSQL",
        TipoProyecto = TipoProyecto.Emprendimiento,
        CreatedAt = DateTime.UtcNow.AddDays(-5),
        SegmentoMercado = "Estudiantes universitarios y de secundaria de " +
"15-25 años",
        PropuestaValor = "Tutores verificados con calificaciones reales, " +
"precios accesibles y flexibilidad horaria",
        AnalisisCompetencia = "Competidores: plataformas genéricas. " +
"Diferenciación: enfoque académico y verificación",
        ModeloIngresos = "Comisión del 15% por sesión + suscripción premium",
        EstimacionInversionInicial = 12000,
        ViabilidadEconomica = "Punto de equilibrio en 10 meses con 300 " +
"sesiones mensuales. Margen del 20%",
        EstrategiaComercializacion = "Alianzas con universidades, marketing " +
"en redes, programa de referidos"
    };
    await context.Proyectos.AddRangeAsync(empr1, empr2, empr3);
    await context.SaveChangesAsync();
    logger.LogInformation("Se crearon 3 proyectos de emprendimiento");
    // ============================================================
    // 5. ASIGNAR ESTUDIANTES A PROYECTOS
    // ============================================================
    // Sociocomunitario 1 (2 estudiantes): Ana y Luis
    estudiantes[0].ProyectoId = socio1.Id;
    estudiantes[1].ProyectoId = socio1.Id;
    // Sociocomunitario 2 (3 estudiantes): Carla, Diego, Valeria
    estudiantes[2].ProyectoId = socio2.Id;
    estudiantes[3].ProyectoId = socio2.Id;
    estudiantes[4].ProyectoId = socio2.Id;
    // Sociocomunitario 3 (2 estudiantes): Sebastián, Camila
    estudiantes[5].ProyectoId = socio3.Id;
    estudiantes[6].ProyectoId = socio3.Id;
    // Emprendimiento 1 (1 estudiante): Gabriel
    estudiantes[7].ProyectoId = empr1.Id;
    // Emprendimiento 2 (1 estudiante): Isabella
    estudiantes[8].ProyectoId = empr2.Id;
    // Emprendimiento 3 (0 estudiantes): queda libre para pruebas
    await context.SaveChangesAsync();
    logger.LogInformation("Se asignaron 9 estudiantes a los proyectos");
    // ============================================================
    // 6. CREAR EVALUACIONES
    // ============================================================

   var evaluaciones = new List<Evaluacion>
{
// Evaluaciones del Proyecto Sociocomunitario 1
new Evaluacion
{
ProyectoId = socio1.Id,
DocenteId = docentes[0].Id,
Observaciones = "El proyecto aborda una problemática real y " +
"cuenta con un prototipo bien definido. " +
"Se recomienda detallar el presupuesto.",
Recomendaciones = "Incluir un plan de mantenimiento a largo plazo " +
"y un cronograma más detallado.",
FechaEvaluacion = DateTime.UtcNow.AddDays(-5),
Nota = 85
},
new Evaluacion
{
ProyectoId = socio1.Id,
DocenteId = docentes[1].Id,
Observaciones = "Buena identificación de beneficiarios y actores " +
"involucrados. El impacto social está bien definido.",
Recomendaciones = "Fortalecer la estrategia de sostenibilidad con " +
"indicadores medibles.",
FechaEvaluacion = DateTime.UtcNow.AddDays(-3),
Nota = 88
},
// Evaluaciones del Proyecto Sociocomunitario 2
new Evaluacion
{
ProyectoId = socio2.Id,
DocenteId = docentes[1].Id,
Observaciones = "Excelente enfoque ambiental. La propuesta de " +
"reciclaje es pertinente para la comunidad.",
Recomendaciones = "Considerar la normativa ambiental vigente y " +
"obtener los permisos necesarios.",
FechaEvaluacion = DateTime.UtcNow.AddDays(-8),
Nota = 90
},
// Evaluaciones del Proyecto Sociocomunitario 3
new Evaluacion
{
ProyectoId = socio3.Id,
DocenteId = docentes[0].Id,
Observaciones = "El proyecto tiene un alto impacto educativo. " +
"La biblioteca digital es una necesidad real.",
Recomendaciones = "Definir mejor los derechos de autor y las " +
"licencias de los libros digitales.",
FechaEvaluacion = DateTime.UtcNow.AddDays(-12),
Nota = 82
},
// Evaluaciones del Proyecto Emprendimiento 1
new Evaluacion
{
ProyectoId = empr1.Id,
DocenteId = docentes[2].Id,
Observaciones = "El modelo de negocio es viable y el análisis " +
"de competencia es acertado.",
Recomendaciones = "Realizar un estudio de mercado más profundo " +
"antes de invertir.",
FechaEvaluacion = DateTime.UtcNow.AddDays(-2),
Nota = 87
},
// Evaluaciones del Proyecto Emprendimiento 2
new Evaluacion
{
ProyectoId = empr2.Id,
DocenteId = docentes[2].Id,
Observaciones = "La propuesta de valor es clara y el segmento " +
"de mercado está bien identificado.",
Recomendaciones = "Validar la logística de envíos a nivel " +
"internacional antes de escalar.",
FechaEvaluacion = DateTime.UtcNow.AddDays(-7),
Nota = 85
}
};
    await context.Evaluaciones.AddRangeAsync(evaluaciones);
    await context.SaveChangesAsync();
    logger.LogInformation("Se crearon {Count} evaluaciones", evaluaciones.Count);
    logger.LogInformation("Seed de proyectos completado exitosamente");
}
catch (Exception ex)
            {
                logger.LogError(ex, "Error al sembrar datos de proyectos");
                throw;
            }
        }
    }
}