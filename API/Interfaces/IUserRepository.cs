using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;

namespace API.Interfaces
{
    public interface IUserRepository
    {
        void Update(AppUser user);

        Task<bool> SaveAllAync();

        Task<IEnumerable<AppUser>> GetUserAsync();

        Task<AppUser> GetUSerByIdASync(int id);

        Task<AppUser> GetUserByUsernameAsync(string username);

        Task<IEnumerable<MemberDto>> GetMembersAync();

        Task<MemberDto> GetMemberAsync(string username);
    }
}