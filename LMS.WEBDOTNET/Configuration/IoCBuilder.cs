using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Autofac;

namespace LMS.WEBDOTNET.Configuration
{
    public class IoCBuilder
    {
        public static IContainer Build()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Repository.Repositories.Auth.AuthRepository>().As<Repository.Interfaces.Auth.IAuthRepository>();
            builder.RegisterType<Repository.Repositories.Employee.LeaveRepository>().As<Repository.Interfaces.Employee.ILeaveRepository>();
            builder.RegisterType<Repository.Repositories.Admin.ManageLeavesRepository>().As<Repository.Interfaces.Admin.IManageLeavesRepository>();
            return builder.Build();
        }

    }
}