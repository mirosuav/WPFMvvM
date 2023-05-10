using CommunityToolkit.Mvvm.Messaging;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReceivedExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPFMvvM.Framework.Messages;
using WPFMvvM.Framework.UI;
using WPFMvvM.Framework.ViewModel;

namespace WPFMvvM.Framework.Tests.UI;

public class WindowBinderTests
{
    private readonly IMessenger messenger;
    private readonly WindowBinder target;
    private readonly IAppScope appScope;
    private readonly BaseWindowModel viewModel;

    public WindowBinderTests()
    {
        messenger = Substitute.For<IMessenger>();
        target = new(messenger);
        appScope = Substitute.For<IAppScope>();
        viewModel = Substitute.For<BaseWindowModel>(appScope);
    }

    [StaFact]
    public void Bind_Should_CreateWindowEvenentsBindings()
    {
        //ARRANGE
        var window = new Window();

        //ACT
        target.BindEvents(window, viewModel);
        window.Activate();
        viewModel.Received(1).ActivateCommand.ExecuteAsync(null);

        window.Close();
        viewModel.Received(1).CloseCommand.ExecuteAsync(null);
        messenger.Received(1).UnregisterAll(window);
        window.IsActive.Should().BeFalse();

        //ENSURE events are cleared
        using var windowMonitor = window.Monitor();

        window.Activate();
        windowMonitor.Should().NotRaise(nameof(Window.Activated));

        window.Close();
        windowMonitor.Should().NotRaise(nameof(Window.Closing));
        windowMonitor.Should().NotRaise(nameof(Window.Closed));
    }


    [StaFact]
    public async Task Bind_Should_SubscribeToMessenger()
    {
        //ARRANGE
        var window = new Window();
        using var windowMonitor = window.Monitor();

        //ACT
        target.BindEvents(window, viewModel);
        (await messenger.Send(new WindowCloseRequests(), viewModel.WindowRequestToken))
            .Should().Be(true);
#warning messenger send not susbtituted for specific call

        //ASSERT
        windowMonitor.Should().NotRaise(nameof(Window.Closed));


    }


}
