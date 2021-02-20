using MainWindowServices;
using OpenSCRLib;
using OptionViews;
using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using System;
using System.Reflection;
using System.Windows;
using VideoViews;

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
            moduleCatalog.AddModule<VideoViewsModule>();
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
