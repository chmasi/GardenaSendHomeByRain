namespace MowerRainSteering
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.GardenaServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.GardenaServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // GardenaServiceProcessInstaller
            // 
            this.GardenaServiceProcessInstaller.Password = null;
            this.GardenaServiceProcessInstaller.Username = null;
            this.GardenaServiceProcessInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.GardenaServiceProcessInstaller_AfterInstall);
            // 
            // GardenaServiceInstaller
            // 
            this.GardenaServiceInstaller.Description = "Check for rain and return mower to docking station";
            this.GardenaServiceInstaller.DisplayName = "MowerRainSteering";
            this.GardenaServiceInstaller.ServiceName = "MowerRainSteering";
            this.GardenaServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            this.GardenaServiceInstaller.AfterInstall += new System.Configuration.Install.InstallEventHandler(this.GardenaServiceInstaller_AfterInstall);
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.GardenaServiceProcessInstaller,
            this.GardenaServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller GardenaServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller GardenaServiceInstaller;
    }
}