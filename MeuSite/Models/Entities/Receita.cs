using System.ComponentModel.DataAnnotations;

namespace MeuSite.Models.Entities
{
    public class Receita
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Descricao { get; set; } = string.Empty;
        
        [Required]
        public decimal Valor { get; set; }
        
        [Required]
        public DateTime Data { get; set; }
        
        public int ControleAnoId { get; set; }
        public int? CategoriaId { get; set; }
        public int? SubcategoriaId { get; set; }
        
        public virtual ControleAno ControleAno { get; set; } = null!;
        public virtual Categoria? Categoria { get; set; }
        public virtual Subcategoria? Subcategoria { get; set; }
    }
}
