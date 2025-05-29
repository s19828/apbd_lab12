using LAB_12.Data;
using LAB_12.DTOs;
using LAB_12.Models;
using Microsoft.EntityFrameworkCore;

namespace LAB_12.Services;

public class DbService : IDbService
{
    private readonly ApbdContext _context;

    public DbService(ApbdContext context)
    {
        _context = context;
    }
    
    public async Task<GetTripsDTO> GetTrips(int page, int pageSize)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        
        var totalTrips = await _context.Trips.CountAsync();

        var totalPages = (int)Math.Ceiling(totalTrips / (double)pageSize);
        
        var trips = await _context.Trips
            .OrderByDescending(t => t.DateFrom)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TripDTO
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.IdCountries.Select(c => new CountryDTO
                {
                    Name = c.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(ct => new ClientDTO
                {
                    FirstName = ct.IdClientNavigation.FirstName,
                    LastName = ct.IdClientNavigation.LastName
                }).ToList()
            }).ToListAsync();

        return new GetTripsDTO
        {
            PageNum = page,
            PageSize = pageSize,
            AllPages = totalPages,
            Trips = trips
        };
    }

    public async Task RemoveClient(int id)
    {
        var client = await _context.Clients
            .Include(c => c.ClientTrips)
            .FirstOrDefaultAsync(c => c.IdClient == id);

        if (client == null)
        {
            throw new KeyNotFoundException("Client not found");
        }

        if (client.ClientTrips.Count != 0)
        {
            throw new InvalidOperationException("Cannot remove client with trips");
        }
        
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }

    public async Task AddClientToTrip(int id, AddClientDTO client)
    {
        var existingClient = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == client.Pesel);

        if (existingClient != null)
        {
            throw new InvalidOperationException("Client with this PESEL already exists");
        }
        
        var trip = await _context.Trips
            .FirstOrDefaultAsync(t => t.IdTrip == id);

        if (trip == null)
        {
            throw new KeyNotFoundException("Trip not found");
        }

        if (trip.DateFrom <= DateTime.Now)
        {
            throw new InvalidOperationException("Cannot register for trip that is currently ongoing");
        }

        var newClient = new Client
        {
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel
        };
        
        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();

        var registeredClient = await _context.ClientTrips
            .AnyAsync(ct => ct.IdClient == newClient.IdClient && ct.IdTrip == id);

        if (registeredClient)
        {
            throw new InvalidOperationException("Client is already registered on this trip");
        }

        var clientTrip = new ClientTrip
        {
            IdClient = newClient.IdClient,
            IdTrip = id,
            RegisteredAt = DateTime.Now,
            PaymentDate = client.PaymentDate
        };
        
        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();
    }
}