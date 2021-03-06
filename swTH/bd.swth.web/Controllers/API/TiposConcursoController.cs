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
using bd.log.guardar.Enumeradores;
using bd.swth.entidades.Utils;

namespace bd.swth.web.Controllers.API
{
    [Produces("application/json")]
    [Route("api/TiposConcurso")]
    public class TiposConcursoController : Controller
    {
        private readonly SwTHDbContext db;

        public TiposConcursoController(SwTHDbContext db)
        {
            this.db = db;
        }

        // GET: api/BasesDatos
        [HttpGet]
        [Route("ListarTiposConcurso")]
        public async Task<List<TipoConcurso>> GetTiposConcurso()
        {
            try
            {
                return await db.TipoConcurso.OrderBy(x => x.Descripcion).ToListAsync();
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
                return new List<TipoConcurso>();
            }
        }

        // GET: api/BasesDatos/5
        [HttpGet("{id}")]
        public async Task<Response> GetTipoConcurso([FromRoute] int id)
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

                var TipoConcurso = await db.TipoConcurso.SingleOrDefaultAsync(m => m.IdTipoConcurso == id);

                if (TipoConcurso == null)
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
                    Resultado = TipoConcurso,
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

        private async Task Actualizar(TipoConcurso TipoConcurso)
        {
            var tipoconcurso = db.TipoConcurso.Find(TipoConcurso.IdTipoConcurso);

            tipoconcurso.Nombre = TipoConcurso.Nombre;
            tipoconcurso.Descripcion = TipoConcurso.Descripcion;
            db.TipoConcurso.Update(tipoconcurso);
            await db.SaveChangesAsync();
        }

        // PUT: api/BasesDatos/5
        [HttpPut("{id}")]
        public async Task<Response> PutTipoConcurso([FromRoute] int id, [FromBody] TipoConcurso TipoConcurso)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ModeloInvalido
                    };
                }

                

                var existe = Existe(TipoConcurso);
                var TipoConcursoActualizar = (TipoConcurso)existe.Resultado;

                if (existe.IsSuccess)
                {


                    if (TipoConcursoActualizar.IdTipoConcurso == TipoConcurso.IdTipoConcurso)
                    {
                        if (TipoConcurso.Nombre == TipoConcursoActualizar.Nombre &&
                        TipoConcurso.Descripcion == TipoConcursoActualizar.Descripcion)
                        {
                            return new Response
                            {
                                IsSuccess = true,
                                Message = Mensaje.ExisteRegistro,
                            };
                        }

                        await Actualizar(TipoConcurso);
                        return new Response
                        {
                            IsSuccess = true,
                            Message = Mensaje.Satisfactorio,
                        };
                    }
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ExisteRegistro,
                    };
                }

                await Actualizar(TipoConcurso);
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
                    IsSuccess = true,
                    Message = Mensaje.Excepcion,
                };
            }
        }

        // POST: api/BasesDatos
        [HttpPost]
        [Route("InsertarTipoConcurso")]
        public async Task<Response> PostTipoConcurso([FromBody] TipoConcurso TipoConcurso)
        {
            try
            {
                               
                var respuesta = Existe(TipoConcurso);
                if (!respuesta.IsSuccess)
                {
                    db.TipoConcurso.Add(TipoConcurso);
                    await db.SaveChangesAsync();
                    return new Response
                    {
                        IsSuccess = true,
                        Message = Mensaje.Satisfactorio
                    };
                }

                if (!ModelState.IsValid)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = ""
                    };
                }

                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.ExisteRegistro
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

        // DELETE: api/BasesDatos/5
        [HttpDelete("{id}")]
        public async Task<Response> DeleteTipoConcurso([FromRoute] int id)
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

                var respuesta = await db.TipoConcurso.SingleOrDefaultAsync(m => m.IdTipoConcurso == id);
                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.TipoConcurso.Remove(respuesta);
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

        private Response Existe(TipoConcurso TipoConcurso)
        {
            var bdd = TipoConcurso.Nombre;
            var TipoConcursorespuesta = db.TipoConcurso.Where(p => p.Nombre == bdd).FirstOrDefault();
            if (TipoConcursorespuesta != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = Mensaje.ExisteRegistro,
                    Resultado = TipoConcursorespuesta,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Resultado = TipoConcursorespuesta,
            };
        }
    }
}