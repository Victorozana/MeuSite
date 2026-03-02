using System.ComponentModel.DataAnnotations;

namespace MeuSite.Models.Entities
{
    public class Divida
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Descricao { get; set; } = string.Empty;
        
        [Required]
        public decimal ValorTotal { get; set; }
        
        [Required]
        public int NumeroParcelas { get; set; }
        
        [Required]
        public DateTime DataPrimeiroVencimento { get; set; }
        
        public int ControleAnoId { get; set; }
        public int? CategoriaId { get; set; }
        
        public virtual ControleAno ControleAno { get; set; } = null!;
        public virtual Categoria? Categoria { get; set; }
        public virtual ICollection<ParcelaDivida> ParcelasDividas { get; set; } = new List<ParcelaDivida>();
    }
}
