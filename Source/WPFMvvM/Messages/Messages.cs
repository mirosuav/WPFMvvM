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
public record PromptMessage(string Title, string Message);

/// <summary>
/// Show question to user and Ok, No options
/// </summary>
public class QuestionMessage: RequestMessage<WindowResult>
{
    public readonly string Title;
    public readonly string Question;

    public QuestionMessage(string title, string message)
    {
        Title = title;
        Question = message;
    }
}
