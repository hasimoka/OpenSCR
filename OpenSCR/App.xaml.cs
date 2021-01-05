using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MainWindowServices;
using System.Windows;
using Prism.Ioc;
using Prism.DryIoc;
using Prism.Modularity;
using Prism.Mvvm;
using System.Reflection;
using OptionViews;
using OpenSCRLib;

namespace OpenSCR
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : PrismApplication
    {
        /// <summary>ModuleCatalogを設定します。</summary>
        /// <param name="moduleCatalog">IModuleCatalog。</param>
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            //moduleCatalog.AddModule<StartUpViewModule>();
            //moduleCatalog.AddModule<ChildViewsModule>();
            moduleCatalog.AddModule<OptionViewsModule>();
        }

        /// <summary>ViewModelLocatorを設定します。</summary>
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();

            ViewModelLocationProvider.SetDefaultViewTypeToViewModelTypeResolver(vt =>
            {
                var viewName = vt.FullName;
                var asmName = vt.GetTypeInfo().Assembly.FullName;
                var vmName = $"{viewName}ViewModel, {asmName}";

                return Type.GetType(vmName);
            });
        }

        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IMainWindowService, MainWindowService>();

            containerRegistry.RegisterInstance(this.Container);
            containerRegistry.RegisterInstance(typeof(DatabaseAccesser), new DatabaseAccesser());
        }


    }
}
