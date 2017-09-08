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
using bd.log.guardar.Enumeradores;
using bd.swth.entidades.Enumeradores;
using bd.swth.entidades.Utils;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using Microsoft.Win32.SafeHandles;
using bd.swth.entidades.ObjectTransfer;
using bd.swth.servicios.Interfaces;

namespace bd.swth.web.Controllers.API
{


    [Produces("application/json")]
    [Route("api/DocumentosInformacionInstitucional")]
    public class DocumentosInformacionInstitucionalController : Controller
    {
        private readonly IUploadFileService uploadFileService;
        private readonly SwTHDbContext db;
      


        public DocumentosInformacionInstitucionalController(SwTHDbContext db, IUploadFileService uploadFileService)
        {
            this.uploadFileService = uploadFileService;
            this.db = db;
        }

        // GET: api/BasesDatos
        [HttpGet]
        [Route("ListarDocumentosInformacionInstitucional")]
        public async Task<List<DocumentoInformacionInstitucional>> GetDocumentosInformacionInstitucional()
        {
            try
            {
                return await db.DocumentoInformacionInstitucional.OrderBy(x => x.Nombre).ToListAsync();
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
                return new List<DocumentoInformacionInstitucional>();
            }
        }


        [HttpPost]
        [Route("UploadFiles")]
        public async Task<Response> Post([FromBody] DocumentoInstitucionalTransfer documentoInstitucionalTransfer)
        {

            var documentoInstitucional = new DocumentoInformacionInstitucional
            {
                Nombre = documentoInstitucionalTransfer.Nombre,
            };

            documentoInstitucional =await InsertarDocumentoInformacionInstitucional(documentoInstitucional);

            var respuesta= await uploadFileService.UploadFile(documentoInstitucionalTransfer.Fichero, "Documentos", Convert.ToString(documentoInstitucional.IdDocumentoInformacionInstitucional), "pdf");


            var seleccionado = db.DocumentoInformacionInstitucional.Find(documentoInstitucional.IdDocumentoInformacionInstitucional);
            seleccionado.Url= string.Format("{0}/{1}.{2}", "Documentos", Convert.ToString(documentoInstitucional.IdDocumentoInformacionInstitucional), "pdf");
            db.DocumentoInformacionInstitucional.Update(seleccionado);
            db.SaveChanges();
            return new Response
            {
                IsSuccess = true,
            };


        }

        // POST: api/BasesDatos
        private async Task<DocumentoInformacionInstitucional> InsertarDocumentoInformacionInstitucional(DocumentoInformacionInstitucional DocumentoInformacionInstitucional)
        {
            db.DocumentoInformacionInstitucional.Add(DocumentoInformacionInstitucional);
            await db.SaveChangesAsync();
            return DocumentoInformacionInstitucional;
        }


        private async Task<DocumentoInformacionInstitucional> ActualizarDocumentoInformacionInstitucional(DocumentoInformacionInstitucional DocumentoInformacionInstitucional)
        {
            db.DocumentoInformacionInstitucional.Add(DocumentoInformacionInstitucional);
            await db.SaveChangesAsync();
            return DocumentoInformacionInstitucional;
        }


        // GET: api/BasesDatos/5
        [HttpGet("{id}")]
        public async Task<Response> GetDocumentoInformacionInstitucional([FromRoute] int id)
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

                var DocumentoInformacionInstitucional = await db.DocumentoInformacionInstitucional.SingleOrDefaultAsync(m => m.IdDocumentoInformacionInstitucional == id);

                if (DocumentoInformacionInstitucional == null)
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
                    Resultado = DocumentoInformacionInstitucional,
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

        // PUT: api/BasesDatos/5
        [HttpPut("{id}")]
        public async Task<Response> PutDocumentoInformacionInstitucional([FromRoute] int id, [FromBody] DocumentoInformacionInstitucional documentoInformacionInstitucional)
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

                var existe = Existe(documentoInformacionInstitucional);
                if (existe.IsSuccess)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.ExisteRegistro,
                    };
                }

                var documentoInformacionInstitucionalActualizar = await db.DocumentoInformacionInstitucional.Where(x => x.IdDocumentoInformacionInstitucional == id).FirstOrDefaultAsync();

                if (documentoInformacionInstitucionalActualizar != null)
                {
                    try
                    {
                        documentoInformacionInstitucionalActualizar.Nombre = documentoInformacionInstitucional.Nombre;
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




                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.ExisteRegistro
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = Mensaje.Excepcion
                };
            }
        }

        // DELETE: api/BasesDatos/5
        [HttpDelete("{id}")]
        public async Task<Response> DeleteDocumentoInformacionInstitucional([FromRoute] int id)
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

                var respuesta = await db.DocumentoInformacionInstitucional.SingleOrDefaultAsync(m => m.IdDocumentoInformacionInstitucional == id);
                if (respuesta == null)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = Mensaje.RegistroNoEncontrado,
                    };
                }
                db.DocumentoInformacionInstitucional.Remove(respuesta);
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

        private Response Existe(DocumentoInformacionInstitucional DocumentoInformacionInstitucional)
        {
            var bdd = DocumentoInformacionInstitucional.Nombre.ToUpper().TrimEnd().TrimStart();
            var DocumentoInformacionInstitucionalrespuesta = db.DocumentoInformacionInstitucional.Where(p => p.Nombre.ToUpper().TrimStart().TrimEnd() == bdd).FirstOrDefault();
            if (DocumentoInformacionInstitucionalrespuesta != null)
            {
                return new Response
                {
                    IsSuccess = true,
                    Message = "Existe un documento de información institucional con igual nombre",
                    Resultado = null,
                };

            }

            return new Response
            {
                IsSuccess = false,
                Resultado = DocumentoInformacionInstitucionalrespuesta,
            };
        }
    }
}