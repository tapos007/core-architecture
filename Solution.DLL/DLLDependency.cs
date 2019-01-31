using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Solution.DLL.Identity.Repository;
using Solution.DLL.Repository;

namespace Solution.DLL
{
   public static class DLLDependency
    {

        public static void  Dependency(IServiceCollection services)
        {
           
            services.AddTransient<ITestRepository, TestRepository>();
           // services.AddSingleton<IElasticHelper, ElasticHelper>();
            services.AddTransient<IUserRepository, UserRepository>();
        }
    }
}
