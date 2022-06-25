﻿using Microsoft.EntityFrameworkCore;
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
                Console.WriteLine("Null symbol");
                throw new BadHttpRequestException("Wrong company format");
            }
            Console.WriteLine($"symbol: {symbol}");
            var companyResponce = await _context.Companies.FirstOrDefaultAsync(c => c.Symbol.Equals(symbol));
            Console.WriteLine($"comp: {companyResponce == null}");
            if (companyResponce == null)
            {
                //Console.WriteLine("Null company");
                string responseBody = await _httpClient.GetStringAsync(
                $"https://api.polygon.io/v3/reference/tickers/{symbol.ToUpper()}?apiKey={_configuration["Stocks API Key"]}");
                Console.WriteLine($"{responseBody}");
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
                    Bloomberg = "",
                    Figi = responceClass.results.composite_figi,
                    Lei = "",
                    Sic = responceClass.results.sic_code,
                    Country = responceClass.results.locale,
                    Industry = "",
                    Sector = responceClass.results.sic_description,
                    MarketCap = responceClass.results.market_cap.ToString(),
                    EmployeesAmount = responceClass.results.total_employees.ToString(),
                    Phone = responceClass.results.phone_number,
                    Ceo = "",
                    Url = responceClass.results.homepage_url,
                    Description = responceClass.results.description,
                    Exchange = responceClass.results.primary_exchange,
                    Name = responceClass.results.name,
                    Symbol = responceClass.results.ticker,
                    ExchangeSymbol = "",
                    HqAddress = $"{responceClass.results.address.address1} {responceClass.results.address.city} {responceClass.results.address.state}, {responceClass.results.address.postal_code}",
                    HqState = responceClass.results.address.state,
                    HqCountry = responceClass.results.locale,
                    Type = responceClass.results.type,
                    Updated = responceClass.results.list_date,
                    Active = responceClass.results.active

                };
                Console.WriteLine(companyResponce);
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
            var TimeSpanParameter = "hour";
            var TimeSpanMultiplier = int.Parse(multiplier);
            var DateFrom = DateTime.Today.AddDays(-1).Date.ToString("yyyy-MM-dd");
            var DateTo = DateTime.Today.AddDays(-1).Date.ToString("yyyy-MM-dd");
            switch (timeSpan)
            {
                case "Weeks":
                    TimeSpanParameter = "day";
                    DateFrom = DateTime.Today.AddDays(-8).Date.ToString("yyyy-MM-dd");
                    break;
                case "Months":
                    TimeSpanParameter = "day";
                    DateFrom = DateTime.Today.AddDays(-1).AddMonths(-1*TimeSpanMultiplier).Date.ToString("yyyy-MM-dd");
                    break;
            }
            string link = $"https://api.polygon.io/v2/aggs/ticker/{symbol}/range/{TimeSpanMultiplier}/{TimeSpanParameter}/{DateFrom}/{DateTo}?adjusted=true&sort=asc&apiKey=DVfT7O6xUbNbTdYdpsCApei4uYcJTS0U";
            Console.WriteLine(link);
            string responseBody = await _httpClient.GetStringAsync(
                $"{link}");
            var responseClass = JsonSerializer.Deserialize<TimeSpanDTO>(responseBody);
            //Console.WriteLine($"{responseClass}");
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
            //await _context.PricesTimeSpans.AddRangeAsync(pricesTimeSpan);
            //await _context.SaveChangesAsync();
            /*foreach (var item in pricesTimeSpan)
            {
                Console.WriteLine(item);
            }*/
            return pricesTimeSpan;
        }
        public async Task AddToWatchlist(string symbol, string userToken)
        {
            var company = await GetCompanyAsync(symbol);
            //Console.WriteLine($"symbol: {company.Symbol}");
            int IdUser = int.Parse(SecurityHelper.GetUserIdFromAccToken(userToken, _configuration["SecretKey"], _configuration["Issuer"], _configuration["Audience"]));
            //Console.WriteLine($"id: {IdUser}");
            await _context.Watchlists.AddAsync(new Watchlist
            {
                IdUser = IdUser,
                IdCompany = company.IdCompany,
                Logo = company.Logo,
                Symbol = symbol,
                Name = company.Name,
                Sector = company.Sector,
                Country = company.Country,
                Ceo = company.Ceo
            });
            await _context.SaveChangesAsync();
            return;
        }
        
        public async Task<List<WatchlistDTO>> GetWatchlist(string userToken)
        {
            int IdUser = int.Parse(SecurityHelper.GetUserIdFromAccToken(userToken, _configuration["SecretKey"], _configuration["Issuer"], _configuration["Audience"]));
            var watchlist = await _context.Watchlists.Where(x => x.IdUser == IdUser)
                .Include(a => a.Company)
                .Select(w => new WatchlistDTO
                {   
                    Logo = w.Company.Logo,
                    Symbol = w.Company.Symbol,
                    Name = w.Company.Name,
                    Sector = w.Company.Sector,
                    Country = w.Company.Country,
                    Ceo = w.Company.Ceo
                }).ToListAsync();
            return watchlist;
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