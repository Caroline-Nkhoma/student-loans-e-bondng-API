﻿using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using StudentLoanseBonderAPI.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentLoanseBonderAPI.Services;

public class AccountService
{
	private readonly ILogger<AccountService> _logger;
	private readonly UserManager<IdentityUser> _userManager;
	private readonly SignInManager<IdentityUser> _signInManager;
	private readonly IConfiguration _configuration;
	private readonly ApplicationDbContext _dbContext;

	public AccountService(ILogger<AccountService> logger, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IConfiguration configuration, ApplicationDbContext dbContext)
	{
		_logger = logger;
		_userManager = userManager;
		_signInManager = signInManager;
		_configuration = configuration;
		_dbContext = dbContext;
	}

	public async Task<IdentityUser?> FindOneByEmail(string email)
	{
		_logger.LogInformation($"Finding account with email {email}");
		var user = await _userManager.FindByEmailAsync(email);

		return user;
	}

	public async Task<IdentityUser?> FindOneById(string accountId)
	{
		_logger.LogInformation($"Finding account with id {accountId}");
		var user = await _userManager.FindByIdAsync(accountId);

		return user;
	}

	public async Task<IdentityResult> AssignRole(IdentityUser user, string roleName)
	{
		_logger.LogInformation($"Attempt to add account with id {user.Id} to {roleName} role");
		var result = await _userManager.AddToRoleAsync(user, roleName);
		return result;
	}

	public async Task<IdentityResult> Register(UserCredentials userCredentials)
	{
		_logger.LogInformation($"Attempt to register {userCredentials.Email}");

		var user = new IdentityUser
		{
			UserName = userCredentials.Email,
			Email = userCredentials.Email,
		};

		var result = await _userManager.CreateAsync(user, userCredentials.Password);
		return result;
	}

	public async Task<SignInResult> Login(UserCredentials userCredentials)
	{
		_logger.LogInformation($"Attempt to log into {userCredentials.Email}");

		// may want to set lockoutOnFailure to be true in production
		return await _signInManager.PasswordSignInAsync(userCredentials.Email, userCredentials.Password, isPersistent: false, lockoutOnFailure: false);
	}

	internal async Task<AuthenticationResponse> BuildToken(UserCredentials userCredentials)
	{
		var claims = new List<Claim>()
		{
			new ("email", userCredentials.Email)
		};

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTKey"]!));
		var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var expiration = DateTime.UtcNow.AddMonths(1);

		var token = new JwtSecurityToken(issuer: null, audience: null, claims: claims, expires: expiration, signingCredentials: creds);

		var user = await FindOneByEmail(userCredentials.Email);

		return new AuthenticationResponse
		{
			AccountId = user.Id,
			Token = new JwtSecurityTokenHandler().WriteToken(token),
			Expiration = expiration,
		};
	}
}
