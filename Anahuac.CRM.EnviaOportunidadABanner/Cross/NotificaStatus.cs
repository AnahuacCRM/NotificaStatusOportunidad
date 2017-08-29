using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anahuac.CRM.EnviaOportunidadABanner.Cross
{
   public class NotificaStatus: BusinessTypeBase
    {
        [Required(ErrorMessage = "El atributo {0} es requerido.")]
        [MaxLength(36, ErrorMessage = "La longitud maxima para el atributo {0} es de {1} caracteres")]
        public string id_Oportunidad { get; set; }


        [Required(ErrorMessage = "El atributo {0} es requerido.")]
        [MaxLength(1, ErrorMessage = "La longitud maxima para el atributo {0} es de {1} caracteres")]
        public string Estatus { get; set; }
    }
}
