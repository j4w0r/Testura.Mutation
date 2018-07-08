﻿using Cama.Common.Tabs;
using Cama.Module.Start.Sections.Welcome;
using Prism.Commands;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Cama.Sections.Menu
{
    public class MenuViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private readonly IModuleManager _moduleManager;
        private readonly IMainTabContainer _mainTabContainer;

        public MenuViewModel(IRegionManager regionManager, IModuleManager moduleManager, IMainTabContainer mainTabContainer)
        {
            _regionManager = regionManager;
            _moduleManager = moduleManager;
            _mainTabContainer = mainTabContainer;
            NewProjectCommand = new DelegateCommand(NewProject);
        }

        public DelegateCommand NewProjectCommand { get; set; }

        private void NewProject()
        {
            _mainTabContainer.AddTab(new WelcomeView());
            /*
            _moduleManager.LoadModule(nameof(MutationModule));
            _regionManager.RequestNavigate(RegionNames.MainContentRegion, new Uri(typeof(MutationShellView).FullName, UriKind.Relative));
            */
        }

    }
}
