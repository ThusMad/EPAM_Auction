﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EPAM_BusinessLogicLayer.DataTransferObjects;
using EPAM_BusinessLogicLayer.Infrastructure;
using EPAM_BusinessLogicLayer.Services.Interfaces;
using EPAM_DataAccessLayer.Entities;
using EPAM_DataAccessLayer.UnitOfWork.Interfaces;
using EPAM_BusinessLogicLayer.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EPAM_BusinessLogicLayer.Services
{
    /// <summary>
    /// This class provides a realization of basic operations that needed to work with user accounts
    /// </summary>
    public class AccountService : IAccountService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        private readonly UserManager<ApplicationUser> _userManager;

        public AccountService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        /// <summary>
        /// This function create a new user for application
        /// </summary>
        /// <param name="registrationDto">Object in which data necessary for registration is stored</param>
        /// <param name="roles">List of roles for created user</param>
        /// <returns>DTO model for created user</returns>
        /// <exception cref="UserException">Thrown when user already registered or password is incorrect for details <seealso cref="UserException.Message"/> </exception>
        public async Task<ApplicationUserDto> InsertUserAsync(RegistrationDTO registrationDto, IEnumerable<string> roles)
        {
            if (await GetUserByUsernameAsync(registrationDto.Username).ConfigureAwait(false) != null)
            {
                throw new UserException(200, "User with following username already exists");
            }

            // TODO: use mapper
            var user = new ApplicationUser
            {
                Email = registrationDto.Email,
                UserName = registrationDto.Username,
                RegistrationDate = Utility.DateTimeToUnixTimestamp(DateTime.UtcNow)
            };
            var createResult = await _userManager.CreateAsync(user, registrationDto.Password);

            if (!createResult.Succeeded)
            {
                throw new UserException(500,
                    string.Join("\n", createResult.Errors.ToArray().SelectMany(a => a.Description)));
            }

            foreach (var role in roles)
            {
                var addRoleResult = await _userManager.AddToRoleAsync(user, role);

                if (!addRoleResult.Succeeded)
                {
                    throw new UserException(500,
                        string.Join("\n", addRoleResult.Errors.ToArray().SelectMany(a => a.Description)));
                }
            }

            return _mapper.Map<ApplicationUser, ApplicationUserDto>(user);
        }

        /// <summary>
        /// Checks if a user with the following username have the following password
        /// </summary>
        /// <param name="username">User password</param>
        /// <param name="password">User username</param>
        /// <returns>Result of comparing password stored in a database with provided</returns>
        ///<exception cref="ItemNotFountException">Throws when a database doesn't contain user with the provided username</exception>
        public async Task<bool> IsValidUsernameAndPasswordCombinationAsync(string? username, string? password)
        {
            if (username == null || password == null)
            {
                throw new ArgumentNullException("password or username is null");
            }

            var user = await GetUserByUsernameAsync(username).ConfigureAwait(false);

            return await _userManager.CheckPasswordAsync(user, password);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="media"></param>
        /// <returns></returns>
        public Task AttachProfilePicture(Guid userId, string media)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// This method found user by provided id and map it to <seealso cref="TUserDto"/> 
        /// </summary>
        /// <typeparam name="TUserDto">Type that <see cref="ApplicationUser"/> entity will be mapped </typeparam>
        /// <param name="id">User id that must be found</param>
        /// <returns>User dto</returns>
        /// <exception cref="ArgumentNullException">Throws when provided id is null or empty</exception>
        /// <exception cref="ItemNotFountException">Throws when a database doesn't contain user with the provided id</exception>
        public async Task<TUserDto> GetUserByIdAsync<TUserDto>(Guid id)
        {
            if (string.IsNullOrEmpty(id.ToString()))
            {
                throw new ArgumentNullException(nameof(id), "id was null");
            }

            var user = await GetUserByIdAsync(id);

            return _mapper.Map<ApplicationUser, TUserDto> (user);
        }

        /// <summary>
        /// This function returns an enumerable collection of <seealso cref="ApplicationUserDto"/> sorted by identifier that are offset by <see cref="offset"/>
        /// and contain no more than <see cref="limit"/> records
        /// </summary>
        /// <param name="limit">limit returned <seealso cref="ApplicationUserDto"/> entities, must be less equal 20</param>
        /// <param name="offset">offset searched <seealso cref="ApplicationUserDto"/> entities</param>
        /// <returns>Enumerable collection of <seealso cref="ApplicationUserDto"/></returns>
        public async Task<IEnumerable<ApplicationUserDto>> GetAllUsersAsync(int? limit, int? offset)
        {
            var limitVal = limit == null || limit > 20 ? 20 : limit.Value;
            var offsetVal = offset ?? 0;

            var users = await _unitOfWork.GetAll<ApplicationUser>(limitVal, offsetVal).ToListAsync();

            return _mapper.Map<IEnumerable<ApplicationUser>, List<ApplicationUserDto>>(users);
        }

        public async Task UpdateUserAsync(Guid id, ApplicationUserPatchModel applicationUserDto)
        {
            var user = await GetUserByIdAsync(id).ConfigureAwait(false);

            await _userManager.UpdateAsync(_mapper.Map(applicationUserDto, user));
        }

        public async Task RemoveUserAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            await _userManager.DeleteAsync(user);
        }

        private async Task<ApplicationUser> GetUserByIdAsync(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                throw new ItemNotFountException("User", $"User with following {nameof(id)} not found");
            }

            return user;
        }

        private async Task<ApplicationUser> GetUserByUsernameAsync(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                throw new ItemNotFountException("User", $"User with following {nameof(username)} not found");
            }

            return user;
        }
    }
}
