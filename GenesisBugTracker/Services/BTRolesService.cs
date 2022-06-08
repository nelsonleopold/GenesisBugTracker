﻿using GenesisBugTracker.Data;
using GenesisBugTracker.Models;
using GenesisBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GenesisBugTracker.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly ApplicationDbContext _context;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<BTUser> _userManager;

        public BTRolesService(ApplicationDbContext context,
                              RoleManager<IdentityRole> roleManager,
                              UserManager<BTUser> userManager)
        {
            _context = context;
            _roleManager = roleManager;
            _userManager = userManager;
        }

        #region Add User To Role Async
        public async Task<bool> AddUserToRoleAsync(BTUser user, string roleName)
        {
            try
            {
                bool result = (await _userManager.AddToRoleAsync(user, roleName)).Succeeded;
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get BTRoles Async
        public async Task<List<IdentityRole>> GetBTRolesAsync()
        {
            try
            {
                List<IdentityRole> result = new();
                result = await _context.Roles.ToListAsync();
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Role Name By Id Async
        public async Task<string> GetRoleNameByIdAsync(string roleId)
        {
            try
            {
                IdentityRole? role = _context.Roles.Find(roleId);
                string result = await _roleManager.GetRoleNameAsync(role!);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get User Role Async
        public async Task<string> GetUserRoleAsync(BTUser user)
        {
            try
            {
                IEnumerable<string> result = await _userManager.GetRolesAsync(user);
                return result.FirstOrDefault()!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get User Roles Async
        public async Task<IEnumerable<string>> GetUserRolesAsync(BTUser user)
        {
            try
            {
                IEnumerable<string> result = await _userManager.GetRolesAsync(user);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Users In Role Async
        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            try
            {
                List<BTUser> bTUsers = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
                List<BTUser> results = bTUsers.Where(u => u.CompanyId == companyId).ToList();

                return results;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Get Users Not In Role Async
        public async Task<List<BTUser>> GetUsersNotInRoleAsync(string roleName, int companyId)
        {
            try
            {
                List<string> userIds = (await _userManager.GetUsersInRoleAsync(roleName)).Select(u => u.Id).ToList();
                List<BTUser> roleUsers = _context.Users.Where(u => !userIds.Contains(u.Id)).ToList();
                List<BTUser> result = roleUsers.Where(u => u.CompanyId == companyId).ToList();

                return result;

            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Is User In Role Async
        public async Task<bool> IsUserInRoleAsync(BTUser user, string roleName)
        {
            try
            {
                bool result = await _userManager.IsInRoleAsync(user, roleName);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Remove User From Role Async
        public async Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName)
        {
            try
            {
                bool result = (await _userManager.RemoveFromRoleAsync(user, roleName)).Succeeded;
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion

        #region Remove User From Roles Async
        public async Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roles)
        {
            try
            {
                bool result = (await _userManager.RemoveFromRolesAsync(user, roles)).Succeeded;
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion
    }
}
