using System.Runtime.InteropServices;

namespace SWTSharp.Platform;

/// <summary>
/// macOS platform implementation - Combo Box widget methods.
/// </summary>
internal partial class MacOSPlatform
{
    // Combo box selectors and classes
    private IntPtr _nsComboBoxClass;
    private IntPtr _nsPopUpButtonClass;
    private IntPtr _selAddItemWithObjectValue;
    private IntPtr _selRemoveItemAtIndex;
    private IntPtr _selRemoveAllItems;
    private IntPtr _selSelectItemAtIndex;
    private IntPtr _selIndexOfSelectedItem;
    private IntPtr _selNumberOfItems;
    private IntPtr _selItemObjectValueAtIndex;
    private IntPtr _selCompletes;
    private IntPtr _selSetCompletes;

    private void InitializeComboSelectors()
    {
        if (_nsComboBoxClass == IntPtr.Zero)
        {
            _nsComboBoxClass = objc_getClass("NSComboBox");
            _nsPopUpButtonClass = objc_getClass("NSPopUpButton");
            _selAddItemWithObjectValue = sel_registerName("addItemWithObjectValue:");
            _selRemoveItemAtIndex = sel_registerName("removeItemAtIndex:");
            _selRemoveAllItems = sel_registerName("removeAllItems");
            _selSelectItemAtIndex = sel_registerName("selectItemAtIndex:");
            _selIndexOfSelectedItem = sel_registerName("indexOfSelectedItem");
            _selNumberOfItems = sel_registerName("numberOfItems");
            _selItemObjectValueAtIndex = sel_registerName("itemObjectValueAtIndex:");
            _selStringValue = sel_registerName("stringValue");
            _selSetStringValue = sel_registerName("setStringValue:");
            _selCompletes = sel_registerName("completes");
            _selSetCompletes = sel_registerName("setCompletes:");
        }
    }

    // REMOVED METHODS (moved to IComboWidget interface):
    // - CreateCombo(IntPtr parentHandle, int style)
    // - SetComboText(IntPtr handle, string text)
    // - GetComboText(IntPtr handle)
    // - AddComboItem(IntPtr handle, string item, int index)
    // - RemoveComboItem(IntPtr handle, int index)
    // - ClearComboItems(IntPtr handle)
    // - SetComboSelection(IntPtr handle, int index)
    // - GetComboSelection(IntPtr handle)
    // - SetComboTextLimit(IntPtr handle, int limit)
    // - SetComboVisibleItemCount(IntPtr handle, int count)
    // - SetComboTextSelection(IntPtr handle, int start, int end)
    // - GetComboTextSelection(IntPtr handle)
    // - ComboTextCopy(IntPtr handle)
    // - ComboTextCut(IntPtr handle)
    // - ComboTextPaste(IntPtr handle)
    // These methods are now implemented via the IComboWidget interface using proper handles
}
