menustack
=========

A nifty dynamically loaded menu, one menu item per file, useful for docking onto your taskbar for easy access to lots of applications/shortcuts.

Here is a screenshot running from the Taskbar.
![Sample #2](https://github.com/kodybrown/menustack/blob/master/samples/sample2.jpg?raw=true)

Here is a screenshot showing the folder contents and double-clicking the shortcut from the Desktop.
![Sample #1](https://github.com/kodybrown/menustack/blob/master/samples/sample1.jpg?raw=true)

Shortcut's Target property used in screenshots:

    %bin%\MenuStack.exe --f *.lnk "C:\Users\Kody\Desktop\Sample Stack Folder"

Here is the usage (displayed by typing 'menustack /?')

    USAGE:

      menustack.exe [options] [path]

    OPTIONS:

      path           The full path to the folder containing files to be displayed in the menu. If not specified
                     it will use the current / working directory.

      /sub           Include sub-folders. Press and hold the Ctrl key during start to force `/sub`.

      /key:X         Used in sorting. the full filename is used to sort, when key is specified, everything
                     before and including the key is removed before displaying in the menu.
                     For instance when --key is ']' the following files:
                       '02] file1.pdf', '01] file2.pdf'
                     they will be displayed like:
                       'file2.pdf', 'file1.pdf'

      /file:ptrn     Specifies which files will be displayed in the menu. The default is `*.*`.
                     You can also combine patterns by using the `;` character.
                     For example: /file=*.sln, /file=*.pdf;*.epub, /file=form*.txt

      /combine       Combine and sort folders and files together. The default behavior is not combined.)

      /ext           Show file extensions. The default behavior is to hide the file extensions.)

      /search        Show the search textbox. The default behavior is to show the search box.)

    NOTES:

     * The '-', '--', and '/' option prefixes can be used interchangeably.

     * Prefix options with a '!' to indicate the opposite value (option prefix not necessary).
       For example use `!combine` to not combine.

     * Surround file pattern option with '/' (begining and end) to indicate
       the file pattern is a regular expression.
       For instance: `--file /.*\.min$` would only include files that
       end in `.js` and not include `*.min.js

     * Boolean options (options start with a /) can include an equal sign with
       true or false (without spaces). for instance: /pause=false or /v=true, etc.

     * You can also prefix a ! in front of boolean options to indicate false.
       This overrides all other values for that option. for instance: /!search, /!sub.

