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

    // Combo control operations
    public IntPtr CreateCombo(IntPtr parentHandle, int style)
    {
        if (_selAlloc == IntPtr.Zero)
        {
            Initialize();
        }
        InitializeComboSelectors();

        IntPtr combo;

        if ((style & SWT.READ_ONLY) != 0)
        {
            // Use NSPopUpButton for read-only combo
            combo = objc_msgSend(_nsPopUpButtonClass, _selAlloc);
            combo = objc_msgSend(combo, _selInit);

            // Set button type
            IntPtr selSetPullsDown = sel_registerName("setPullsDown:");
            objc_msgSend_void(combo, selSetPullsDown, false); // Drop-down, not pull-down
        }
        else
        {
            // Use NSComboBox for editable combo
            combo = objc_msgSend(_nsComboBoxClass, _selAlloc);
            combo = objc_msgSend(combo, _selInit);
        }

        // Set default frame
        var frame = new CGRect(0, 0, 150, 25);
        objc_msgSend_rect(combo, _selSetFrame, frame);

        // Add to parent if provided
        AddChildToParent(parentHandle, combo);

        return combo;
    }

    public void SetComboText(IntPtr handle, string text)
    {
        if (handle == IntPtr.Zero) return;

        InitializeComboSelectors();
        IntPtr nsText = CreateNSString(text ?? string.Empty);
        objc_msgSend(handle, _selSetStringValue, nsText);
    }

    public string GetComboText(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return string.Empty;

        InitializeComboSelectors();
        IntPtr nsString = objc_msgSend(handle, _selStringValue);
        return GetNSStringValue(nsString);
    }

    public void AddComboItem(IntPtr handle, string item, int index)
    {
        if (handle == IntPtr.Zero) return;

        InitializeComboSelectors();
        IntPtr nsItem = CreateNSString(item ?? string.Empty);

        if (index < 0)
        {
            // Append to end
            objc_msgSend(handle, _selAddItemWithObjectValue, nsItem);
        }
        else
        {
            // Insert at specific index
            IntPtr selInsertItemAtIndex = sel_registerName("insertItemWithObjectValue:atIndex:");
            objc_msgSend(handle, selInsertItemAtIndex, nsItem, (IntPtr)index);
        }
    }

    public void RemoveComboItem(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero || index < 0) return;

        InitializeComboSelectors();
        objc_msgSend(handle, _selRemoveItemAtIndex, (IntPtr)index);
    }

    public void ClearComboItems(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        InitializeComboSelectors();
        objc_msgSend_void(handle, _selRemoveAllItems);
    }

    public void SetComboSelection(IntPtr handle, int index)
    {
        if (handle == IntPtr.Zero) return;

        InitializeComboSelectors();
        objc_msgSend(handle, _selSelectItemAtIndex, (IntPtr)index);
    }

    public int GetComboSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return -1;

        InitializeComboSelectors();
        IntPtr result = objc_msgSend(handle, _selIndexOfSelectedItem);
        return (int)result.ToInt64();
    }

    public void SetComboTextLimit(IntPtr handle, int limit)
    {
        if (handle == IntPtr.Zero) return;

        // NSComboBox doesn't have a built-in text limit
        // This would require a delegate implementation
        // For now, this is a no-op
    }

    public void SetComboVisibleItemCount(IntPtr handle, int count)
    {
        if (handle == IntPtr.Zero || count < 1) return;

        // Set the number of visible items in the drop-down
        IntPtr selSetNumberOfVisibleItems = sel_registerName("setNumberOfVisibleItems:");
        objc_msgSend(handle, selSetNumberOfVisibleItems, (IntPtr)count);
    }

    public void SetComboTextSelection(IntPtr handle, int start, int end)
    {
        if (handle == IntPtr.Zero) return;

        // Get current editor (for NSComboBox)
        IntPtr selCurrentEditor = sel_registerName("currentEditor");
        IntPtr editor = objc_msgSend(handle, selCurrentEditor);

        if (editor != IntPtr.Zero)
        {
            var range = new NSRange(start, end - start);
            unsafe
            {
                IntPtr rangePtr = (IntPtr)(&range);
                IntPtr selSetSelectedRange = sel_registerName("setSelectedRange:");
                objc_msgSend(editor, selSetSelectedRange, rangePtr);
            }
        }
    }

    public (int Start, int End) GetComboTextSelection(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return (0, 0);

        // Get current editor (for NSComboBox)
        IntPtr selCurrentEditor = sel_registerName("currentEditor");
        IntPtr editor = objc_msgSend(handle, selCurrentEditor);

        if (editor != IntPtr.Zero)
        {
            NSRange range;
            unsafe
            {
                IntPtr selSelectedRange = sel_registerName("selectedRange");
                objc_msgSend_stret(out range, editor, selSelectedRange);
            }

            return ((int)range.location, (int)(range.location + range.length));
        }

        return (0, 0);
    }

    public void ComboTextCopy(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        // Get current editor
        IntPtr selCurrentEditor = sel_registerName("currentEditor");
        IntPtr editor = objc_msgSend(handle, selCurrentEditor);

        if (editor != IntPtr.Zero)
        {
            IntPtr selCopy = sel_registerName("copy:");
            objc_msgSend(editor, selCopy, IntPtr.Zero);
        }
    }

    public void ComboTextCut(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        // Get current editor
        IntPtr selCurrentEditor = sel_registerName("currentEditor");
        IntPtr editor = objc_msgSend(handle, selCurrentEditor);

        if (editor != IntPtr.Zero)
        {
            IntPtr selCut = sel_registerName("cut:");
            objc_msgSend(editor, selCut, IntPtr.Zero);
        }
    }

    public void ComboTextPaste(IntPtr handle)
    {
        if (handle == IntPtr.Zero) return;

        // Get current editor
        IntPtr selCurrentEditor = sel_registerName("currentEditor");
        IntPtr editor = objc_msgSend(handle, selCurrentEditor);

        if (editor != IntPtr.Zero)
        {
            IntPtr selPaste = sel_registerName("paste:");
            objc_msgSend(editor, selPaste, IntPtr.Zero);
        }
    }
}
