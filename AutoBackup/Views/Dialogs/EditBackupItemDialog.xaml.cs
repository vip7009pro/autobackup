using System.Windows;
using AutoBackup.Models;
using AutoBackup.ViewModels;

namespace AutoBackup.Views.Dialogs;

/// <summary>
/// Interaction logic for EditBackupItemDialog.xaml
/// </summary>
public partial class EditBackupItemDialog : Window
{
    private readonly EditBackupItemViewModel _viewModel;

    public BackupItem? Result { get; private set; }

    public EditBackupItemDialog()
    {
        InitializeComponent();
        _viewModel = new EditBackupItemViewModel();
        DataContext = _viewModel;

        _viewModel.SaveRequested += OnSaveRequested;
        _viewModel.CancelRequested += OnCancelRequested;
    }

    public void LoadItem(BackupItem? item)
    {
        _viewModel.LoadItem(item);
        Title = item == null ? "Add Backup Item" : "Edit Backup Item";
    }

    private void OnSaveRequested(object? sender, BackupItem item)
    {
        Result = item;
        DialogResult = true;
        Close();
    }

    private void OnCancelRequested(object? sender, EventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.IsValid)
        {
            _viewModel.SaveCommand.Execute(null);
        }
        else
        {
            MessageBox.Show(_viewModel.ValidationMessage, "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
