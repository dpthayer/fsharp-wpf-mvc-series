﻿namespace Mvc.Wpf

open System.ComponentModel
open Castle.DynamicProxy

type Model() = 

    static let proxyFactory = ProxyGenerator()

    static let notifyPropertyChanged = 
        {
            new StandardInterceptor() with
                member this.PostProceed invocation = 
                    match invocation.Method.Name.Split('_'), invocation.InvocationTarget with 
                        | [| "set"; propertyName |], (:? Model as model) -> model.TriggerPropertyChanged propertyName
                        | _ -> ()
        }

    let propertyChangedEvent = Event<_,_>()

    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member this.PropertyChanged = propertyChangedEvent.Publish

    member internal this.TriggerPropertyChanged propertyName = 
        propertyChangedEvent.Trigger(this, PropertyChangedEventArgs propertyName)

    static member Create<'M when 'M :> Model and 'M : not struct>()  : 'M = 
        proxyFactory.CreateClassProxy notifyPropertyChanged
