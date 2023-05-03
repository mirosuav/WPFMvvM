﻿using CommunityToolkit.Mvvm.Messaging.Messages;

namespace WPFMvvM.Messages;

public class ViewModelRequest : RequestMessage<BaseViewModel?>
{
    public readonly Type ViewModelType;

    public ViewModelRequest(Type viewModelType)
    {
        ViewModelType = viewModelType;
    }
}
