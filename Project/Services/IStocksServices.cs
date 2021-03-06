using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Models;
using Project.Models.UserAuth;

namespace Project.Services
{
    public interface IStocksServices
    {
        Task<Company> GetCompanyAsync(string sympol);
        Task<List<string>> GetCompaniesAsync(string link);
        Task<List<PricesTimeSpan>> GetChartDataTimeSpanAsync(string symbol, string timeSpan, string multiplier);
        Task AddToWatchlist(string symbol, string userToken);
        Task<List<WatchlistDTO>> GetWatchlist(string userToken);
        Task DeleteFromWatchlist(int idCompany);
    }
}
