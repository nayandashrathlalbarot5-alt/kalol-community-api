using System.Threading.Tasks;

namespace KalolCommunity.Application.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IRefreshTokenRepository RefreshTokens { get; }
        ICountryRepository Countries { get; }
        IStateRepository States { get; }
        ICommunityDetailRepository CommunityDetails { get; }
        IChildrenDetailRepository ChildrenDetails { get; }
        IContactUsRepository ContactUs { get; }
        Task<int> SaveChangesAsync();
    }
}
