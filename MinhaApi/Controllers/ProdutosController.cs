using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MinhaApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly ProdutoContext _context;

        private ProdutoLog _log;

        private static List<Produto> _produtos = new List<Produto>
        {
            new Produto { Id = 1, Nome = "Biscoito recheado", Preco = 5.60m },
            new Produto { Id = 2, Nome = "Sal refinado", Preco = 1.99m }
        };

        public ProdutosController(ProdutoContext context)
        {
            _context = context;

            if (_context.Produtos.Count() == 0)
                foreach (var item in _produtos)
                    _context.Produtos.Add(item);

            _context.SaveChangesAsync();
        }

        private void Inicializar()
        {
            _log = new ProdutoLog(HttpContext.Request);
            //Utils.RandomSleep();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProduto()
        {
            Inicializar();

            List<Produto> list = await _context.Produtos.ToListAsync();
            _log.Get(list, HttpStatusCode.OK);

            return await Task.FromResult(list);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            Inicializar();

            Produto? produto = _context.Produtos.FindAsync(id).Result;

            if (produto == null)
            {
                _log.Get(id, null, HttpStatusCode.NotFound);
                return NotFound();
            }

            _log.Get(id, produto, HttpStatusCode.Found);

            return produto;
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, Produto produto)
        {
            Inicializar();

            if (id != produto.Id)
            {
                _log.Put(produto, null, HttpStatusCode.BadRequest);
                return BadRequest();
            }

            Produto? antes = _context.Produtos.FindAsync(id).Result;

            _context.Entry(antes).CurrentValues.SetValues(produto);
            _context.Entry(antes).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();

                if (antes is not null)
                    _log.Put(antes, produto, HttpStatusCode.Accepted);

            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!ProdutoExists(id))
                {
                    _log.Put(produto, null, HttpStatusCode.NotFound, ex);
                    return NotFound();
                }
                else
                {
                    _log.Put(produto, null, HttpStatusCode.BadRequest, ex);
                    throw;
                }
            }
            return Accepted();
        }

        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto(Produto produto)
        {
            Inicializar();
            _context.Produtos.Add(produto);
            try
            {
                await _context.SaveChangesAsync();
                _log.Post(produto, HttpStatusCode.Created);
            }
            catch (Exception ex)
            {
                _log.Post(produto, HttpStatusCode.InternalServerError, ex);
                throw;
            }

            return CreatedAtAction(nameof(GetProduto), new { id = produto.Id }, produto);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            Inicializar();

            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
            {
                _log.Delete(id, null, HttpStatusCode.NotFound);
                return NotFound();
            }

            try
            {
                _context.Produtos.Remove(produto);
                await _context.SaveChangesAsync();

                _log.Delete(id, produto, HttpStatusCode.Accepted);

                return Accepted();
            }
            catch(Exception ex)
            {
                _log.Delete(id, null, HttpStatusCode.InternalServerError, ex);
                throw;
            }
        }

        private bool ProdutoExists(int id)
        {
            return _context.Produtos.Any(e => e.Id == id);
        }
    }
}

