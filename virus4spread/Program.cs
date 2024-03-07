
namespace virus4spread;

internal static class Program
{
  
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]

    static void Main()
    {
        // settings for fastbitmap
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        // To customize application configuration such as set high DPI settings or default font,
        // see aka.ms/applicationconfiguration.            
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}