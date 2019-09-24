// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PubliAX.Web.DependencyResolution {
    using MediatR;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using PubliAX.Domain.DTO;
    using PubliAX.Domain.Entitys;
    using PubliAX.EF.Persistence.Context;
    using PubliAX.EF.Persistence.Repository;
    using PubliAX.EF.Persistence.UnitOfWork;
    using PubliAX.Logger;
    using PubliAX.Messanger;
    using PubliAX.Repository.Dapper.Banco;
    using PubliAX.Repository.Dapper.BlockedPerson;
    using PubliAX.Repository.Dapper.ExternalAuthentication;
    using PubliAX.Repository.Dapper.Fornecedor;
    using PubliAX.Repository.Dapper.Invite;
    using PubliAX.Repository.Dapper.Logger;
    using PubliAX.Repository.Dapper.Notification;
    using PubliAX.Repository.Dapper.Pagamento;
    using PubliAX.Repository.Dapper.PedidoProducao;
    using PubliAX.Repository.Dapper.User;
    using PubliAX.Web.Configs.Identity;
    using StructureMap.Configuration.DSL;
    using StructureMap.Graph;
    using StructureMap.Pipeline;
    using System;
    using System.Configuration;
    using System.Data.Entity;
    using VMRCPACK.Infrastructure.Interfaces.Dapper;
    using VMRCPACK.Infrastructure.Interfaces.Logger;
    using VMRCPACK.Infrastructure.Interfaces.Message;
    using VMRCPACK.Infrastructure.Interfaces.Repository;
    using VMRCPACK.Infrastructure.Interfaces.UnitOfWork;

    public class DefaultRegistry : Registry {
        #region Constructors and Destructors

        public DefaultRegistry() {
            Scan(
                scan => {
                    scan.TheCallingAssembly();
                    scan.WithDefaultConventions();
					scan.With(new ControllerConvention());

                    //injeção de commands e handlers
                    scan.AssemblyContainingType<CQRS.User.Register.UserRegisterCommand>();
                    scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>));
                    scan.ConnectImplementationsToTypesClosing(typeof(IRequestHandler<>));
                    scan.ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>));
                });
            
            For<IMediator>().LifecycleIs<TransientLifecycle>().Use<Mediator>();
            For<ServiceFactory>().Use<ServiceFactory>(ctx => ctx.GetInstance);

            var connectionStringPubliAx = ConfigurationManager.ConnectionStrings["publiAxDb"].ConnectionString;
            var connectionStringPubli = ConfigurationManager.ConnectionStrings["publiDb"].ConnectionString;

            For<DbContext>().Use<PubliAxContex>().Ctor<String>("nameOrConnectionString").Is(connectionStringPubliAx)
                                                 .Ctor<string>("schema").Is(string.Empty);
            For<IRepositoryWrite<User, Guid>>().Use<UserRepository>();
            For<IRepositoryWrite<Invite, Guid>>().Use<InviteRepository>();
            For<IRepositoryWrite<Notification, Guid>>().Use<NotificationRepository>();
            For<IRepositoryWrite<BlockedPerson, Guid>>().Use<BlockedPersonRepository>();
            For<IRepositoryWrite<ExternalAuthenticator, Guid>>().Use<ExternalAuthenticationRepository>();
            For<IUnitOfWork>().Use<UowPubliAx>();

            For<IDapperWrite<LoggerModel>>(Lifecycles.Singleton).Use<LoggerRepository>().Ctor<string>("connectionstring").Is(connectionStringPubliAx);
            For<IDapperWrite<Domain.PubliEntitys.Fornecedor>>(Lifecycles.Singleton).Use<FornecedorRepositoryWrite>().Ctor<string>("connectionstring").Is(connectionStringPubli);
            For<IDapperWrite<Domain.DTO.ChaveDto>>(Lifecycles.Singleton).Use<ChavesRepositoryDto>().Ctor<string>("connectionstring").Is(connectionStringPubli);
            For<IDapperWrite<Domain.DTO.PedidoProducaoDto>>(Lifecycles.Singleton).Use<PedidoProducaoRepositoryDto>().Ctor<string>("connectionstring").Is(connectionStringPubli);
            For<IDapper<UserDto>>(Lifecycles.Singleton).Use<UserDTORepository>().Ctor<string>("connectionstring").Is(connectionStringPubliAx);
            For<IDapper<NotificationDto>>(Lifecycles.Singleton).Use<NotificationDtoRepository>().Ctor<string>("connectionstring").Is(connectionStringPubliAx);
            For<IDapper<InviteDto>>(Lifecycles.Singleton).Use<InviteDtoRepository>().Ctor<string>("connectionstring").Is(connectionStringPubliAx);
            For<IDapper<FornecedorDto>>(Lifecycles.Singleton).Use<FornecedorDtoRepository>().Ctor<string>("connectionstring").Is(connectionStringPubli);
            For<IDapper<BancoDto>>(Lifecycles.Singleton).Use<BancoDtoRepository>().Ctor<string>("connectionstring").Is(connectionStringPubli);
            For<IDapper<PagamentoDto>>(Lifecycles.Singleton).Use<PagamentoDtoRepository>().Ctor<string>("connectionstring").Is(connectionStringPubli);
            For<IDapper<PgtosConsecutivosDto>>(Lifecycles.Singleton).Use<PagamentosConsecutivosDtoRepository>().Ctor<string>("connectionstring").Is(connectionStringPubli);
            For<IDapper<BlockedPersonDto>>(Lifecycles.Singleton).Use<BlockedPersonDtoRepository>().Ctor<string>("connectionstring").Is(connectionStringPubliAx);
            For<IDapper<ExternalAuthenticationDto>>(Lifecycles.Singleton).Use<ExternalAuthenticationDtoRepository>().Ctor<string>("connectionstring").Is(connectionStringPubliAx);

            For<ILogger>().Use<LoggerService>();
            For<IMsgService>().Use<MsgService>();


            For<ApplicationSignInManager>().Use<ApplicationSignInManager>();
            For<SignInManager<ApplicationUser, string>>().Use<ApplicationSignInManager>();
            For<ApplicationUserManager>().Use<ApplicationUserManager>();
            For<UserManager<ApplicationUser, string>>().Use<ApplicationUserManager>();
            For<ApplicationUserStore>().Use<ApplicationUserStore>();
            For<IUserStore<ApplicationUser>>().Use<ApplicationUserStore>();
        }

        #endregion
    }
}