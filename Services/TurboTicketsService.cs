﻿using TurboTicketsMVC.Data;
using TurboTicketsMVC.Models;
using TurboTicketsMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace TurboTicketsMVC.Services
{
    public class TurboTicketsService: ITurboTicketsService
    {
        private readonly ApplicationDbContext _context;
        public TurboTicketsService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IEnumerable<Company>> GetAllCompaniesAsync()
        {
            try
            {
                IEnumerable<Company> allCompanies = await _context.Companies.ToListAsync();
                return allCompanies;
            }
            catch (Exception)
            {

                throw;
            }
        }

    }
}
