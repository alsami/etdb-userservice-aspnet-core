﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using EntertainmentDatabase.REST.ServiceBase.DataStructure.Abstraction;

namespace EntertainmentDatabase.REST.Domain.Entities
{
    public class Movie : IEntity
    {
        public Guid Id { get; set; }

        public byte[] RowVersion
        {
            get;
            set;
        }

        public string Title { get; set; }
    }
}