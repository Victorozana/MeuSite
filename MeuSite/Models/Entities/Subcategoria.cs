using System.ComponentModel.DataAnnotations;

namespace MeuSite.Models.Entities
{
    public class Subcategoria
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;
        
        public int CategoriaId { get; set; }
        
        public virtual Categoria Categoria { get; set; } = null!;
    }
}
