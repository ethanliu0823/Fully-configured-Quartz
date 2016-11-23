namespace QuartzExtScheduler.Service
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.QuartzExtServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.QuartzExtServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // QuartzExtServiceProcessInstaller
            // 
            this.QuartzExtServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.QuartzExtServiceProcessInstaller.Password = null;
            this.QuartzExtServiceProcessInstaller.Username = null;
            // 
            // QuartzExtServiceInstaller
            // 
            this.QuartzExtServiceInstaller.Description = "云采购调度服务";
            this.QuartzExtServiceInstaller.DisplayName = "QuartzExtService";
            this.QuartzExtServiceInstaller.ServiceName = "QuartzExtService";
            this.QuartzExtServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.QuartzExtServiceProcessInstaller,
            this.QuartzExtServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller QuartzExtServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller QuartzExtServiceInstaller;
    }
}