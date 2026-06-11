using KalolCommunity.Application.Interfaces.Repositories;
using KalolCommunity.Domain.Entities;
using KalolCommunity.Infrastructure.Persistence;

namespace KalolCommunity.Infrastructure.Repositories
{
    public class ContactUsRepository(KalolCommunityDbContext context) : Repository<ContactUs>(context), IContactUsRepository
    {
    }
}
