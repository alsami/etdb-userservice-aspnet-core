﻿using System.Threading.Tasks;
using AutoMapper;
using Etdb.ServiceBase.Extensions;
using Etdb.UserService.Cqrs.Abstractions.Commands.Authentication;
using Etdb.UserService.Cqrs.Abstractions.Commands.Users;
using Etdb.UserService.Presentation.Authentication;
using Etdb.UserService.Presentation.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Etdb.UserService.Controllers.V1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly IMediator bus;
        private readonly ILogger<AuthController> logger;


        public AuthController(IMediator bus, IMapper mapper, ILogger<AuthController> logger)
        {
            this.bus = bus;
            this.mapper = mapper;
            this.logger = logger;
        }

        [AllowAnonymous]
        [HttpPost("registration")]
        public async Task<IActionResult> RegistrationAsync([FromBody] UserRegisterDto dto)
        {
            if (!this.ModelState.IsValid)
                throw this.ModelState.GenerateValidationException("User register request is invalid!");

            var command = this.mapper.Map<UserRegisterCommand>(dto);

            await this.bus.Send(command);

            return this.NoContent();
        }

        [AllowAnonymous]
        [HttpPost("authentication")]
        public Task<AccessTokenDto> AuthenticationAsync([FromBody] InternalAuthenticationDto authenticationDto)
        {
            var command = this.mapper.Map<InternalAuthenticationCommand>(authenticationDto);

            return this.bus.Send(command);
        }

        [AllowAnonymous]
        [HttpPost("external-authentication")]
        public Task<AccessTokenDto> ExternalAuthenticationAsync(
            [FromBody] ExternalAuthenticationDto authenticationDto)
        {
            var command = this.mapper.Map<ExternalAuthenticationCommand>(authenticationDto);

            return this.bus.Send(command);
        }

        [AllowAnonymous]
        [HttpGet("refresh-authentication/{refreshToken}/{clientId}/{authenticationProvider}")]
        public Task<AccessTokenDto> RefreshAuthenticationAsync(string refreshToken, string clientId,
            string authenticationProvider)
            => this.bus.Send(
                new RefreshAuthenticationCommand(refreshToken, clientId, authenticationProvider));

        [HttpGet("user-identity/{accessToken}")]
        public Task<IdentityUserDto> UserIdentityAsync(string accessToken)
            => this.bus.Send(
                new IdentityUserLoadCommand(accessToken));
    }
}