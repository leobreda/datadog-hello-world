using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MinhaApi
{
    public class ProdutoContext : DbContext
    {
        public ProdutoContext(DbContextOptions<ProdutoContext> options)
            : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }
    }
}
