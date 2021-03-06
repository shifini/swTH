using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using bd.swth.datos;
using bd.swth.entidades.Negocio;
using bd.log.guardar.Servicios;
using bd.log.guardar.ObjectTranfer;
using bd.swth.entidades.Enumeradores;
using bd.swth.entidades.Utils;
using bd.log.guardar.Enumeradores;
using bd.swth.entidades.ObjectTransfer;

namespace bd.swth.web.Controllers.API
{
    [Produces("application/json")]
    [Route("api/Empleados")]
    public class EmpleadosController : Controller
    {
        private readonly SwTHDbContext db;

        public EmpleadosController(SwTHDbContext db)
        {
            this.db = db;
        }

        //public class listaEmpleadoViewModel
        //{
        //    public int IdEmpleado { get; set; }
        //    public string NombreApellido { get; set;}
        //}

        // GET: api/BasesDatos
        [HttpGet]
        [Route("ListarEmpleados")]
        public async Task<List<ListaEmpleadoViewModel>> GetEmpleados()
        {
            try
            {
                //return await db.Empleado.Include(x => x.Persona).Include(x => x.CiudadNacimiento).Include(x => x.ProvinciaSufragio).Include(x => x.Dependencia).OrderBy(x => x.FechaIngreso).ToListAsync();
                var lista = await db.Empleado.Include(x => x.Persona).OrderBy(x => x.FechaIngreso).ToListAsync();
                var listaSalida = new List<ListaEmpleadoViewModel>();
                foreach (var item in lista)
                {
                    listaSalida.Add(new ListaEmpleadoViewModel
                    {
                        IdEmpleado = item.IdEmpleado,
                        NombreApellido = string.Format("{0} {1}", item.Persona.Nombres, item.Persona.Apellidos),
                        Identificacion = item.Persona.Identificacion,
                        TelefonoPrivado = item.Persona.TelefonoPrivado,
                        CorreoPrivado = item.Persona.CorreoPrivado

                    });
                }
                return listaSalida;

            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new List<ListaEmpleadoViewModel>();
            }
        }

        // GET: api/Empleados/5
        [HttpGet("{id}")]
        public async Task<Response> GetEmpleado([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }

                var Empleado = await db.Empleado.SingleOrDefaultAsync(m => m.IdEmpleado == id);

                if (Empleado == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio,
                    Resultado = Empleado,
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        //[HttpPut("{id}")]
        //public async Task<Response> PutEmpleado([FromRoute] int id, [FromBody] EmpleadoViewModel empleadoViewModel)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //        {
        //            return new Response
        //            {
        //                IsSuccess = false,
        //                Message = Mensaje.ModeloInvalido
        //            };
        //        }

        //        var existe = Existe(empleadoViewModel);
        //        if (existe.IsSuccess)
        //        {
        //            return new Response
        //            {
        //                IsSuccess = false,
        //                Message = Mensaje.ExisteRegistro,
        //            };
        //        }

        //        var empleadoActualizar = await db.Empleado.Where(x => x.IdEmpleado == id).FirstOrDefaultAsync();
        //        var personaActualizar = await db.Persona.Where(x => x.IdPersona == id).FirstOrDefaultAsync();

        //        if (empleadoActualizar != null)
        //        {
        //            try
        //            {
        //                //empleadoActualizar.Nombre = empleadoViewModel.Nombre;
        //                await db.SaveChangesAsync();

        //                return new Response
        //                {
        //                    IsSuccess = true,
        //                    Message = Mensaje.Satisfactorio,
        //                };

        //            }
        //            catch (Exception ex)
        //            {
        //                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
        //                {
        //                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
        //                    ExceptionTrace = ex,
        //                    Message = Mensaje.Excepcion,
        //                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
        //                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
        //                    UserName = "",

        //                });
        //                return new Response
        //                {
        //                    IsSuccess = false,
        //                    Message = Mensaje.Error,
        //                };
        //            }
        //        }




        //        return new Response
        //        {
        //            IsSuccess = false,
        //            Message = Mensaje.ExisteRegistro
        //        };
        //    }
        //    catch (Exception)
        //    {
        //        return new Response
        //        {
        //            IsSuccess = false,
        //            Message = Mensaje.Excepcion
        //        };
        //    }
        //}


        // POST: api/BasesDatos
        [HttpPost]
        [Route("InsertarEmpleado")]
        public async Task<Response> PostEmpleado([FromBody] EmpleadoViewModel empleadoViewModel)
        {

            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {



                    string fechaIndiceOcupacional = empleadoViewModel.IndiceOcupacionalModalidadPartida.Fecha.DayOfWeek.ToString();

                    if (fechaIndiceOcupacional.Equals("Saturday") || fechaIndiceOcupacional.Equals("Sunday"))
                    {
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "La fecha de tipo de nombramiento no puede ser fin de semana"
                        };
                    }


                    string fechaIngresoEmpleado = empleadoViewModel.Empleado.FechaIngreso.DayOfWeek.ToString();

                    if (fechaIngresoEmpleado.Equals("Saturday") || fechaIngresoEmpleado.Equals("Sunday"))
                    {
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "La fecha de ingreso a la instituci�n no puede ser fin de semana"
                        };
                    }

                    string fechaIngresoSectorPublico = empleadoViewModel.Empleado.FechaIngresoSectorPublico.DayOfWeek.ToString();

                    if (fechaIngresoSectorPublico.Equals("Saturday") || fechaIngresoSectorPublico.Equals("Sunday"))
                    {
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "La fecha de ingreso al sector p�blico no puede ser fin de semana"
                        };
                    }

                    if (empleadoViewModel.Empleado.FechaIngresoSectorPublico > empleadoViewModel.Empleado.FechaIngreso)
                    {
                        return new Response
                        {
                            IsSuccess = false,
                            Message = "La fecha de ingreso del sector p�blico no puede ser mayor que la fecha de ingreso"
                        };
                    }

                    foreach (var trayectoria in empleadoViewModel.TrayectoriaLaboral)
                    {
                        if (trayectoria.FechaInicio <= DateTime.Today)
                        {
                            return new Response
                            {
                                IsSuccess = false,
                                Message = "La fecha inicio no puede ser menor o igual que la fecha de hoy"
                            };
                        }

                        if (trayectoria.FechaFin <= DateTime.Today)
                        {
                            return new Response
                            {
                                IsSuccess = false,
                                Message = "La fecha inicio no puede ser menor o igual que la fecha de hoy"
                            };
                        }


                        if (trayectoria.FechaInicio > trayectoria.FechaFin)
                        {
                            return new Response
                            {
                                IsSuccess = false,
                                Message = "La fecha de inicio no puede ser mayor que la fecha fin"
                            };
                        }

                        string fechaInicio = trayectoria.FechaInicio.DayOfWeek.ToString();

                        if (fechaInicio.Equals("Saturday") || fechaInicio.Equals("Sunday"))
                        {
                            return new Response
                            {
                                IsSuccess = false,
                                Message = "La fecha de inicio no puede ser fin de semana"
                            };
                        }


                        string fechaFin = trayectoria.FechaFin.DayOfWeek.ToString();

                        if (fechaFin.Equals("Saturday") || fechaFin.Equals("Sunday"))
                        {
                            return new Response
                            {
                                IsSuccess = false,
                                Message = "La fecha fin no puede ser fin de semana"
                            };
                        }
                    }



                    //1. Insertar Persona 
                    var persona = await db.Persona.AddAsync(empleadoViewModel.Persona);
                    await db.SaveChangesAsync();

                    //2. Insertar Empleado (Inicializado : IdPersona, IdDependencia)
                    empleadoViewModel.Empleado.IdPersona = persona.Entity.IdPersona;
                    empleadoViewModel.Empleado.IdDependencia = 1;
                    var empleado = await db.Empleado.AddAsync(empleadoViewModel.Empleado);
                    await db.SaveChangesAsync();

                    //3. Insertar Datos Bancarios (Inicializado : IdEmpleado)
                    empleadoViewModel.DatosBancarios.IdEmpleado = empleado.Entity.IdEmpleado;
                    await db.DatosBancarios.AddAsync(empleadoViewModel.DatosBancarios);
                    await db.SaveChangesAsync();

                    //4. Insertar Empleado Contacto Emergencia  (Inicializado : IdPersona, IdEmpleado)
                    empleadoViewModel.EmpleadoContactoEmergencia.IdPersona = persona.Entity.IdPersona;
                    empleadoViewModel.EmpleadoContactoEmergencia.IdEmpleado = empleado.Entity.IdEmpleado;
                    await db.EmpleadoContactoEmergencia.AddAsync(empleadoViewModel.EmpleadoContactoEmergencia);
                    await db.SaveChangesAsync();

                    //5. Insertar Indice Ocupacional Modalidad Partida  (Inicializado : IdIndiceOcupacional, IdEmpleado)
                    empleadoViewModel.IndiceOcupacionalModalidadPartida.IdIndiceOcupacional = 1;
                    empleadoViewModel.IndiceOcupacionalModalidadPartida.IdEmpleado = 1;
                    await db.IndiceOcupacionalModalidadPartida.AddAsync(empleadoViewModel.IndiceOcupacionalModalidadPartida);
                    await db.SaveChangesAsync();

                    //6. Insertar Persona Estudio (Inicializado : IdPersona)
                    foreach (var personaEstudio in empleadoViewModel.PersonaEstudio)
                    {
                        personaEstudio.IdPersona = persona.Entity.IdPersona;
                        await db.PersonaEstudio.AddAsync(personaEstudio);
                        await db.SaveChangesAsync();
                    }

                    // 7. Insertar Trayectoria Laboral (Inicializado : IdPersona)
                    foreach (var trayectoriaLaboral in empleadoViewModel.TrayectoriaLaboral)
                    {
                        trayectoriaLaboral.IdPersona = persona.Entity.IdPersona;
                        await db.TrayectoriaLaboral.AddAsync(trayectoriaLaboral);
                        await db.SaveChangesAsync();
                    }

                    // 8. Insertar Persona Discapacidad (Inicializado : IdPersona)
                    foreach (var personaDiscapacidad in empleadoViewModel.PersonaDiscapacidad)
                    {
                        personaDiscapacidad.IdPersona = persona.Entity.IdPersona;
                        await db.PersonaDiscapacidad.AddAsync(personaDiscapacidad);
                        await db.SaveChangesAsync();
                    }

                    // 9. Insertar Persona Enfermedad (Inicializado : IdPersona)
                    foreach (var personaEnfermedad in empleadoViewModel.PersonaEnfermedad)
                    {
                        personaEnfermedad.IdPersona = persona.Entity.IdPersona;
                        await db.PersonaEnfermedad.AddAsync(personaEnfermedad);
                        await db.SaveChangesAsync();
                    }

                    // 10. Insertar Persona Sustituto (Inicializado : IdPersona, IdPersonaDiscapacidad)
                    //10.1. Insertar Persona Discapacidad
                    var objetoPersonaSustitutoDiscapacidad = await db.Persona.AddAsync(empleadoViewModel.PersonaSustituto.PersonaDiscapacidad);
                    await db.SaveChangesAsync();

                    // 10.2. Insertar Persona Sustituto
                    empleadoViewModel.PersonaSustituto.IdPersona = persona.Entity.IdPersona;
                    empleadoViewModel.PersonaSustituto.IdPersonaDiscapacidad = objetoPersonaSustitutoDiscapacidad.Entity.IdPersona;
                    var personaSustituto = empleadoViewModel.PersonaSustituto;
                    await db.PersonaSustituto.AddAsync(empleadoViewModel.PersonaSustituto);
                    await db.SaveChangesAsync();


                    // 10.3. Insertar Discapacidad Sustituto (Inicializado : IdPersonaSustituto)
                    foreach (var discapacidadSustituto in empleadoViewModel.DiscapacidadSustituto)
                    {
                        discapacidadSustituto.IdPersonaSustituto = personaSustituto.IdPersonaSustituto;
                        await db.DiscapacidadSustituto.AddAsync(discapacidadSustituto);
                        await db.SaveChangesAsync();
                    }

                    //10.4. Insertar Enfermedad Sustituto(Inicializado : IdPersonaSustituto)
                    foreach (var enfermedadSustituto in empleadoViewModel.EnfermedadSustituto)
                    {
                        enfermedadSustituto.IdPersonaSustituto = enfermedadSustituto.IdPersonaSustituto;
                        await db.EnfermedadSustituto.AddAsync(enfermedadSustituto);
                        await db.SaveChangesAsync();
                    }

                    // 11.  Insertar Familiares Empleado
                    foreach (var empleadoFamiliar in empleadoViewModel.EmpleadoFamiliar)
                    {
                        //11.1. Insertar Persona 
                        var personafamiliar = await db.Persona.AddAsync(empleadoFamiliar.Persona);
                        await db.SaveChangesAsync();

                        //11.2.  Insertar Empleado Familiar (I.icializado : IdPersona, IdEmpleado)
                        empleadoFamiliar.IdEmpleado = empleado.Entity.IdEmpleado;
                        empleadoFamiliar.IdPersona = personafamiliar.Entity.IdPersona;
                        await db.EmpleadoFamiliar.AddAsync(empleadoFamiliar);
                        await db.SaveChangesAsync();

                    }


                    transaction.Commit();

                    return new Response
                    {
                        IsSuccess = true,
                        Message = Mensaje.Satisfactorio,
                        Resultado = empleadoViewModel
                    };
                }
                catch (Exception ex)
                {

                    transaction.Rollback();
                    await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                    {
                        ApplicationName = Convert.ToString(Aplicacion.SwTH),
                        ExceptionTrace = ex,
                        Message = Mensaje.Excepcion,
                        LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                        LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                        UserName = "",

                    });
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.Error,
                    };
                }
            }

        }

        // DELETE: api/BasesDatos/5
        [HttpDelete("{id}")]
        public async Task<Response> DeleteEmpleado([FromRoute] int id)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }

                var respuesta = await db.Empleado.SingleOrDefaultAsync(m => m.IdEmpleado == id);
                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.Empleado.Remove(respuesta);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio,
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        //private Response Existe(EmpleadoViewModel empleadoViewModel)
        //{
        //    var bdd = Empleado.IdPersona;
        //    var Empleadorespuesta = db.Empleado.Where(p => p.IdPersona == bdd).FirstOrDefault();
        //    if (Empleadorespuesta != null)
        //    {
        //        return new Response
        //        {
        //            IsSuccess = true,
        //            Message = Mensaje.ExisteRegistro,
        //            Resultado = Empleadorespuesta,
        //        };

        //    }

        //    return new Response
        //    {
        //        IsSuccess = false,
        //        Resultado = Empleadorespuesta,
        //    };
        //}

        //Empleado-Contacto-Emergencia

        [HttpPost]
        [Route("InsertarEmpleadoContactoEmergencia")]
        public async Task<Response> InsertarEmpleadoContactoEmergencia([FromBody] EmpleadoContactoEmergencia empleadoContactoEmergencia)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }
                db.EmpleadoContactoEmergencia.Add(empleadoContactoEmergencia);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        [HttpPost]
        [Route("EliminarEmpleadoContactoEmergencia")]
        public async Task<Response> EliminarEmpleadoContactoEmergencia([FromBody] EmpleadoContactoEmergencia empleadoContactoEmergencia)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }

                var respuesta = await db.EmpleadoContactoEmergencia.SingleOrDefaultAsync(m => m.IdEmpleadoContactoEmergencia == empleadoContactoEmergencia.IdEmpleadoContactoEmergencia && m.IdEmpleado == empleadoContactoEmergencia.IdEmpleado);

                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.EmpleadoContactoEmergencia.Remove(respuesta);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio,
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        //�mpleado-Familiar

        [HttpPost]
        [Route("InsertarEmpleadoFamiliar")]
        public async Task<Response> InsertarEmpleadoFamiliar([FromBody] EmpleadoFamiliar empleadoFamiliar)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }
                db.EmpleadoFamiliar.Add(empleadoFamiliar);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        [HttpPost]
        [Route("EliminarEmpleadoFamiliar")]
        public async Task<Response> EliminarEmpleadoFamiliar([FromBody] EmpleadoFamiliar empleadoFamiliar)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }

                var respuesta = await db.EmpleadoFamiliar.SingleOrDefaultAsync(m => m.IdEmpleadoFamiliar == empleadoFamiliar.IdEmpleadoFamiliar && m.IdEmpleado == empleadoFamiliar.IdEmpleado);

                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.EmpleadoFamiliar.Remove(respuesta);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio,
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        //�mpleado-Nepotismo

        [HttpPost]
        [Route("InsertarEmpleadoNepotismo")]
        public async Task<Response> InsertarEmpleadoNepotismo([FromBody] EmpleadoNepotismo empleadoNepotismo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }
                db.EmpleadoNepotismo.Add(empleadoNepotismo);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        [HttpPost]
        [Route("EliminarEmpleadoNepotismo")]
        public async Task<Response> EliminarEmpleadoNepotismo([FromBody] EmpleadoNepotismo empleadoNepotismo)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }

                var respuesta = await db.EmpleadoNepotismo.SingleOrDefaultAsync(m => m.IdEmpleadoNepotismo == empleadoNepotismo.IdEmpleadoNepotismo && m.IdEmpleado == empleadoNepotismo.IdEmpleado);

                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.EmpleadoNepotismo.Remove(respuesta);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio,
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }


        //�mpleado-Datos-Bancarios

        [HttpPost]
        [Route("InsertarEmpleadoDatosBancarios")]
        public async Task<Response> InsertarEmpleadoDatosBancarios([FromBody] DatosBancarios datosBancarios)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }
                db.DatosBancarios.Add(datosBancarios);
                await db.SaveChangesAsync();
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

        [HttpPost]
        [Route("EliminarEmpleadoDatosBancarios")]
        public async Task<Response> EliminarEmpleadoDatosBancarios([FromBody] DatosBancarios datosBancarios)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido,
                    };
                }

                var respuesta = await db.DatosBancarios.SingleOrDefaultAsync(m => m.IdDatosBancarios == datosBancarios.IdDatosBancarios && m.IdEmpleado == datosBancarios.IdEmpleado);

                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.DatosBancarios.Remove(respuesta);
                await db.SaveChangesAsync();

                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.Satisfactorio,
                };
            }
            catch (Exception ex)
            {
                await GuardarLogService.SaveLogEntry(new LogEntryTranfer
                {
                    ApplicationName = Convert.ToString(Aplicacion.SwTH),
                    ExceptionTrace = ex,
                    Message = Mensaje.Excepcion,
                    LogCategoryParametre = Convert.ToString(LogCategoryParameter.Critical),
                    LogLevelShortName = Convert.ToString(LogLevelParameter.ERR),
                    UserName = "",

                });
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Error,
                };
            }
        }

    }
}