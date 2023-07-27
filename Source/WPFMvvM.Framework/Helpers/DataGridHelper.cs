using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace WPFMvvM.Framework.Helpers;


public static class DataGridHelper
{
    #region EnableFixForDataGridContext - Assign DataGrid's DataContext to all its columns 

    public static void EnableFixForDataGridContext()
    {
        DependencyProperty dp = FrameworkElement.DataContextProperty.AddOwner(typeof(DataGridColumn));
        FrameworkElement.DataContextProperty.OverrideMetadata(typeof(DataGrid),
        new FrameworkPropertyMetadata
           (null, FrameworkPropertyMetadataOptions.Inherits,
           new PropertyChangedCallback(OnDataContextChanged)));
    }

    public static void OnDataContextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var grid = d as DataGrid;
        if (grid != null)
        {
            //set new DataContext in each column including template columns
            foreach (DataGridColumn col in grid.Columns)
            {
                col.SetValue(FrameworkElement.DataContextProperty, e.NewValue);
            }
        }
    }

    #endregion




    #region RowCommandProperty 


    public static ICommand GetRowCommand(DependencyObject obj) => (ICommand)obj.GetValue(RowCommandProperty);

    public static void SetRowCommand(DependencyObject obj, ICommand value) => obj.SetValue(RowCommandProperty, value);

    /// <summary>
    /// RowCommand property handles the row click event in DataGrid control.
    /// The command is fired with the DataContext of the cliecked row as parameter.
    /// </summary>
    public static readonly DependencyProperty RowCommandProperty =
        DependencyProperty.RegisterAttached("RowCommand", typeof(ICommand), typeof(DataGridHelper), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(RowCommandPropertyChanged)));


    private static void RowCommandPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dg = d as DataGrid ?? throw new ApplicationException("RowCommandProperty can be set on DataGrid control only!");
        if (e.NewValue != null)
        {
            WeakEventManager<DataGrid, MouseButtonEventArgs>.AddHandler(dg, nameof(DataGrid.MouseLeftButtonUp), DataGrid_MouseLeftButtonUp);
            dg.Cursor = Cursors.Hand;
        }
        else
        {
            WeakEventManager<DataGrid, MouseButtonEventArgs>.RemoveHandler(dg, nameof(DataGrid.MouseLeftButtonUp), DataGrid_MouseLeftButtonUp);
        }
    }

    private static void DataGrid_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        var dg = sender as DataGrid ?? throw new ApplicationException("RowCommandProperty can be set on DataGrid control only!");
        var command = GetRowCommand(dg);
        //skip if command is not defined
        if (command == null) return;

        var dep = (DependencyObject)e.OriginalSource;
        //skip if there was a button within a row pressed
        if (dep is ButtonBase) return;
        var row = VisualHelper.FindVisualParent<DataGridRow>(dep, true);
        if (row != null)
        {
            if (command.CanExecute(row.DataContext))
                command.Execute(row.DataContext);
        }
    }


    #endregion

}
