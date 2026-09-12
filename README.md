DATA CODE 2.0

Plataforma web para la gestión integral del evento universitario DATA CODE 2.0 de la UAdeO, Unidad Regional Guamúchil.

El sistema centraliza autenticación y roles, alumnos, talleres, horarios, inscripciones con cupos separados por turno, asistencia, logística, propuestas y votaciones, torneos de videojuegos, gafetes y reportes administrativos.

Estado del proyecto revisado: ASP.NET Core MVC sobre .NET 10, Entity Framework Core, SQL Server y ASP.NET Core Identity.

1. Tecnologías

.NET 10 / ASP.NET Core MVC

Entity Framework Core 10.0.12

Microsoft.EntityFrameworkCore.SqlServer 10.0.12

ASP.NET Core Identity + Entity Framework Core 10.0.12

Microsoft SQL Server

Razor Views

Bootstrap

Bootstrap Icons

jQuery / jQuery Validation

Git y GitHub

El proyecto usa net10.0, nullable reference types e implicit usings.

2. Funcionalidad implementada

Autenticación y autorización

El proyecto usa ASP.NET Core Identity para autenticación, hash de contraseñas, cookies, usuarios y roles.

Los roles creados automáticamente al iniciar son:

Administrador

Alumno

Tallerista

Comite

Las rutas de autenticación configuradas son:

Login: /Account/Login

Acceso denegado: /Account/AccessDenied

Logout: POST /Account/Logout

La política de contraseña actual requiere:

mínimo 6 caracteres;

al menos un número;

al menos una letra minúscula;

no exige mayúscula;

no exige carácter especial.

Administración de usuarios

El módulo Usuarios está restringido al rol Administrador.

Permite:

listar cuentas de Identity y sus roles;

crear cuentas como Administrador, Alumno, Tallerista o Comité;

evitar correos duplicados;

cuando el rol es Alumno, crear también el registro en Alumnos y enlazarlo con AspNetUsers mediante UsuarioId.

Alumnos

El administrador puede:

listar alumnos;

crear alumno y cuenta Identity en una sola operación;

consultar detalle;

editar nombre, matrícula y turno;

eliminar un alumno cuando no tiene inscripciones registradas.

El alumno autenticado puede consultar su propio gafete mediante /Alumnos/MiGafete.

Campos principales:

Nombre

Matrícula

Correo

Turno (Matutino o Vespertino)

vínculo opcional con IdentityUser

La matrícula y el correo están protegidos con índices únicos en la base de datos.

Talleres y horarios

Los talleres almacenan:

nombre;

descripción;

instructor.

Los horarios almacenan:

taller;

fecha;

hora de inicio;

hora de fin;

espacio;

cupo matutino;

cupo vespertino.

Permisos actuales:

listar y consultar talleres: disponible desde el controlador sin restricción global;

crear y editar taller: Administrador o Comite;

eliminar taller: Administrador;

crear, editar y eliminar horarios: Administrador.

Inscripciones con equidad por turno

El módulo Inscripciones está autorizado para Administrador y Alumno.

El sistema:

identifica al alumno autenticado por UsuarioId;

permite al administrador seleccionar un alumno;

valida que el horario exista;

impide una inscripción activa duplicada;

impide empalmes con otro taller confirmado en la misma fecha y horario;

cuenta inscritos matutinos y vespertinos por separado;

aplica CupoMatutino y CupoVespertino de forma independiente;

permite cancelar una inscripción;

si una inscripción al mismo horario estaba cancelada, la reactiva en lugar de crear otra fila.

Estados usados actualmente:

Confirmada

Cancelada

La base de datos también contiene un índice único sobre:

AlumnoId + HorarioTallerId

Esto refuerza la protección frente a duplicados.

Asistencia

Disponible para Administrador y Tallerista.

Permite:

consultar registros de asistencia;

consultar alumnos confirmados por horario;

registrar o actualizar asistencia;

marcar Presente o Ausente;

guardar una observación de hasta 250 caracteres;

consultar detalle;

consultar resumen de presentes y ausentes.

Existe una relación uno a uno entre Inscripcion y Asistencia, con índice único por InscripcionId.

Logística

Disponible para Administrador y Tallerista.

Incluye:

agenda general de horarios;

participantes confirmados por horario;

disponibilidad de cupos por turno;

consulta de espacios;

resumen operativo con talleres, horarios, espacios, inscripciones confirmadas y canceladas.

Comité y propuestas

El módulo de propuestas requiere usuario autenticado para consulta.

Administrador y Comite pueden:

crear propuestas;

editar propuestas abiertas;

cerrar propuestas.

Cada propuesta guarda:

título;

descripción;

fecha de creación;

fecha de cierre;

estado;

usuario creador.

Estados usados:

Abierta

Cerrada

Votaciones

El rol Alumno puede:

consultar propuestas abiertas;

votar;

consultar sus votos.

Opciones válidas en el código:

A favor

En contra

Abstencion

El sistema valida que:

la propuesta exista;

permanezca abierta;

la opción sea válida;

el mismo usuario no pueda votar dos veces por la misma propuesta.

Además, la base de datos contiene un índice único sobre:

PropuestaComiteId + UsuarioId

Los resultados están restringidos a Administrador y Comite.

Torneos de videojuegos

La consulta de torneos y sus detalles es pública.

El administrador puede:

crear torneos;

editar torneos;

cerrar inscripciones;

generar la primera ronda;

programar partidas;

registrar resultados;

generar rondas posteriores;

consultar al campeón.

El alumno puede:

inscribirse a un torneo abierto;

cancelar su inscripción mientras el torneo siga abierto;

reactivar una participación cancelada si todavía existe cupo.

Cada torneo incluye:

nombre;

videojuego;

plataforma;

formato;

lugar;

cupo máximo;

fecha de inicio;

fecha de fin;

estado.

Estados utilizados en el flujo:

Abierto

Cerrado

En curso

Finalizado

Las partidas soportan:

rondas;

dos participantes o pase automático;

programación de fecha/hora;

puntajes;

ganador;

avance de ganadores a la siguiente ronda;

determinación del campeón final.

La base de datos impide registrar dos veces al mismo alumno en el mismo torneo.

Reportes

El módulo Reportes está restringido al rol Administrador.

Incluye:

total de alumnos;

alumnos matutinos;

alumnos vespertinos;

total de talleres;

total de horarios;

total de inscripciones;

inscripciones confirmadas;

inscripciones canceladas;

reporte de ocupación por horario y turno;

lugares disponibles para Matutino y Vespertino;

listado completo de inscripciones.

Gafete

El proyecto incluye vistas de gafete para alumno con:

nombre;

matrícula;

turno;

correo;

identificación visual de DATA CODE 2.0.

Configuración general del evento

La entidad EventoDataCode almacena:

nombre;

descripción;

modalidad del reto;

cobertura territorial;

sede;

fechas;

si tiene costo;

costo y moneda;

estado del registro;

fecha límite de registro.

Los datos iniciales actuales crean, cuando la tabla está vacía:

Nombre: DATA CODE 2.0

Modalidad: Buildathon

Cobertura: Local

Sede: UAdeO Unidad Regional Guamúchil

Costo: 0 MXN

Registro abierto: true

Por lo tanto, la configuración inicial representa un evento gratuito, así que el proyecto no implementa transacciones reales de pago.

3. Matriz resumida de permisos

Módulo

Público

Alumno

Tallerista

Comité

Administrador

Inicio

✅

✅

✅

✅

✅

Login

✅

✅

✅

✅

✅

Talleres - consultar

✅

✅

✅

✅

✅

Talleres - crear/editar

❌

❌

❌

✅

✅

Talleres - eliminar

❌

❌

❌

❌

✅

Horarios - administrar

❌

❌

❌

❌

✅

Inscripciones

❌

✅

❌

❌

✅

Mi gafete

❌

✅

❌

❌

❌

Asistencia

❌

❌

✅

❌

✅

Logística

❌

❌

✅

❌

✅

Propuestas - consultar

❌

✅

✅

✅

✅

Propuestas - gestionar

❌

❌

❌

✅

✅

Votar

❌

✅

❌

❌

❌

Resultados de votación

❌

❌

❌

✅

✅

Torneos - consultar

✅

✅

✅

✅

✅

Torneos - inscribirse

❌

✅

❌

❌

❌

Torneos - administrar

❌

❌

❌

❌

✅

Reportes

❌

❌

❌

❌

✅

Usuarios y roles

❌

❌

❌

❌

✅

4. Modelo de datos e integridad

ApplicationDbContext hereda de IdentityDbContext<IdentityUser>.

DbSets principales:

EventosDataCode

PropuestasComite

VotosPropuesta

Alumnos

Talleres

HorariosTaller

Inscripciones

Asistencias

TorneosVideojuegos

ParticipantesTorneo

PartidasTorneo

Restricciones relevantes configuradas con Entity Framework Core:

matrícula de Alumno única;

correo de Alumno único;

relación uno a uno Alumno -> IdentityUser mediante UsuarioId;

combinación Alumno + Horario única en Inscripciones;

una Asistencia por Inscripción;

un Voto por Usuario y Propuesta;

una Participación por Alumno y Torneo;

costo del evento con precisión decimal 10,2.

5. Migraciones incluidas

El proyecto contiene las siguientes migraciones:

CrearBaseInicial

AgregarTalleresEInscripciones

AgregarIdentity

RelacionAlumnoIdentity

AgregarRestriccionesUnicas

AgregarAsistencias

AgregarComiteYVotaciones

AgregarTorneoVideojuegos

AgregarConfiguracionEvento

Program.cs ejecuta automáticamente:

await context.Database.MigrateAsync();

Por eso, cuando la aplicación inicia con una conexión válida, intenta aplicar las migraciones pendientes antes de crear roles y datos iniciales.

6. Datos iniciales

Data/DatosIniciales.cs crea información solamente cuando corresponde.

Si no existen talleres, crea:

Introducción a Inteligencia Artificial

Instructor: Ing. Ana López

Laboratorio 1

09:00 a 11:00

Cupo: 15 Matutino + 15 Vespertino

Desarrollo Web

Instructor: Ing. Carlos García

Laboratorio 2

11:30 a 13:30

Cupo: 15 Matutino + 15 Vespertino

Ciberseguridad

Instructor: Ing. María Torres

Aula 5

10:00 a 12:00

Cupo: 15 Matutino + 15 Vespertino

Las fechas de estos horarios se calculan relativamente al día en que se inicializa por primera vez la base (DateTime.Today + 1/+2 días).

También crea la configuración general de DATA CODE 2.0 si todavía no existe.

El seeder no crea automáticamente alumnos, talleristas, comité, propuestas ni torneos. Las cuentas adicionales se crean desde el módulo de administración de usuarios.

7. Requisitos

Desarrollo en Windows

.NET 10 SDK

SQL Server

Visual Studio con soporte para ASP.NET Core / .NET 10, o CLI de .NET

Git

Comprobar SDK:

dotnet --version

El proyecto espera net10.0.

8. Instalación rápida

8.1 Clonar el repositorio

git clone https://github.com/cesariivan2/WebApplication1.git
cd WebApplication1

8.2 Restaurar dependencias

Desde la raíz del repositorio:

dotnet restore WebApplication1/WebApplication1.csproj

8.3 Configurar SQL Server

La configuración actual de desarrollo en appsettings.json es:

{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DataCodeDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}

Esto usa autenticación integrada de Windows.

Si su instancia usa otro nombre, modifique Server=. Ejemplos comunes:

Server=localhost
Server=.\SQLEXPRESS
Server=NOMBRE-PC\SQLEXPRESS

No suba credenciales reales al repositorio.

8.4 Configurar administrador inicial con User Secrets

El proyecto tiene UserSecretsId configurado y Program.cs lee:

DemoAdmin:Email

DemoAdmin:Password

Desde la raíz del repositorio:

dotnet user-secrets --project WebApplication1/WebApplication1.csproj set "DemoAdmin:Email" "admin@datacode.mx"
dotnet user-secrets --project WebApplication1/WebApplication1.csproj set "DemoAdmin:Password" "CAMBIAR_POR_UNA_CONTRASENA_SEGURA"

La contraseña debe respetar la política actual: mínimo 6 caracteres, al menos una minúscula y un número.

No incluya la contraseña real en README.md, appsettings.json, commits o capturas públicas.

8.5 Compilar

dotnet build WebApplication1/WebApplication1.csproj

8.6 Ejecutar

dotnet run --project WebApplication1/WebApplication1.csproj

Al iniciar correctamente, la aplicación:

se conecta a SQL Server;

aplica migraciones pendientes;

crea los roles faltantes;

crea o completa el rol del administrador configurado por User Secrets;

carga los datos iniciales faltantes.

9. Ejecución desde Visual Studio

Abrir WebApplication1.slnx.

Verificar appsettings.json.

Configurar DemoAdmin mediante User Secrets.

Comprobar que SQL Server esté iniciado y accesible.

Compilar con Ctrl + Shift + B.

Ejecutar el proyecto.

Iniciar sesión en /Account/Login.

10. Credenciales de demostración

El repositorio no debe incluir contraseñas reales.

La cuenta de administrador se determina por los User Secrets locales:

DemoAdmin:Email
DemoAdmin:Password

Para crear cuentas de prueba de otros roles:

iniciar sesión como Administrador;

entrar a Usuarios;

elegir Crear usuario;

seleccionar Alumno, Tallerista, Comite o Administrador;

si se selecciona Alumno, capturar también nombre, matrícula y turno.

11. Flujo funcional principal

Flujo recomendado para demostrar el sistema:

Login Alumno
   ↓
Consultar Talleres
   ↓
Mis inscripciones
   ↓
Nueva inscripción
   ↓
Validación de horario + cupo Matutino/Vespertino
   ↓
Guardar en SQL Server
   ↓
Consultar detalle
   ↓
Cancelar / reactivar inscripción
   ↓
Consultar Mi Gafete

Otros flujos demostrables:

Gobernanza

Comité crea propuesta
   ↓
Alumno vota
   ↓
Índice único evita doble voto
   ↓
Comité/Admin consulta resultados
   ↓
Comité/Admin cierra propuesta

Operación

Tallerista abre Agenda
   ↓
Consulta participantes
   ↓
Registra asistencia
   ↓
Consulta espacios y resumen logístico

Torneo

Admin crea torneo
   ↓
Alumnos se inscriben
   ↓
Admin cierra inscripciones
   ↓
Genera primera ronda
   ↓
Programa partidas
   ↓
Registra resultados
   ↓
Genera siguientes rondas
   ↓
Sistema determina campeón

12. Publicación para Linux

.NET y ASP.NET Core son multiplataforma. El proyecto puede generarse para un destino Linux x64 con:

dotnet publish WebApplication1/WebApplication1.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained false \
  -o publish-linux

Una publicación exitosa debe generar, entre otros archivos:

publish-linux/
├── WebApplication1.dll
├── WebApplication1.deps.json
├── WebApplication1.runtimeconfig.json
├── appsettings.json
└── wwwroot/

En un servidor con runtime .NET 10 instalado se ejecutaría con:

dotnet WebApplication1.dll

Importante: conexión SQL en Linux

La cadena actual usa:

Trusted_Connection=True

Esa configuración corresponde al entorno de desarrollo con autenticación integrada de Windows y no debe asumirse como configuración de producción Linux.

Para un VPS Linux, configure la cadena mediante variables de entorno y una forma de autenticación admitida por el SQL Server objetivo, por ejemplo:

export ConnectionStrings__DefaultConnection="Server=SERVIDOR_SQL;Database=DataCodeDB;User Id=USUARIO_SQL;Password=CONTRASENA;TrustServerCertificate=True"

Configurar también el administrador inicial:

export DemoAdmin__Email="admin@datacode.mx"
export DemoAdmin__Password="CONTRASENA_SEGURA"

Y, si se desea exponer Kestrel en un puerto interno del servidor:

export ASPNETCORE_URLS="http://0.0.0.0:5000"
dotnet WebApplication1.dll

13. Propuesta de despliegue en VPS Linux

Arquitectura recomendada:

Internet
   ↓
Nginx (HTTPS / reverse proxy)
   ↓
ASP.NET Core - Kestrel
   ↓
Entity Framework Core
   ↓
SQL Server

Recomendaciones de producción:

publicar con dotnet publish;

usar Nginx como reverse proxy;

ejecutar la app con systemd;

guardar secretos en variables de entorno o un gestor de secretos;

no versionar contraseñas;

usar HTTPS;

restringir el acceso de red a SQL Server;

crear un usuario SQL con permisos mínimos necesarios para la base DataCodeDB.

Ejemplo conceptual de servicio systemd

[Unit]
Description=DATA CODE 2.0
After=network.target

[Service]
WorkingDirectory=/var/www/datacode
ExecStart=/usr/bin/dotnet /var/www/datacode/WebApplication1.dll
Restart=always
RestartSec=10
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000

[Install]
WantedBy=multi-user.target

Las cadenas de conexión y contraseñas deben configurarse de forma segura en el servidor, no escribirse directamente en un repositorio público.

14. Verificación específica antes de desplegar en Linux

Linux distingue mayúsculas y minúsculas en nombres de archivo y carpetas.

Antes del despliegue, compruebe que Git registre exactamente los nombres esperados por las acciones MVC, especialmente:

Views/Talleres/Index.cshtml
Views/Talleres/Details.cshtml
Views/Inscripciones/Create.cshtml
Views/Alumnos/MiGafete.cshtml
Views/Votaciones/MisVotos.cshtml

En el paquete fuente revisado existen entradas cuyo uso de mayúsculas/minúsculas no es uniforme. Windows puede ocultar este problema; Linux no. Normalice estos nombres en Git antes de afirmar que el proyecto fue ejecutado exitosamente en Linux.

Una publicación linux-x64 exitosa verifica que el proyecto puede publicarse para Linux. La verificación completa de ejecución requiere además arrancar la aplicación en Linux con una conexión SQL válida y recorrer los flujos principales.

15. Seguridad

Medidas presentes en el código:

ASP.NET Core Identity;

contraseñas almacenadas mediante el mecanismo de Identity, no como texto plano;

autorización por roles;

antiforgery tokens en formularios POST relevantes;

User Secrets para el administrador de desarrollo;

índices únicos para reforzar reglas críticas de integridad;

alumno autenticado identificado mediante ClaimTypes.NameIdentifier / UsuarioId en el flujo de inscripciones;

validaciones de cupo, duplicados y estado antes de operaciones críticas.

No suba al repositorio:

secrets.json;

contraseñas SQL;

contraseñas de usuarios;

datos personales reales innecesarios;

archivos de backup con información sensible.

16. Limitaciones y notas actuales

Para mantener la documentación fiel al código actual:

el evento inicial está configurado como gratuito, por lo que no existe un flujo de pago real;

no existe un módulo independiente de avisos/notificaciones persistidas en base de datos;

los datos iniciales crean talleres, horarios y configuración del evento, pero no crean automáticamente propuestas o torneos;

eliminar un registro de Alumno desde AlumnosController no elimina automáticamente la cuenta correspondiente de AspNetUsers; para la demostración se recomienda evitar borrar alumnos con cuenta salvo que se gestione también la identidad;

los archivos *.cshtml.cs presentes junto a algunas vistas no forman parte del patrón necesario para estas vistas MVC y no son requeridos para describir el funcionamiento principal;

antes de desplegar en Linux debe verificarse el casing exacto de carpetas y vistas descrito anteriormente.

17. Estructura principal

WebApplication1/
├── Controllers/
│   ├── AccountController.cs
│   ├── AlumnosController.cs
│   ├── AsistenciasController.cs
│   ├── HomeController.cs
│   ├── HorariosTallerController.cs
│   ├── InscripcionesController.cs
│   ├── LogisticaController.cs
│   ├── PartidasTorneoController.cs
│   ├── PropuestasComiteController.cs
│   ├── ReportesController.cs
│   ├── TalleresController.cs
│   ├── TorneosVideojuegosController.cs
│   ├── UsuariosController.cs
│   └── VotacionesController.cs
├── Data/
│   └── DatosIniciales.cs
├── Migrations/
├── Models/
├── Views/
├── wwwroot/
├── ApplicationDbContext.cs
├── Program.cs
├── appsettings.json
└── WebApplication1.csproj

18. Solución de problemas

No se puede conectar a SQL Server

Verifique:

que el servicio SQL Server esté iniciado;

el valor de Server=;

que DataCodeDB pueda crearse/accederse;

permisos del usuario;

firewall si SQL Server está en otra máquina;

puerto TCP cuando corresponda.

Error al iniciar por tablas inexistentes

Program.cs ejecuta Database.MigrateAsync() antes de crear roles y datos iniciales. Si falla, revise primero la cadena de conexión y los permisos del usuario SQL para crear/modificar esquema.

No aparece el administrador

Compruebe los User Secrets:

dotnet user-secrets --project WebApplication1/WebApplication1.csproj list

Deben existir las claves:

DemoAdmin:Email
DemoAdmin:Password

AccessDenied

Es comportamiento esperado cuando un rol intenta abrir un módulo para el que no está autorizado.

La aplicación funciona en Windows pero una vista falla en Linux

Revise primero mayúsculas/minúsculas del nombre de la carpeta y del .cshtml.

19. Recursos de terceros

El proyecto incluye o utiliza:

Bootstrap

Bootstrap Icons

jQuery

jQuery Validation

jQuery Validation Unobtrusive

Las dependencias locales conservan sus archivos de licencia correspondientes dentro de wwwroot/lib cuando están incluidos por la plantilla/proyecto.

Este paquete no contiene un archivo LICENSE propio en la raíz. Si la entrega exige una licencia permisiva para el código del equipo, agregue un archivo LICENSE apropiado antes de publicar la versión final.

20. Checklist de entrega

Antes de entregar:

Ctrl + Shift + B / dotnet build termina con 0 errores.

Login y logout funcionan.

Administrador puede acceder a Usuarios, Alumnos, Talleres, Horarios, Inscripciones, Asistencias, Logística, Comité, Torneos y Reportes.

Alumno puede inscribirse, cancelar y reactivar una inscripción.

Alumno puede consultar gafete, torneos y votaciones.

Tallerista puede consultar agenda y registrar asistencia.

Comité puede crear/editar/cerrar propuestas y consultar resultados.

Un rol sin permiso llega a AccessDenied.

No hay contraseñas reales en GitHub.

El repositorio es público si así lo exige la entrega.

El README está visible en la raíz del repositorio.

Se verificó el casing de vistas para Linux.

Se ejecutó dotnet publish -r linux-x64 si se presenta como evidencia de publicación Linux.

Se agregó LICENSE si es obligatorio para la entrega.

21. Repositorio

Repositorio del proyecto:

https://github.com/cesariivan2/WebApplication1.git

22. Equipo

Proyecto desarrollado para DATA CODE 2.0.

UAdeO - Unidad Regional Guamúchil.
