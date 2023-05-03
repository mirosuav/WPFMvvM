using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;
using WPFMvvM.Messages;

namespace WPFMvvM.Extensions;

public static class AppScopeExtensions
{
    public static TViewModel RequestViewModel<TViewModel>(this IAppScope scope) where TViewModel : BaseViewModel
    {
        var request = new ViewModelRequest(typeof(TViewModel));
        var result = scope.SendMessage(request).Response as TViewModel;
        Guard.IsNotNull(result, $"Could not resolve ViewModel of type {typeof(TViewModel)}!");
        return result;
    }
}
