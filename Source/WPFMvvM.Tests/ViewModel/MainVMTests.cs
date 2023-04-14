using FluentAssertions;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFMvvM.Settings;
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
        var target = new MainVM(Options.Create(opts));
        using var monitoredtarget = target.Monitor();
        await target.InitializeAsync(CancellationToken.None);

        //Assert
        target.Title.Should().Be(opts.Title);
        monitoredtarget.Should().RaisePropertyChangeFor(x => x.Title, "Title proprety change not raised.");
    }
}
