using Microsoft.EntityFrameworkCore;
using Project.DTOs;
using Project.Models;

namespace Project.Services
{
    public interface IStocksServices
    {
        Task<Company> GetCompanyAsync(string sympol);
        Task<List<string>> GetCompaniesAsync(string link);
        Task<List<PricesTimeSpan>> GetChartDataTimeSpanAsync(string symbol);
    }
}
