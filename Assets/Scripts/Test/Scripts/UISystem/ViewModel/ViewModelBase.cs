// using System;
// using System.Collections.Generic;
//
// public abstract class ViewModelBase : IViewModel
// {
//     public const int FullRefreshKey = -1;
//
//     public event Action<int> RefreshRequested;
//
//     public virtual void OnEnter() { }
//
//     public virtual void OnExit() { }
//
//     protected bool SetProperty<T>(ref T field, T value)
//     {
//         if (EqualityComparer<T>.Default.Equals(field, value))
//         {
//             return false;
//         }
//
//         field = value;
//         return true;
//     }
//
//     protected bool SetProperty<T>(ref T field, T value, int refreshKey)
//     {
//         bool changed = SetProperty(ref field, value);
//         if (changed)
//         {
//             RefreshRequested?.Invoke(refreshKey);
//         }
//
//         return changed;
//     }
//
//     protected void NotifyRefresh(int refreshKey = FullRefreshKey)
//     {
//         RefreshRequested?.Invoke(refreshKey);
//     }
//
//     protected void NotifyRefresh<TEnum>(TEnum refreshKey) where TEnum : struct, Enum
//     {
//         RefreshRequested?.Invoke(Convert.ToInt32(refreshKey));
//     }
// }