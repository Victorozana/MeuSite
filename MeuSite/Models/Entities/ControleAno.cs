using System.ComponentModel.DataAnnotations;

namespace MeuSite.Models.Entities
{
    public class ControleAno
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Nome { get; set; } = string.Empty;
        
        [Required]
        public int Ano { get; set; }
        
        public bool Ativo { get; set; } = true;
        
        public virtual ICollection<Categoria> Categorias { get; set; } = new List<Categoria>();
        public virtual ICollection<Receita> Receitas { get; set; } = new List<Receita>();
        public virtual ICollection<Despesa> Despesas { get; set; } = new List<Despesa>();
        public virtual ICollection<Divida> Dividas { get; set; } = new List<Divida>();
    }
}
