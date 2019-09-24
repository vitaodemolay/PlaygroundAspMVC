// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StructuremapMvc.cs" company="Web Advanced">
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

using PubliAX.Web.App_Start;

using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(StructuremapMvc), "Start")]
[assembly: ApplicationShutdownMethod(typeof(StructuremapMvc), "End")]

namespace PubliAX.Web.App_Start {
	using System.Web.Mvc;

    using Microsoft.Web.Infrastructure.DynamicModuleHelper;

	using PubliAX.Web.DependencyResolution;

    using StructureMap;
    using System.Linq;

    public static class StructuremapMvc {
        #region Public Properties

        public static StructureMapDependencyScope StructureMapDependencyScope { get; set; }

        #endregion
		
		#region Public Methods and Operators
		
		public static void End() {
            StructureMapDependencyScope.Dispose();
        }
		
        public static void Start() {
            IContainer container = IoC.Initialize();
            StructureMapDependencyScope = new StructureMapDependencyScope(container);
            DependencyResolver.SetResolver(StructureMapDependencyScope);
            DynamicModuleUtility.RegisterModule(typeof(StructureMapScopeModule));

            // SqlExecutor
            var context = container.GetInstance<System.Data.Entity.DbContext>();
            var sqlExecutor = new PubliAX.EF.Persistence.SqlExecutor.PubliAxSqlExec();
            var _log = container.GetInstance<VMRCPACK.Infrastructure.Interfaces.Logger.ILogger>();
            sqlExecutor.GetViews().ToList().ForEach(item => {
                try
                {
                    context.Database.ExecuteSqlCommand(item);
                }
                catch (System.Exception ex)
                {
                    if (_log != null)
                        _log.Error(VMRCPACK.Infrastructure.Helpers.Tools.Util.CompileExceptionObjectOnOneString(ex, string.Empty));
                }
            });
        }

        #endregion
    }
}