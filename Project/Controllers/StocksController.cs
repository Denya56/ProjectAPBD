using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Project.DTOs;
using Project.Models;
using Project.Services;

namespace Project.Controllers
{
    [Authorize]
    [Route("api/stocks")]
    [ApiController]
    public class StocksController : ControllerBase
    {
        private readonly IStocksServices _stocksServices;
        public StocksController(IStocksServices stocksServices)
        {
            _stocksServices = stocksServices;
        }
        [HttpGet("company")]
        public async Task<IActionResult> GetCompany([FromQuery]string symbol)
        {
            //Console.WriteLine(symbol);
            var s = await _stocksServices.GetCompanyAsync(symbol);
            return Ok(s);
        }
        [HttpGet("companies")]
        public object GetCompanies()
        {
            return _stocksServices.GetCompaniesAsync("sdf");
        }
        [HttpGet("timeSpan")]
        public async Task<IQueryable<PricesTimeSpan>> GetChartData([FromQuery]string symbol, [FromQuery]string timeSpan, [FromQuery] string multiplier)
        {
            var s = await _stocksServices.GetChartDataTimeSpanAsync(symbol, timeSpan, multiplier);
            /*foreach (var item in s)
            {
                Console.WriteLine(item);
            }*/
            return s.AsQueryable();
        }
        [HttpGet("watchlist")]
        public async Task<List<WatchlistDTO>> GetWatchlistAsync([FromQuery] string token)
        {
            //Console.WriteLine($"token controller: {token}");
            var s = await _stocksServices.GetWatchlist(token);
            /*foreach (var item in s)
            {
                Console.WriteLine(item);
            }*/
            return s;
        }
        [HttpPost("watchlistAdd")]
        public async Task AddToWatchlistAsync([FromQuery]string symbol, [FromQuery] string userToken)
        {
            //Console.WriteLine("lulz");
            try
            {
                await _stocksServices.AddToWatchlist(symbol, userToken);
            }
            catch (BadHttpRequestException)
            {

                Response.StatusCode = StatusCodes.Status400BadRequest;
            }
            return;
        }
    }
}
