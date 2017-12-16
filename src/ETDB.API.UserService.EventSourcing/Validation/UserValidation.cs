﻿using System;
using Etdb.UserService.EventSourcing.Commands;
using Etdb.UserService.Repositories.Abstractions;
using ETDB.API.ServiceBase.EventSourcing.Validation;
using FluentValidation;

namespace Etdb.UserService.EventSourcing.Validation
{
    public abstract class UserValidation<TCommand> : CommandValidation<TCommand> where TCommand : UserCommand
    {
        private readonly IUserRepository userRepository;

        protected UserValidation(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        protected void ValidateUserNameAndEmailNotTaken()
        {
            this.RuleFor(user => user)
                .Must(this.IsUnique)
                .WithMessage("Username or email addresss already taken!");
        }

        protected void ValidateUserName()
        {
            this.RuleFor(user => user.UserName)
                .NotEmpty()
                .WithMessage("Username may not be empty!")
                .NotNull()
                .WithMessage("Username may not be null!")
                .NotEqual("Admin", StringComparer.OrdinalIgnoreCase)
                .NotEqual("Administrator", StringComparer.OrdinalIgnoreCase)
                .WithMessage("Username blacklisted!")
                .MinimumLength(6)
                .WithMessage("Username must have at least six letters!")
                .MaximumLength(32)
                .WithMessage("Username may not be longer than 32 characters");
        }

        protected void ValidateEmail()
        {
            this.RuleFor(user => user.Email)
                .NotEmpty()
                .WithMessage("Email address may not be empty!")
                .NotNull()
                .WithMessage("Email may not be null!")
                .EmailAddress()
                .WithMessage("Email must be valid!");
        }

        protected void ValidateName()
        {
            this.RuleFor(user => user.Name)
                .NotEmpty()
                .NotNull()
                .WithMessage("Name must be given!");
        }

        protected void ValidateLastName()
        {
            this.RuleFor(user => user.LastName)
                .NotEmpty()
                .NotNull()
                .WithMessage("Lastname must be given!");
        }

        protected void ValidatePassword()
        {
            this.RuleFor(user => user.Password)
                .NotEmpty()
                .NotNull()
                .WithMessage("Password must be given!")
                .Length(8, 64)
                .WithMessage("Password must have 8 to 64 characters");
        }

        protected void ValidateRowVersion()
        {
            this.RuleFor(user => user.RowVersion)
                .NotEmpty()
                .NotNull()
                .WithMessage("Concurrency token must be given!");
        }

        private bool IsUnique(UserCommand command)
        {
            var existingUser = this.userRepository
                .Find(command.UserName, command.Email);

            if (existingUser == null)
            {
                return true;
            }

            return existingUser.Id == command.Id;
        }
    }
}
