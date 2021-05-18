using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Text.Json;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;
using Microsoft.Net.Http.Headers;
using System.Net.Mime;
using System.Text;

namespace EthrData
{
  public static class GetTransactions
  {
    [FunctionName("GetTransactions")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "blocks/{blockNumber}/trsansactions")] HttpRequest req,
        int blockNumber,
        ILogger log)
    {
      var web3 = new Web3("https://cloudflare-eth.com");

      var blockWithTxs = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(new HexBigInteger(blockNumber));

      var transactions = blockWithTxs.Transactions
        .Where(tx => tx.Value.Value > 0 && tx.Input.Length == 2)      
        .Select(tx => JsonSerializer.Serialize(new 
        {
          From = tx.From,
          To = tx.To,
          Amount = UnitConversion.Convert.FromWei(tx.Value.Value)
        }));

      return new FileContentResult(
        Encoding.Default.GetBytes(string.Join("\n", transactions)), 
        MediaTypeNames.Application.Octet);
    }
  }
}
