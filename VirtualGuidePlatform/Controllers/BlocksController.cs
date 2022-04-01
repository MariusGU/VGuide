using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VirtualGuidePlatform.Data.Entities.Blocks;
using VirtualGuidePlatform.Data.Repositories;

namespace VirtualGuidePlatform.Controllers
{
    [ApiController]
    [Route("api/guides/{guideid}/blocks")]
    public class BlocksController : ControllerBase
    {
        //public BlocksController(IBlocksRepository blocksRepository)
        //{
        //    _accountsRepository = accountsRepository;
        //}

        //[HttpGet]
        //public async Task<ActionResult<Pblocks>> GetPBlocks()
        //{

        //}

    }
}
