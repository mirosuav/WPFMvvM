namespace CarDealer;

public class Program
{
    [STAThread]
    public static int Main(string[] args)
    {

        var app = new App(args);
        return app.Run();
    }


}

