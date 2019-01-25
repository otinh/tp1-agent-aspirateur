namespace tp1_agent_aspirateur
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            
            var environment = new Environment();
            environment.start();
            
            var agent = new Agent();
            agent.start();
        }
    }
}