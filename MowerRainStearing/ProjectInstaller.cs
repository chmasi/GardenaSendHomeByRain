using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.Threading.Tasks;

namespace MowerRainSteering
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void GardenaServiceInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }

        private void GardenaServiceProcessInstaller_AfterInstall(object sender, InstallEventArgs e)
        {

        }
    }
}
