using CommunityToolkit.Mvvm.Messaging.Messages;
using WPFMvvM.ViewModel;

namespace WPFMvvM.Messages;

public record CarListNavigation;
public record NewCarNavigation;
public record AboutNavigation;
public record CarEditNavigation(int CarId);

/// <summary>
/// Show message to user and sidplay Ok button
/// </summary>
public record ShowMessage(string title, string message);

/// <summary>
/// Show question to user and Ok, No options
/// </summary>
public class QuestionMessage: RequestMessage<WindowResult>
{
    public readonly string Title;
    public readonly string Message;

    public QuestionMessage(string title, string message)
    {
        Title = title;
        Message = message;
    }
}
