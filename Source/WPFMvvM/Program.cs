namespace WPFMvvM;

public class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        App app = new(args);
        app.Run();
    }
}

