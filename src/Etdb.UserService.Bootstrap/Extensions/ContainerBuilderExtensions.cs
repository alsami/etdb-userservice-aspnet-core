﻿using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.FluentBuilder;
using AutoMapper.Extensions.Autofac.DependencyInjection;
using Elders.RedLock;
using Etdb.ServiceBase.Cqrs.Abstractions.Bus;
using Etdb.ServiceBase.Cqrs.Abstractions.Validation;
using Etdb.ServiceBase.Cqrs.Bus;
using Etdb.ServiceBase.Cryptography.Abstractions.Hashing;
using Etdb.ServiceBase.Cryptography.Hashing;
using Etdb.ServiceBase.DocumentRepository;
using Etdb.ServiceBase.DocumentRepository.Abstractions;
using Etdb.ServiceBase.Services;
using Etdb.ServiceBase.Services.Abstractions;
using Etdb.UserService.Authentication.Abstractions.Strategies;
using Etdb.UserService.Authentication.Services;
using Etdb.UserService.Authentication.Strategies;
using Etdb.UserService.Authentication.Validator;
using Etdb.UserService.AutoMapper.Profiles;
using Etdb.UserService.Bootstrap.Configuration;
using Etdb.UserService.Cqrs.Handler;
using Etdb.UserService.Domain.Enums;
using Etdb.UserService.Repositories;
using Etdb.UserService.Services;
using Etdb.UserService.Services.Abstractions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Validation;
using MediatR.Extensions.Autofac.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Etdb.UserService.Bootstrap.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static void SetupDependencies(this ContainerBuilder containerBuilder, IHostingEnvironment environment,
            IConfiguration configuration)
        {
            new AutofacFluentBuilder(containerBuilder
                    .AddMediatR(typeof(UserRegisterCommandHandler).Assembly)
                    .AddAutoMapper(typeof(UsersProfile).Assembly))
                .RegisterResolver<RedisLockManager, IRedisLockManager>(RedisLockManagerResolver)
                .RegisterResolver(ExternalAuthenticationStrategyResolver)
                .RegisterInstance<IHostingEnvironment>(environment)
                .RegisterInstance<IConfiguration>(configuration)
                .RegisterTypeAsSingleton<ContextLessRouteProvider>()
                .RegisterTypeAsSingleton<UserProfileImageUrlFactory, IUserProfileImageUrlFactory>()
                .RegisterTypeAsSingleton<ActionContextAccessor, IActionContextAccessor>()
                .RegisterTypeAsSingleton<Hasher, IHasher>()
                .RegisterTypeAsSingleton<FileService, IFileService>()
                .RegisterTypeAsSingleton<CorsPolicyService, ICorsPolicyService>()
                .RegisterTypeAsSingleton<UserServiceDbContext, DocumentDbContext>()
                .RegisterTypeAsScoped<Bus, IBus>()
                .RegisterTypeAsScoped<HttpContextAccessor, IHttpContextAccessor>()
                .RegisterTypeAsScoped<ProfileService, IProfileService>()
                .RegisterTypeAsScoped<ResourceOwnerPasswordGrandValidator, IResourceOwnerPasswordValidator>()
                .RegisterTypeAsScoped<GoogleAuthenticationStrategy, IGoogleAuthenticationStrategy>()
                .RegisterTypeAsScoped<FacebookAuthenticationStrategy, IFacebookAuthenticationStrategy>()
                .RegisterTypeAsScoped<ExternalGrantValidator, IExtensionGrantValidator>()
                .RegisterTypeAsScoped<CachedGrantStoreService, IPersistedGrantStore>()
                .RegisterTypeAsScoped<UsersService, IUsersService>()
                .RegisterTypeAsScoped<ResourceLockingAdapter, IResourceLockingAdapter>()
                .AddClosedTypeAsScoped(typeof(ICommandValidation<>),
                    new[] {typeof(UserRegisterCommandHandler).Assembly})
                .AddClosedTypeAsScoped(typeof(IDocumentRepository<,>), new[] {typeof(UserServiceDbContext).Assembly});
        }

        private static IRedisLockManager RedisLockManagerResolver(IComponentContext componentContext) =>
            new RedisLockManager(new RedLockOptions
            {
                LockRetryCount = 2
            }, componentContext.Resolve<IOptions<RedisConfiguration>>().Value.Connection); 

        private static IExternalAuthenticationStrategy ExternalAuthenticationStrategyResolver(IComponentContext componentContext,
            IEnumerable<Parameter> @params)
        {
            var provider = @params.TypedAs<AuthenticationProvider>();

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (provider)
            {
                case AuthenticationProvider.Google:
                {
                    return componentContext.Resolve<IGoogleAuthenticationStrategy>();
                }
                case AuthenticationProvider.Facebook:
                {
                    return componentContext.Resolve<IFacebookAuthenticationStrategy>();
                }
                case AuthenticationProvider.Twitter:
                {
                    throw new NotImplementedException();
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider));
            }
        }
    }
}