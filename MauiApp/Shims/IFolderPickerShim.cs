namespace WorkMood.MauiApp.Shims;

public interface IFolderPickerShim
{
    /// <summary>
    /// Opens a folder picker dialog. Returns the selected absolute folder path,
    /// or null if the user cancelled the dialog.
    /// </summary>
    Task<string?> PickFolderAsync();
}
