using LAB_12.DTOs;

namespace LAB_12.Services;

public interface IDbService
{
    Task<GetTripsDTO> GetTrips(int page, int pageSize);
    Task RemoveClient(int id);
    Task AddClientToTrip(int id, AddClientDTO client);
}