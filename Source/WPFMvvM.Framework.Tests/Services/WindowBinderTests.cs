using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using System.Windows;
using WPFMvvM.Framework.Exceptions;
using WPFMvvM.Framework.Messages;
using WPFMvvM.Framework.Services;
using WPFMvvM.Framework.ViewModel;

namespace WPFMvvM.Framework.Tests.Services;

public class WindowBinderTests
{
    private readonly IMessenger messenger;
    private readonly IExceptionHandler exceptionHandler;
    private readonly WindowBinder target;
    private readonly IAppScope appScope;
    private int errorsHandled;

    public WindowBinderTests()
    {
        messenger = new StrongReferenceMessenger();
        exceptionHandler = Substitute.For<IExceptionHandler>();
        exceptionHandler
            .When(h => h.Handle(Arg.Is(LogLevel.Error), Arg.Any<string>(), Arg.Any<Exception?>()))
            .Do(x => errorsHandled++);

        target = new WindowBinder(messenger, exceptionHandler);
        appScope = Substitute.For<IAppScope>();
    }

    private BaseWindowModel CreateViewModel()
    {
        var viewModel = Substitute.For<BaseWindowModel>(appScope);
        viewModel.CanClose(Arg.Any<WindowClosingRequest>(), CancellationToken.None).ReturnsForAnyArgs(true);
        return viewModel;
    }

    private Window CreateWindow()
    {
        var window = new Window();
        return window;
    }

    private WeakReference CreateWeakRefWindow()
    {
        return new WeakReference(CreateWindow());
    }

    [StaFact]
    public void Bind_OnWindowActivation_MustCallActivateCommand()
    {
        //ARRANGE
        var viewModel = CreateViewModel();
        var window = CreateWindow();

        //ACT
        target.BindViewModel(window, viewModel);
        window.Activate();

        //ASSERT
        viewModel.Received(1).ActivateCommand.ExecuteAsync(null);
        viewModel.DidNotReceiveWithAnyArgs().CanClose(Arg.Any<WindowClosingRequest>(), Arg.Any<CancellationToken>());
        viewModel.DidNotReceiveWithAnyArgs().CloseCommand.ExecuteAsync(Arg.Any<WindowClosingRequest>());
        errorsHandled.Should().Be(0);
        window.Close();
    }


    [StaFact]
    public void Bind_OnWindowClose_MustCallCloseCommand()
    {
        //ARRANGE
        var viewModel = CreateViewModel();
        var window = CreateWindow();

        //ACT
        target.BindViewModel(window, viewModel);
        window.Activate();
        window.Close();

        //ASSERT
        viewModel.Received(1).CanClose(Arg.Is<WindowClosingRequest>(r => r.UITriggered), Arg.Any<CancellationToken>());
        viewModel.Received(1).CloseCommand.ExecuteAsync(Arg.Is<WindowClosingRequest>(r => r.UITriggered));
        errorsHandled.Should().Be(0);
    }

    [StaFact]
    public void Bind_OnWindowCloseRequest_MustClose()
    {
        //ARRANGE
        var viewModel = CreateViewModel();
        var window = CreateWindow();
        using var monitoredWindow = window.Monitor();

        //ACT
        target.BindViewModel(window, viewModel);
        window.Activate();
        var response = messenger.Send(new SelfWindowCloseRequest(), viewModel.WindowRequestToken);

        //ASSERT
        viewModel.DidNotReceive()
            .CloseCommand
            .ExecuteAsync(Arg.Any<WindowClosingRequest>());

        response.Response.Result.Should().BeTrue();
        monitoredWindow.Should().Raise(nameof(window.Closing));
        monitoredWindow.Should().Raise(nameof(window.Closed));
        errorsHandled.Should().Be(0);
    }


    [StaFact]
    public void Bind_WhenCloseIsPrevented_MustNotCloseWindow()
    {
        //ARRANGE
        var viewModel = CreateViewModel();
        viewModel
            .CanClose(Arg.Any<WindowClosingRequest>(), CancellationToken.None)
            .ReturnsForAnyArgs(false);

        var window = CreateWindow();

        //ACT
        target.BindViewModel(window, viewModel);
        window.Activate();
        window.Close();

        //ASSERT
        viewModel.Received(1)
            .CanClose(Arg.Any<WindowClosingRequest>(), Arg.Any<CancellationToken>());

        viewModel.DidNotReceive()
            .CloseCommand
            .ExecuteAsync(Arg.Any<WindowClosingRequest>());

        messenger.IsRegistered<SelfWindowCloseRequest, Guid>(window, viewModel.WindowRequestToken)
            .Should()
            .BeTrue();

        errorsHandled.Should().Be(0);
    }


    [StaFact]
    public void Bind_AfterClose_ShouldReleaseEvents()
    {
        //ARRANGE
        var viewModel = CreateViewModel();
        var window = CreateWindow();

        //ACT
        target.BindViewModel(window, viewModel);
        window.Activate();
        window.Close();

        //ASSERT
        viewModel.ReceivedWithAnyArgs(1)
            .CloseCommand
            .ExecuteAsync(null);

        messenger.IsRegistered<SelfWindowCloseRequest, Guid>(window, viewModel.WindowRequestToken)
            .Should()
            .BeFalse();

        window.IsActive.Should().BeFalse();
        errorsHandled.Should().Be(0);
    }



    [StaFact]
    public void Bind_Should_SubscribeToMessenger()
    {
        //ARRANGE
        var viewModel = CreateViewModel();
        var window = CreateWindow();

        //ACT
        target.BindViewModel(window, viewModel);

        //ASSERT
        messenger.IsRegistered<SelfWindowCloseRequest, Guid>(window, viewModel.WindowRequestToken)
            .Should()
            .BeTrue();

    }

    [StaFact]
    public void Bind_AfterClose_WindowMustBeGCed()
    {
        //ARRANGE
        var viewModel = CreateViewModel();
        WeakReference window = CreateWeakRefWindow();

        //ACT
        bindActivateAndCloseWindow(window, viewModel);

        //ASSERT
        GC.Collect();
        window.IsAlive.Should().BeFalse();
        errorsHandled.Should().Be(0);
    }


    [StaFact]
    public void Bind_OnWndowCloseRequest_MustCloseOnlyMe()
    {
        //ARRANGE
        var viewModel1 = CreateViewModel();
        var window1 = CreateWindow();
        var viewModel2 = CreateViewModel();
        var window2 = CreateWindow();
        using var monitoredWindow1 = window1.Monitor();
        using var monitoredWindow2 = window2.Monitor();

        //ACT
        target.BindViewModel(window1, viewModel1);
        window1.Activate();
        target.BindViewModel(window2, viewModel2);
        window2.Activate();

        var response = messenger.Send(new SelfWindowCloseRequest(), viewModel1.WindowRequestToken);

        //ASSERT

        monitoredWindow1.Should().Raise(nameof(window1.Closing));
        monitoredWindow1.Should().Raise(nameof(window1.Closed));

        monitoredWindow2.Should().NotRaise(nameof(window2.Closing));
        monitoredWindow2.Should().NotRaise(nameof(window2.Closed));

        response.Response.Result.Should().BeTrue();

        errorsHandled.Should().Be(0);
    }

    private void bindActivateAndCloseWindow(WeakReference wr, BaseWindowModel viewModel)
    {
        var window = wr.Target as Window;
        window.Should().NotBeNull();
        target.BindViewModel(window!, viewModel);
        window!.Activate();
        window!.Close();
    }
}
