using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Context;
using Project.DTOs;
using Project.Exceptions;
using Project.Models;
using Syncfusion.Blazor.Data;
using System.Net.Http;
using System.Text.Json;

namespace Project.Services
{
    public class StocksServices : IStocksServices
    {
        private readonly IConfiguration _configuration;
        private readonly ProjectContext _context;
        private readonly HttpClient _httpClient = new HttpClient();

        public StocksServices(IConfiguration configuration, ProjectContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task<Company> GetCompanyAsync(string symbol)
        {
            Console.WriteLine("LOL");
            if (symbol == null)
            {
                throw new BadHttpRequestException("Wrong company format");
            }
            var companyResponce = await _context.Companies.FirstOrDefaultAsync(c => c.Symbol.Equals(symbol));

            if(companyResponce == null)
            {
                string responseBody = await _httpClient.GetStringAsync(
                $"https://api.polygon.io/v3/reference/tickers/{symbol}?apiKey={_configuration["Stocks API Key"]}");

                var responceClass = JsonSerializer.Deserialize<CompanyDTO>(responseBody);
                if (responceClass == null)
                {
                    Console.WriteLine("Null result");
                }
                Console.WriteLine($"{responceClass}");

                companyResponce = new Company
                {
                    Logo = responceClass.results.branding.logo_url,
                    Cik = responceClass.results.cik,
                    Bloomberg = null,
                    Figi = responceClass.results.composite_figi,
                    Lei = null,
                    Sic = responceClass.results.sic_code,
                    Country = responceClass.results.locale,
                    Industry = null,
                    Sector = responceClass.results.sic_description,
                    MarketCap = responceClass.results.market_cap.ToString(),
                    EmployeesAmount = responceClass.results.total_employees.ToString(),
                    Phone = responceClass.results.phone_number,
                    Ceo = null,
                    Url = responceClass.results.homepage_url,
                    Description = responceClass.results.description,
                    Exchange = responceClass.results.primary_exchange,
                    Name = responceClass.results.name,
                    Symbol = responceClass.results.ticker,
                    ExchangeSymbol = null,
                    HqAddress = $"{responceClass.results.address.address1} {responceClass.results.address.city} {responceClass.results.address.state}, {responceClass.results.address.postal_code}",
                    HqState = responceClass.results.address.state,
                    HqCountry = responceClass.results.locale,
                    Type = responceClass.results.type,
                    Updated = responceClass.results.list_date,
                    Active = responceClass.results.active

                };
                await _context.Companies.AddAsync(companyResponce);
                await _context.SaveChangesAsync();
                return companyResponce;
            }
            return companyResponce;
        }
        
        public async Task<List<string>> GetCompaniesAsync(string link)
        {
            string responseBody = await _httpClient.GetStringAsync(
                $"{link}&apiKey={_configuration["Stocks API Key"]}");
            var responseClass = JsonSerializer.Deserialize<CompaniesListDTO>(responseBody);
            var list = responseClass.results.Select(x => x.ticker).ToList();

            /*foreach (var item in list)
            {
                Console.WriteLine(item);
            }*/
            //Console.WriteLine($"{list}");
            return list;
        }
        public async Task<List<PricesTimeSpan>> GetChartDataTimeSpanAsync(string symbol)
        {
            string link = $"https://api.polygon.io/v2/aggs/ticker/{symbol}/range/1/day/2022-05-21/2022-06-21?adjusted=true&sort=asc&limit=120&apiKey=DVfT7O6xUbNbTdYdpsCApei4uYcJTS0U";
            string responseBody = await _httpClient.GetStringAsync(
                $"{link}");
            var responseClass = JsonSerializer.Deserialize<TimeSpanDTO>(responseBody);
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            List<PricesTimeSpan> pricesTimeSpan = responseClass.results.Select(x => new PricesTimeSpan
            {
                Date = dateTime.AddMilliseconds(x.t),
                Open = x.o,
                Low = x.l,
                Close = x.c,
                High = x.h,
                Volume = x.v
            })
            .ToList();
            await _context.PricesTimeSpans.AddRangeAsync(pricesTimeSpan);
            await _context.SaveChangesAsync();
            foreach (var item in pricesTimeSpan)
            {
                Console.WriteLine(item);
            }
            return pricesTimeSpan;
        }
        /*private async Task<List<string>> GetCompaniesListAsync(string link)
        {
            string responseBody = await _httpClient.GetStringAsync(
                $"{link}&apiKey={_configuration["Stocks API Key"]}");
            var responseClass = JsonSerializer.Deserialize<CompaniesListDTO>(responseBody);
            var list = responseClass.results.Select(x => x.ticker).ToList();
            if (responseClass.next_url != null)
            {
                Console.WriteLine("recurse");
                await Task.Delay(2000);
                list.AddRange(await GetCompaniesListAsync(responseClass.next_url));
            }
            return list;
        }*/
    }
}