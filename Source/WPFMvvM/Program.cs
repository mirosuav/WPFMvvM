namespace WPFMvvM;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App app = new App(args);
        app.InitializeComponent();
        app.Run();
    }
}

