using System;
using System.Windows;

namespace Cobalt.Extensibility
{
    public interface IViewLocator
    {
        UIElement GetOrCreateViewType(Type viewType);
    }
}