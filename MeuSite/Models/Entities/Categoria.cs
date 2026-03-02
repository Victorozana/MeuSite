using System.ComponentModel.DataAnnotations;

namespace MeuSite.Models.Entities
{
    public class Categoria
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;
        
        [MaxLength(7)]
        public string Cor { get; set; } = "#1e3a5f";
        
        public int ControleAnoId { get; set; }
        
        public virtual ControleAno ControleAno { get; set; } = null!;
        public virtual ICollection<Subcategoria> Subcategorias { get; set; } = new List<Subcategoria>();
    }
}
