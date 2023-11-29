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

        public async Task<IEnumerable<Ticket>> GetTicketsByCompanyAsync(int companyId)
        {
            try
            {
                IEnumerable<Ticket> companyTickets = await _context.Tickets
                                                            .Include(t => t.DeveloperUser)
                                                            .Include(t => t.Project)
                                                            .Include(t => t.SubmitterUser)
                                                            .Where(t => t.Project!.CompanyId == companyId).ToListAsync();
                return companyTickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsByCompanyAsync(int companyId)
        {
            try
            {
                IEnumerable<Project> companyProjects = await _context.Projects
                                                                    .Where(p => p.CompanyId == companyId).ToListAsync();
                return companyProjects;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<IEnumerable<TTUser>> GetUsersByCompanyAsync(int companyId)
        {
            IEnumerable<TTUser> companyUsers = await _context.Users
                                                             .Where(u => u.CompanyId == companyId).ToListAsync();
            return companyUsers;
        }
    }
}
