using Microsoft.Extensions.Options;
using WPFMvvM.Common;
using WPFMvvM.Utils;
using WPFMvvM.ViewModel;

namespace WPFMvvM.Tests.ViewModel;

public class MainVMTests
{
    [Fact]
    public async Task MainVM_OnInitialized_ShouldHaveTitle()
    {
        //Arrenge
        var opts = new GeneralSettings { Title = "Hello Tests!" };

        //Act
        var target = new MainWindowModel(Substitute.For<IAppScope>(), Options.Create(opts));
        using var monitoredtarget = target.Monitor();
        await target.Initialize(CancellationToken.None);

        //Assert
        target.Title.Should().Be(opts.Title);
        monitoredtarget.Should().RaisePropertyChangeFor(x => x.Title, "Title proprety change not raised.");
    }
}
