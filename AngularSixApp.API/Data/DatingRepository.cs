using System.Collections.Generic;
using System.Threading.Tasks;
using AngularSixApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using AngularSixApp.API.Helpers;
using System;

namespace AngularSixApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId)
                .FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);

            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users =  _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();

            users = users.Where(u => u.Id != userParams.UserId);

            users = users.Where(g => g.Gender == userParams.Gender);

            if(userParams.MinAge != 18 || userParams.MaxAge != 99){
                var minDOB= DateTime.Now.AddYears(-userParams.MaxAge -1);
                var maxDOB = DateTime.Now.AddYears(userParams.MinAge);

                users = users.Where( d => d.DateOfBirth >= minDOB && d.DateOfBirth <= maxDOB);
            }    

            if(!string.IsNullOrEmpty(userParams.OrderBy)){
                switch(userParams.OrderBy)
                {
                    case "created":
                    users= users.OrderByDescending(u => u.Created);
                    break;
                    default:
                    users= users.OrderByDescending(u => u.LastActive);
                    break;                   
                }
            }    
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}