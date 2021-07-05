using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;

        }

        public async Task<UserLike> GetUserLike(int sourceUserId, int likedUserId)
        {
            return await _context.Likes.FindAsync(sourceUserId, likedUserId);
        }

        public async Task<PagedList<LikedDto>> GetUserLikes(LikesParams likesparams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var Likes = _context.Likes.AsQueryable();

            if(likesparams.Predicate == "liked")
            {
                Likes = Likes.Where(like => like.SourceUserId == likesparams.UserId);
                users = Likes.Select(like => like.LikedUser);
            }

             if(likesparams.Predicate == "likedBy")
            {
                Likes = Likes.Where(like => like.LikedUserId == likesparams.UserId);
                users = Likes.Select(like => like.SourceUser);
            }

            var likedUsers = users.Select(user => new LikedDto
            {
                Username = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City,
                Id = user.Id
            });
            return await PagedList<LikedDto>.CreateAsync(likedUsers, 
            likesparams.PageNumber, likesparams.PageSize);
        }

        public async Task<AppUser> GetUSerWithLikes(int userId)
        {
            return await _context.Users
             .Include(x => x.LikedUsers)
             .FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}