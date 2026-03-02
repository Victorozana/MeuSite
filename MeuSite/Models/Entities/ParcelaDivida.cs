using System.ComponentModel.DataAnnotations;

namespace MeuSite.Models.Entities
{
    public class ParcelaDivida
    {
        public int Id { get; set; }
        
        [Required]
        public int NumeroParcela { get; set; }
        
        [Required]
        public decimal Valor { get; set; }
        
        [Required]
        public DateTime DataVencimento { get; set; }
        
        public bool Paga { get; set; } = false;
        public DateTime? DataPagamento { get; set; }
        
        public int DividaId { get; set; }
        
        public virtual Divida Divida { get; set; } = null!;
    }
}
