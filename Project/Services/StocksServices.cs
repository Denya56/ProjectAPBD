using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Project.Context;
using Project.DTOs;
using Project.Exceptions;
using Project.Models;
using Syncfusion.Blazor.Data;
using System.Net.Http;
using System.Text.Json;
using Project.Helper;

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

            if (symbol == null)
            {
                //Console.WriteLine("Null symbol");
                throw new BadHttpRequestException("Wrong company format");
            }
            //Console.WriteLine($"symbol: {symbol}");
            var companyResponce = await _context.Companies.FirstOrDefaultAsync(c => c.Symbol.Equals(symbol));
            //Console.WriteLine($"comp: {companyResponce == null}");
            if (companyResponce == null)
            {
                //Console.WriteLine("Null company");
                string responseBody = await _httpClient.GetStringAsync(
                $"https://api.polygon.io/v3/reference/tickers/{symbol.ToUpper()}?apiKey={_configuration["Stocks API Key"]}");
                //Console.WriteLine($"{responseBody}");
                var responceClass = JsonSerializer.Deserialize<CompanyDTO>(responseBody);
                if (responceClass == null)
                {
                    Console.WriteLine("Null result");
                }
                //Console.WriteLine($"{responceClass}");

                var logo = responceClass.results.branding == null ? "" : $"{responceClass.results.branding.logo_url}?apiKey={_configuration["Stocks API Key"]}";
                var address = (responceClass.results.address == null) ? "" :
                    $"{responceClass.results.address.address1} {responceClass.results.address.city} {responceClass.results.address.state}, {responceClass.results.address.postal_code}";
                var state = (responceClass.results.address == null) ? "" : $"{responceClass.results.address.state}";
                companyResponce = new Company
                {
                    Logo = logo,
                    Cik = responceClass.results.cik,
                    Bloomberg = "",
                    Figi = responceClass.results.composite_figi,
                    Lei = "",
                    Sic = responceClass.results.sic_code,
                    Country = responceClass.results.locale,
                    Industry = "",
                    Sector = responceClass.results.sic_description,
                    MarketCap = $"{responceClass.results.market_cap}",
                    EmployeesAmount = $"{responceClass.results.total_employees}",
                    Phone = responceClass.results.phone_number,
                    Ceo = "",
                    Url = responceClass.results.homepage_url,
                    Description = responceClass.results.description,
                    Exchange = responceClass.results.primary_exchange,
                    Name = responceClass.results.name,
                    Symbol = responceClass.results.ticker,
                    ExchangeSymbol = "",
                    HqAddress = address,
                    HqState = state,
                    HqCountry = responceClass.results.locale,
                    Type = responceClass.results.type,
                    Updated = responceClass.results.list_date,
                    Active = responceClass.results.active

                };
                //Console.WriteLine(companyResponce);
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
        public async Task<List<PricesTimeSpan>> GetChartDataTimeSpanAsync(string symbol, string timeSpan, string multiplier)
        {
            List<PricesTimeSpan> pricesTimeSpan = new List<PricesTimeSpan>();
            var TimeSpanParameter = "hour";
            var TimeSpanMultiplier = int.Parse(multiplier);
            var DateFrom = DateTime.Today.AddDays(-2).Date;
            var DateTo = DateTime.Today.AddDays(-2).Date;
            switch (timeSpan)
            {
                case "Weeks":
                    TimeSpanParameter = "day";
                    DateFrom = DateTime.Today.AddDays(-8).Date;
                    break;
                case "Months":
                    TimeSpanParameter = "day";
                    DateFrom = DateTime.Today.AddDays(-1).AddMonths(-1 * TimeSpanMultiplier).Date;
                    break;
            }
            string link = $"https://api.polygon.io/v2/aggs/ticker/{symbol}/range/{TimeSpanMultiplier}/{TimeSpanParameter}/{DateFrom.ToString("yyyy-MM-dd")}/{DateTo.ToString("yyyy-MM-dd")}?adjusted=true&sort=asc&apiKey={_configuration["Stocks API Key"]}";
            //Console.WriteLine(link);
            string responseBody = await _httpClient.GetStringAsync(
                $"{link}");
            var responseClass = JsonSerializer.Deserialize<TimeSpanDTO>(responseBody);
            if (responseClass == null)
            {
                var results = await _context.PricesTimeSpans.Where(x => x.Date > DateFrom && x.Date < DateTo).ToListAsync();
                return results;
            }
            //Console.WriteLine($"{responseClass}");
            //var s = System.Net.HttpStatusCode.BadRequest;
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            if (responseClass.results != null)
            {
                pricesTimeSpan = responseClass.results.Select(x => new PricesTimeSpan
                {
                    Date = dateTime.AddMilliseconds(x.t),
                    Open = x.o,
                    Low = x.l,
                    Close = x.c,
                    High = x.h,
                    Volume = x.v
                })
               .ToList();
            }
            await _context.PricesTimeSpans.AddRangeAsync(pricesTimeSpan);
            await _context.SaveChangesAsync();
            return pricesTimeSpan;
        }
        public async Task AddToWatchlist(string symbol, string userToken)
        {
            var company = await GetCompanyAsync(symbol);
            //Console.WriteLine($"symbol: {company.Symbol}");
            int IdUser = int.Parse(SecurityHelper.GetUserIdFromAccToken(userToken, _configuration["SecretKey"], _configuration["Issuer"], _configuration["Audience"]));
            //Console.WriteLine($"id: {IdUser}");
            var item = await _context.Watchlists.FirstAsync(x => x.IdUser == IdUser && x.IdCompany == company.IdCompany);
            //Console.WriteLine($"in: {IdUser} {company.IdCompany}");
            //Console.WriteLine($"out: {item.IdUser} {item.IdCompany}");
            if (item != null)
            {
                throw new BadHttpRequestException("Company is already in the watchlist", StatusCodes.Status400BadRequest);
            }
            else
            {
                await _context.Watchlists.AddAsync(new Watchlist
                {
                    IdUser = IdUser,
                    IdCompany = company.IdCompany
                });
                await _context.SaveChangesAsync();
            }
            return;
        }

        public async Task<List<WatchlistDTO>> GetWatchlist(string userToken)
        {
            int IdUser = int.Parse(SecurityHelper.GetUserIdFromAccToken(userToken, _configuration["SecretKey"], _configuration["Issuer"], _configuration["Audience"]));
            var watchlist = await _context.Watchlists.Where(x => x.IdUser == IdUser)
                .Include(a => a.Company)
                .Select(w => new WatchlistDTO
                {
                    IdCompany = w.IdCompany,
                    Logo = w.Company.Logo,
                    Symbol = w.Company.Symbol,
                    Name = w.Company.Name,
                    Sector = w.Company.Sector,
                    Country = w.Company.Country,
                    Ceo = w.Company.Ceo
                }).ToListAsync();
            return watchlist;
        }
        public async Task DeleteFromWatchlist(int idCompany)
        {
            //Console.WriteLine($"{idCompany}");
            var item = await _context.Watchlists.FirstAsync(x => x.IdCompany == idCompany);
            //Console.WriteLine($"{idCompany}");
            _context.Watchlists.Remove(item);
            //Console.WriteLine($"{idCompany}");
            await _context.SaveChangesAsync();
            //Console.WriteLine($"{idCompany}");
            return;
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